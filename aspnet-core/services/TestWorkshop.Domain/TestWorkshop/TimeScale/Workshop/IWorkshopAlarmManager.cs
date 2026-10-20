namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警管理：负责报警的成立、恢复与去重。判定器只负责"报告状态"。
/// </summary>
public interface IWorkshopAlarmManager
{
    /// <summary>
    /// 报警成立（条件首次满足时调用）。已存在进行中的同一条报警则只刷新内容。
    /// startedAt 传"条件开始满足的时刻"。
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

}
