namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警消息发送
/// </summary>
public interface IWorkshopAlarmNotifier
{
    /// <summary>
    /// 按级别发送报警消息（级别决定收件人范围）
    /// </summary>
    Task NotifyAsync(WorkshopAlarm alarm, int level, DateTime now);

    /// <summary>
    /// 发送恢复消息
    /// </summary>
    Task NotifyRecoveredAsync(WorkshopAlarm alarm, DateTime now);
}
