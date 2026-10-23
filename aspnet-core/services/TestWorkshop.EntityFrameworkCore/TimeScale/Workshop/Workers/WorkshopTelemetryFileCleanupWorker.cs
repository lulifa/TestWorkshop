namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;

/// <summary>
/// 后台任务：定期清理已过期且已完成/失败的遥测任务，
/// 并删除超过保留期的设备遥测超级表时间分块。
/// </summary>
public class WorkshopTelemetryFileCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    // 每批最多删除 100 个任务，避免单次事务过大。
    private const int CleanupBatchSize = 100;

    // 单次最多处理 10 批，防止历史积压任务长时间占用后台线程。
    private const int MaxBatchesPerRun = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopTelemetryFileCleanupWorker> _logger;
    private readonly WorkshopTelemetryOptions _options;
    private readonly TimeSpan _cleanupInterval;

    public WorkshopTelemetryFileCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopTelemetryFileCleanupWorker> logger,
        IOptions<WorkshopTelemetryOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _options.Validate();
        _cleanupInterval = TimeSpan.FromMinutes(_options.FileCleanupIntervalMinutes);
        Timer.Period = (int)_cleanupInterval.TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskManager = scope.ServiceProvider.GetRequiredService<IWorkshopTelemetryTaskManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        await CleanupExpiredFilesAsync(taskManager);
        await CleanupExpiredDeviceTelemetryAsync(dbContext, clock.Now);
    }

    private async Task CleanupExpiredFilesAsync(
        IWorkshopTelemetryTaskManager taskManager)
    {
        try
        {
            var cleanedCount = 0;

            // 任务创建时 ExpiresAt 已按 WorkshopTelemetry:RetentionDays 写入。
            for (var batch = 0; batch < MaxBatchesPerRun; batch++)
            {
                var count = await taskManager.CleanupExpiredTasksAsync(CleanupBatchSize);
                cleanedCount += count;

                if (count < CleanupBatchSize)
                {
                    break;
                }
            }

            if (cleanedCount > 0)
            {
                _logger.LogInformation("清理完成：已清理 {Count} 个过期任务", cleanedCount);
            }
            else
            {
                _logger.LogDebug("没有需要清理的过期任务");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期任务时发生异常");
        }
    }

    private async Task CleanupExpiredDeviceTelemetryAsync(
        TestWorkshopDbContext dbContext,
        DateTime now)
    {
        try
        {
            var cutoff = now.AddDays(-_options.RetentionDays);

            // 只删除已完整超过保留期的 TimescaleDB 分块，避免对超级表逐行删除。
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""SELECT drop_chunks('"AppWorkshopDeviceTelemetries"', older_than => {cutoff});""");

            _logger.LogInformation(
                "设备遥测数据清理完成：已删除 {Cutoff:O} 之前的时间分块",
                cutoff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理设备遥测数据时发生异常");
        }
    }
}
