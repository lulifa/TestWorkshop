namespace TestWorkshop.EntityFrameworkCore;

using Microsoft.Extensions.Options;
using Volo.Abp.Uow;

/// <summary>
/// 后台任务：扫描每台测试台的最新一帧实时数据，判定阈值报警和设备报警。
/// 值超出阈值上下限时，立即产生报警，不做人员通知。
/// </summary>
public class WorkshopThresholdAlarmWorker : AsyncPeriodicBackgroundWorkerBase
{
    private const string NoDataItem = "";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkshopThresholdAlarmWorker> _logger;

    public WorkshopThresholdAlarmWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkshopThresholdAlarmWorker> logger,
        IOptions<WorkshopAlarmOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var value = options.Value;
        value.Validate();
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
            var realtimeDataRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopRealtimeDataRepository>();
            var deviceRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopDeviceRepository>();
            var thresholdRepository =
                scope.ServiceProvider.GetRequiredService<IWorkshopThresholdRuleRepository>();
            var alarmManager =
                scope.ServiceProvider.GetRequiredService<IWorkshopAlarmManager>();

            var frames = await realtimeDataRepository.GetLatestPerDeviceAsync(ct);
            if (frames.Count == 0)
            {
                return;
            }

            var devices = (await deviceRepository.GetListAsync(
                    includeDetails: false,
                    cancellationToken: ct))
                .ToDictionary(x => x.Id);

            var now = clock.Now;
            var rulesByDeviceType = new Dictionary<DeviceTypeEnum, List<WorkshopThresholdRule>>();

            foreach (var frame in frames)
            {
                if (!devices.TryGetValue(frame.DeviceId, out var device))
                {
                    _logger.LogWarning(
                        "实时数据找不到对应设备 {DeviceCode}，已跳过",
                        frame.DeviceCode);
                    continue;
                }

                if (!rulesByDeviceType.TryGetValue(device.Type, out var rules))
                {
                    rules = await thresholdRepository.GetListByDeviceTypeAsync(device.Type);
                    rulesByDeviceType[device.Type] = rules;
                }

                foreach (var rule in rules.Where(x => x.Enabled))
                {
                    var value = frame.GetValue(rule.DataItemType);
                    if (value == null)
                    {
                        // 该物理量这一帧没有数据，无法判定，维持现状
                        continue;
                    }

                    var exceeded =
                        (rule.UpperLimit.HasValue && value.Value > rule.UpperLimit.Value)
                        || (rule.LowerLimit.HasValue && value.Value < rule.LowerLimit.Value);

                    var quantity = TelemetryQuantity.Find(rule.DataItemType);
                    var quantityName = quantity?.Name ?? rule.DataItemType;
                    var thresholdContent = rule.AlarmContent.IsNullOrWhiteSpace()
                        ? $"{quantityName}异常"
                        : rule.AlarmContent;

                    if (exceeded)
                    {
                        // 阈值报警：所有设备类型都判（测试台与传感器设备都适用）
                        await alarmManager.RaiseAsync(
                            WorkshopAlarmType.Threshold,
                            device.Id,
                            device.Code,
                            device.OrganizationUnitId,
                            rule.DataItemType,
                            thresholdContent,
                            frame.Timestamp);

                        // 设备报警里的“传感器数值异常”只对传感器类型的设备成立；
                        // 测试台超限已经由阈值报警表达，不再重复出一条设备报警。
                        if (device.Type == DeviceTypeEnum.SENSOR)
                        {
                            await alarmManager.RaiseAsync(
                                WorkshopAlarmType.Device,
                                device.Id,
                                device.Code,
                                device.OrganizationUnitId,
                                rule.DataItemType,
                                $"{quantityName}数值异常",
                                frame.Timestamp);
                        }
                    }
                    else
                    {
                        await alarmManager.ClearAsync(
                            WorkshopAlarmType.Threshold,
                            device.Id,
                            rule.DataItemType,
                            now);

                        // 恢复不判类型：不论哪类设备，数值回到范围内就把该物理量的设备报警恢复掉。
                        // 这样历史版本为非传感器设备产生的那条报警也能被清掉，不会一直挂着继续升级。
                        await alarmManager.ClearAsync(
                            WorkshopAlarmType.Device,
                            device.Id,
                            rule.DataItemType,
                            now);
                    }
                }

                // 测试台急停（独立于物理量阈值）
                if (frame.EmergencyStop == true)
                {
                    await alarmManager.RaiseAsync(
                        WorkshopAlarmType.Device,
                        device.Id,
                        device.Code,
                        device.OrganizationUnitId,
                        NoDataItem,
                        "测试台急停",
                        frame.Timestamp);
                }
                else
                {
                    await alarmManager.ClearAsync(
                        WorkshopAlarmType.Device,
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
            _logger.LogError(ex, "阈值/设备报警判定 Worker 执行异常，Worker 将继续运行");
        }
    }
}
