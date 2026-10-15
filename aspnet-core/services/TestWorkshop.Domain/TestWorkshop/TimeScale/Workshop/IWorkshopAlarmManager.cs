namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警管理：负责报警的成立、升级、恢复与去重。判定器只负责"报告状态"。
/// </summary>
public interface IWorkshopAlarmManager
{
    /// <summary>
    /// 报警成立（条件首次满足时调用）。已存在进行中的同一条报警则只刷新内容。
    /// startedAt 传"条件开始满足的时刻"，升级时长从它算起。
    /// </summary>
    Task RaiseAsync(
        WorkshopAlarmType alarmType,
        Guid deviceId,
        string deviceCode,
        Guid organizationUnitId,
        string dataItemType,
        string content,
        DateTime startedAt);

    /// <summary>
    /// 报警恢复（条件不再满足时调用）
    /// </summary>
    Task ClearAsync(WorkshopAlarmType alarmType, Guid deviceId, string dataItemType, DateTime clearedAt);

    /// <summary>
    /// 设备实时数据停发时，把它仍在进行中的数据类报警（阈值/设备）全部恢复。
    /// 这类报警没有恢复消息，所以这里只改状态、不发通知；通信报警不受影响。
    /// </summary>
    Task ClearStaleDataAlarmsAsync(Guid deviceId, DateTime clearedAt);

    /// <summary>
    /// 扫描全部进行中的报警，按持续时间逐级升级并发送通知
    /// </summary>
    Task EvaluateEscalationAsync(DateTime now);
}
