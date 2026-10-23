namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;
using Volo.Abp.MultiTenancy;

/// <summary>
/// 后台任务：清理超过保留期的订单测试报告文件。
/// </summary>
public class WorkshopTestReportCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private const int CleanupBatchSize = 100;
    private const int MaxBatchesPerRun = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopTestReportCleanupWorker> _logger;
    private readonly WorkshopTestReportOptions _options;

    public WorkshopTestReportCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopTestReportCleanupWorker> logger,
        IOptions<WorkshopTestReportOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _options.Validate();

        Timer.Period = (int)TimeSpan
            .FromMinutes(_options.FileCleanupIntervalMinutes)
            .TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(
        PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var fileObjectRepository =
                serviceProvider.GetRequiredService<IFileObjectRepository>();
            var fileObjectManager =
                serviceProvider.GetRequiredService<IFileObjectManager>();
            var orderManager =
                serviceProvider.GetRequiredService<IWorkshopOrderManager>();
            var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();
            var clock = serviceProvider.GetRequiredService<IClock>();
            var cutoff = clock.Now.AddDays(-_options.FileRetentionDays);
            var cleanedFileCount = 0;
            var expiredOrderCount = 0;

            for (var batch = 0; batch < MaxBatchesPerRun; batch++)
            {
                var expiredFiles = await fileObjectRepository
                    .GetExpiredByOwnerTypeAsync(
                        SystemFileTypes.WorkshopTestReport,
                        cutoff,
                        CleanupBatchSize,
                        ct);

                if (expiredFiles.Count == 0)
                {
                    break;
                }

                var fileGroups = expiredFiles
                    .GroupBy(file => new { file.TenantId, file.OwnerId })
                    .ToList();

                foreach (var fileGroup in fileGroups)
                {
                    var ownerId = fileGroup.Key.OwnerId;
                    if (ownerId.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    using (currentTenant.Change(fileGroup.Key.TenantId))
                    {
                        try
                        {
                            var hasUnexpiredFile = (await fileObjectRepository.GetListAsync(
                                    file => file.OwnerType == SystemFileTypes.WorkshopTestReport
                                            && file.OwnerId == ownerId
                                            && file.CreationTime >= cutoff))
                                .Count > 0;
                            if (hasUnexpiredFile)
                            {
                                continue;
                            }

                            if (Guid.TryParse(ownerId, out var orderId))
                            {
                                await orderManager.MarkTestReportExpiredAsync(orderId);
                                expiredOrderCount++;
                            }

                            await fileObjectManager.DeleteFilesAsync(
                                SystemFileTypes.WorkshopTestReport,
                                ownerId,
                                throwOnPhysicalDeleteFailure: true);
                            cleanedFileCount += fileGroup.Count();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "清理过期测试报告失败: TenantId={TenantId}, OrderId={OrderId}",
                                fileGroup.Key.TenantId,
                                ownerId);
                        }
                    }
                }

                if (expiredFiles.Count < CleanupBatchSize)
                {
                    break;
                }
            }

            if (cleanedFileCount > 0)
            {
                _logger.LogInformation(
                    "测试报告清理完成：已处理 {OrderCount} 个订单，删除 {FileCount} 个文件",
                    expiredOrderCount,
                    cleanedFileCount);
            }
            else
            {
                _logger.LogDebug("没有需要清理的过期测试报告");
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期测试报告时发生异常，Worker 将继续运行");
        }
    }
}
