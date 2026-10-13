namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备通信报警消息发送
/// </summary>
public interface IWorkshopDeviceHeartbeatNotificationSender
{
    /// <summary>
    /// 按报警级别给设备所属车间相关人员发送通信报警消息。
    /// level: 1=测试工程师，2=+车间主管，3=+系统管理员。
    /// </summary>
    Task NotifyCommunicationAlarmAsync(WorkshopDeviceHeartbeat heartbeat, int level);

    /// <summary>
    /// 发送"通信已恢复"消息，收件人范围与恢复前的报警级别一致。
    /// level 传恢复前所处的报警级别。
    /// </summary>
    Task NotifyCommunicationRecoveredAsync(WorkshopDeviceHeartbeat heartbeat, int level);
}
