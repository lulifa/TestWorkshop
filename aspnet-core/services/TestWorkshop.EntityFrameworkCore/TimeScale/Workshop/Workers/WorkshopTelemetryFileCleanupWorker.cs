namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 后台任务：定期清理已过期且已完成/失败的遥测任务
/// </summary>
public class WorkshopTelemetryFileCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopTelemetryFileCleanupWorker> _logger;

    public WorkshopTelemetryFileCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopTelemetryFileCleanupWorker> logger)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        Timer.Period = 3600000; // 1 小时
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskManager = scope.ServiceProvider.GetRequiredService<IWorkshopTelemetryTaskManager>();

        try
        {
            var cleanedCount = await taskManager.CleanupExpiredTasksAsync(batchSize: 100);

            if (cleanedCount > 0)
            {
                _logger.LogInformation("✅ 清理完成：已清理 {Count} 个过期任务", cleanedCount);
            }
            else
            {
                _logger.LogDebug("没有需要清理的过期任务");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 清理过期任务时发生异常");
        }
    }
}