namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警管理实现。所有报警类型共用同一套升级/去重/恢复逻辑。
/// </summary>
public class WorkshopAlarmManager : DomainService, IWorkshopAlarmManager
{
    private readonly IWorkshopAlarmRepository _alarmRepository;
    private readonly IWorkshopAlarmNotifier _alarmNotifier;
    private readonly IGuidGenerator _guidGenerator;
    private readonly WorkshopAlarmOptions _options;
    private readonly ILogger<WorkshopAlarmManager> _logger;

    public WorkshopAlarmManager(
        IWorkshopAlarmRepository alarmRepository,
        IWorkshopAlarmNotifier alarmNotifier,
        IGuidGenerator guidGenerator,
        IOptions<WorkshopAlarmOptions> options,
        ILogger<WorkshopAlarmManager> logger)
    {
        _alarmRepository = alarmRepository;
        _alarmNotifier = alarmNotifier;
        _guidGenerator = guidGenerator;
        _options = options.Value;
        _options.Validate();
        _logger = logger;
    }

    public virtual async Task RaiseAsync(
        WorkshopAlarmType alarmType,
        Guid deviceId,
        string deviceCode,
        Guid organizationUnitId,
        string dataItemType,
        string content,
        DateTime startedAt)
    {
        var normalizedItem = dataItemType ?? string.Empty;
        var alarm = await _alarmRepository.FindActiveAsync(alarmType, deviceId, normalizedItem);

        if (alarm == null)
        {
            alarm = new WorkshopAlarm(
                _guidGenerator.Create(),
                alarmType,
                deviceId,
                deviceCode,
                organizationUnitId,
                normalizedItem,
                content,
                startedAt);

            await _alarmRepository.InsertAsync(alarm, autoSave: true);
            return;
        }

        // 只有内容真的变了才落库，避免每分钟无意义的 UPDATE（它会刷新并发戳，跟升级流程打架）
        if (alarm.RefreshContent(content))
        {
            await _alarmRepository.UpdateAsync(alarm, autoSave: true);
        }
    }

    public virtual async Task ClearAsync(
        WorkshopAlarmType alarmType,
        Guid deviceId,
        string dataItemType,
        DateTime clearedAt)
    {
        var alarm = await _alarmRepository.FindActiveAsync(alarmType, deviceId, dataItemType ?? string.Empty);
        if (alarm == null)
        {
            return;
        }

        // 原子占用"恢复"，保证只处理一次、不跟升级流程冲突
        if (!await _alarmRepository.ClaimClearAsync(alarm.Id, clearedAt))
        {
            return;
        }

        // 目前只有通信报警发恢复消息，数据类报警的恢复状态在报警列表里看
        if (alarmType == WorkshopAlarmType.Communication)
        {
            await _alarmNotifier.NotifyRecoveredAsync(alarm, clearedAt);
        }

        _logger.LogInformation(
            "报警已恢复 {AlarmType}/{DeviceCode}/{DataItemType}",
            alarm.AlarmType,
            alarm.DeviceCode,
            alarm.DataItemType);
    }

    /// <summary>
    /// 设备数据停发兜底：没有新数据可判了，就不能让报警继续挂着升级。
    /// 只处理数据类报警（阈值/设备），通信报警要等心跳回来才恢复。
    /// </summary>
    public virtual async Task ClearStaleDataAlarmsAsync(Guid deviceId, DateTime clearedAt)
    {
        var count = await _alarmRepository.ClearActiveDataAlarmsAsync(deviceId, clearedAt);
        if (count > 0)
        {
            _logger.LogInformation(
                "设备 {DeviceId} 实时数据已停发，自动恢复 {Count} 条数据类报警",
                deviceId,
                count);
        }
    }

    public virtual async Task EvaluateEscalationAsync(DateTime now)
    {
        var alarms = await _alarmRepository.GetActiveListAsync();
        if (alarms.Count == 0)
        {
            return;
        }

        foreach (var alarm in alarms)
        {
            var elapsedMinutes = (now - alarm.StartedAt).TotalMinutes;
            var targetLevel = elapsedMinutes >= _options.Level3Minutes
                ? 3
                : elapsedMinutes >= _options.Level2Minutes
                    ? 2
                    : elapsedMinutes >= _options.Level1Minutes
                        ? 1
                        : 0;

            if (targetLevel <= alarm.Level)
            {
                continue;
            }

            // 原子占用该级别：抢到才发消息，保证"每级只发一次"且不与其他 worker 冲突
            if (!await _alarmRepository.ClaimLevelAsync(alarm.Id, targetLevel, now))
            {
                continue;
            }

            await _alarmNotifier.NotifyAsync(alarm, targetLevel, now);
        }
    }
}
