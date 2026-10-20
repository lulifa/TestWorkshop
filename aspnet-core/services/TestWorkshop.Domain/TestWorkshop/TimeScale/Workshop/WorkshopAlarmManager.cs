namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警管理实现。所有报警类型共用同一套去重/恢复逻辑。
/// </summary>
public class WorkshopAlarmManager : DomainService, IWorkshopAlarmManager
{
    private readonly IWorkshopAlarmRepository _alarmRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<WorkshopAlarmManager> _logger;

    public WorkshopAlarmManager(
        IWorkshopAlarmRepository alarmRepository,
        IGuidGenerator guidGenerator,
        ILogger<WorkshopAlarmManager> logger)
    {
        _alarmRepository = alarmRepository;
        _guidGenerator = guidGenerator;
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

        _logger.LogInformation(
            "报警已恢复 {AlarmType}/{DeviceCode}/{DataItemType}",
            alarm.AlarmType,
            alarm.DeviceCode,
            alarm.DataItemType);
    }

}
