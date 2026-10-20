namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;
using Volo.Abp.Uow;

/// <summary>
/// 后台任务：扫描设备心跳，把"断线 / 恢复"同步给统一的报警管理。
/// 通信报警只进入报警列表，不做人员通知。
/// </summary>
public class WorkshopDeviceHeartbeatMonitorWorker : AsyncPeriodicBackgroundWorkerBase
{
    /// <summary>
    /// 通信报警的"数据类型"为空字符串（不区分物理量）
    /// </summary>
    private const string NoDataItem = "";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopDeviceHeartbeatMonitorWorker> _logger;
    private readonly int _timeoutSeconds;

    public WorkshopDeviceHeartbeatMonitorWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopDeviceHeartbeatMonitorWorker> logger,
        IOptions<WorkshopAlarmOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var value = options.Value;
        value.Validate();
        _timeoutSeconds = value.CommunicationTimeoutSeconds;
        Timer.Period = (int)TimeSpan.FromSeconds(value.CheckIntervalSeconds).TotalMilliseconds;
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
            var heartbeatRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopDeviceHeartbeatRepository>();
            var deviceRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopDeviceRepository>();
            var alarmManager =
                scope.ServiceProvider.GetRequiredService<IWorkshopAlarmManager>();

            var heartbeats = await heartbeatRepository.GetListAsync(
                includeDetails: false,
                cancellationToken: ct);
            if (heartbeats.Count == 0)
            {
                return;
            }

            var devices = await deviceRepository.GetListAsync(
                includeDetails: false,
                cancellationToken: ct);
            var deviceMap = devices.ToDictionary(x => x.Id);

            var now = clock.Now;
            foreach (var heartbeat in heartbeats)
            {
                if (!deviceMap.TryGetValue(heartbeat.DeviceId, out var device))
                {
                    _logger.LogWarning(
                        "心跳记录 {DeviceCode} 找不到对应设备，已跳过",
                        heartbeat.DeviceCode);
                    continue;
                }

                var elapsedSeconds = (now - heartbeat.LastHeartbeatAt).TotalSeconds;
                if (elapsedSeconds >= _timeoutSeconds)
                {
                    // 条件（无心跳）从最后一次心跳那刻开始算，升级时长也从它算起
                    await alarmManager.RaiseAsync(
                        WorkshopAlarmType.Communication,
                        device.Id,
                        device.Code,
                        device.OrganizationUnitId,
                        NoDataItem,
                        "通信中断",
                        heartbeat.LastHeartbeatAt);
                }
                else
                {
                    await alarmManager.ClearAsync(
                        WorkshopAlarmType.Communication,
                        device.Id,
                        NoDataItem,
                        now);
                }
            }

            await uow.CompleteAsync(ct);
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
