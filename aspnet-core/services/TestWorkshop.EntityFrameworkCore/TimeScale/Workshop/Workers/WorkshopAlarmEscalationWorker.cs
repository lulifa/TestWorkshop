namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;
using Volo.Abp.Uow;

/// <summary>
/// 后台任务：扫描进行中的报警，按持续时间（5/10/20 分钟）逐级升级并发送通知。
/// 所有报警类型共用这一套升级逻辑。
/// </summary>
public class WorkshopAlarmEscalationWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopAlarmEscalationWorker> _logger;

    public WorkshopAlarmEscalationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopAlarmEscalationWorker> logger,
        IOptions<WorkshopAlarmOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var value = options.Value;
        value.Validate();
        Timer.Period = (int)TimeSpan.FromMinutes(value.CheckIntervalMinutes).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            // 后台 Worker 不会自动开工作单元；仓储/DbContext 需要它
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var alarmManager = scope.ServiceProvider.GetRequiredService<IWorkshopAlarmManager>();

            await alarmManager.EvaluateEscalationAsync(clock.Now);

            await uow.CompleteAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "报警升级 Worker 执行异常，Worker 将继续运行");
        }
    }
}
