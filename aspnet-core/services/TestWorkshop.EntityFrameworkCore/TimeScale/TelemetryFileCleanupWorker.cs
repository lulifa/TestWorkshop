using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;

namespace TestWorkshop.EntityFrameworkCore;

public class TelemetryFileCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryFileCleanupWorker> _logger;

    public TelemetryFileCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetryFileCleanupWorker> logger)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        Timer.Period = 3600000; // 1 小时
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskRepo = scope.ServiceProvider.GetRequiredService<ITelemetryTaskRepository>();
        var blobContainer = scope.ServiceProvider.GetRequiredService<IBlobContainer>();

        try
        {
            // ✅ 只获取已经成功或最终失败的过期任务，避免误删正在处理或待处理的任务
            var expiredTasks = await taskRepo.GetExpiredCompletedTasksAsync();
            if (expiredTasks.Count == 0)
            {
                _logger.LogDebug("没有需要清理的过期任务");
                return;
            }

            _logger.LogInformation("发现 {Count} 个已完成/失败的过期任务，开始清理", expiredTasks.Count);

            int successCount = 0;
            long freedSize = 0;

            // 可选：控制并行删除数，避免瞬时大量网络请求
            var semaphore = new SemaphoreSlim(5);
            var tasks_list = expiredTasks.Select(async task =>
            {
                await semaphore.WaitAsync();
                try
                {
                    if (!string.IsNullOrEmpty(task.BlobName))
                    {
                        try
                        {
                            await blobContainer.DeleteAsync(task.BlobName);
                            Interlocked.Add(ref freedSize, task.FileSize);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "删除 Blob 失败 {BlobName}，继续标记任务为已删除", task.BlobName);
                        }
                    }

                    // 无论 Blob 是否删除成功，都标记任务为已删除（文件可能已不存在）
                    task.IsDeleted = true;
                    task.DeletedAt = DateTime.UtcNow;
                    Interlocked.Increment(ref successCount);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks_list);

            // 批量更新数据库
            await taskRepo.UpdateManyAsync(expiredTasks);
            _logger.LogInformation("清理完成：成功处理 {Count} 个任务，释放空间约 {Size} 字节", successCount, freedSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期任务时发生异常");
        }
    }
}