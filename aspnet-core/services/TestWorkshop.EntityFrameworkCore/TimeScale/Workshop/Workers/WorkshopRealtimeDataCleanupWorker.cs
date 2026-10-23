namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;

/// <summary>
/// 后台任务：按保留月数清理测试台实时数据超级表时间分块。
/// </summary>
public class WorkshopRealtimeDataCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopRealtimeDataCleanupWorker> _logger;
    private readonly WorkshopRealtimeDataOptions _options;

    public WorkshopRealtimeDataCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopRealtimeDataCleanupWorker> logger,
        IOptions<WorkshopRealtimeDataOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _options.Validate();

        Timer.Period = (int)TimeSpan
            .FromMinutes(_options.CleanupIntervalMinutes)
            .TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(
        PeriodicBackgroundWorkerContext workerContext)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        try
        {
            var cutoff = clock.Now.AddMonths(-_options.RetentionMonths);

            // 只删除已完整超过保留期的 TimescaleDB 分块，避免对超级表逐行删除。
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""SELECT drop_chunks('"AppWorkshopRealtimeData"', older_than => {cutoff});""");

            _logger.LogInformation(
                "实时数据清理完成：已删除 {Cutoff:O} 之前的时间分块",
                cutoff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理实时数据时发生异常，Worker 将继续运行");
        }
    }
}
