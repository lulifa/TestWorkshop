namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;

/// <summary>
/// 后台任务：定期扫描设备心跳状态，按断线时长判定通信报警并逐级升级通知。
/// 断线时长 = 当前时间 - 最近一次心跳时间；超过阈值即认为通信异常。
/// </summary>
public class WorkshopDeviceHeartbeatMonitorWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopDeviceHeartbeatMonitorWorker> _logger;
    private readonly int _level1Minutes;
    private readonly int _level2Minutes;
    private readonly int _level3Minutes;

    public WorkshopDeviceHeartbeatMonitorWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopDeviceHeartbeatMonitorWorker> logger,
        IOptions<WorkshopHeartbeatOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var value = options.Value;
        value.Validate();
        _level1Minutes = value.Level1Minutes;
        _level2Minutes = value.Level2Minutes;
        _level3Minutes = value.Level3Minutes;

        Timer.Period = (int)TimeSpan.FromMinutes(value.CheckIntervalMinutes).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var heartbeatRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopDeviceHeartbeatRepository>();
            var notifier =
                scope.ServiceProvider.GetRequiredService<IWorkshopDeviceHeartbeatNotificationSender>();

            var heartbeats = await heartbeatRepository.GetListAsync(includeDetails: false, cancellationToken: ct);
            if (heartbeats.Count == 0)
            {
                return;
            }

            var now = clock.Now;
            foreach (var heartbeat in heartbeats)
            {
                var elapsedMinutes = (now - heartbeat.LastHeartbeatAt).TotalMinutes;
                var targetLevel = elapsedMinutes >= _level3Minutes
                    ? 3
                    : elapsedMinutes >= _level2Minutes
                        ? 2
                        : elapsedMinutes >= _level1Minutes
                            ? 1
                            : 0;

                if (targetLevel > heartbeat.AlarmLevel)
                {
                    // 只在级别上升时发一次，避免同一条离线每分钟重复通知。
                    await notifier.NotifyCommunicationAlarmAsync(heartbeat, targetLevel);
                    heartbeat.RaiseAlarmLevel(targetLevel, now);
                    await heartbeatRepository.UpdateAsync(heartbeat, autoSave: true, ct);
                }
                else if (targetLevel == 0 && heartbeat.AlarmLevel > 0)
                {
                    heartbeat.ResetAlarm();
                    await heartbeatRepository.UpdateAsync(heartbeat, autoSave: true, ct);
                    _logger.LogInformation(
                        "设备 {DeviceCode} 心跳恢复，已复位报警级别",
                        heartbeat.DeviceCode);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设备心跳监控 Worker 执行异常，Worker 将继续运行");
        }
    }
}
