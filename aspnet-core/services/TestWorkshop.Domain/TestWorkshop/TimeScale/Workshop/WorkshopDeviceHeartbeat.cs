namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备心跳状态 - 每台下位机设备一行。
/// 下位机调用 online 接口时按设备编码 upsert 刷新 <see cref="LastHeartbeatAt"/>；
/// 后台 Worker 依据「最近心跳时间」与当前时间的差值判定通信报警，并逐级升级通知。
/// 这里保存的是「最新状态」而非心跳明细，行数等于设备数，不会持续增长，无需清理。
/// </summary>
public class WorkshopDeviceHeartbeat : Entity<Guid>
{
    /// <summary>
    /// 关联设备ID（WorkshopDevice.Id）
    /// </summary>
    public Guid DeviceId { get; private set; }

    /// <summary>
    /// 设备编码（WorkshopDevice.Code，唯一）
    /// </summary>
    public string DeviceCode { get; private set; }

    /// <summary>
    /// 最近一次心跳时间（服务器时间）
    /// </summary>
    public DateTime LastHeartbeatAt { get; private set; }

    /// <summary>
    /// 已通知到的报警级别：0=正常，1/2/3 对应 5/10/20 分钟三级阈值。
    /// 用于避免同一条离线被每分钟重复通知，并保证只向上升级。
    /// </summary>
    public int AlarmLevel { get; private set; }

    /// <summary>
    /// 最近一次发送报警消息的时间
    /// </summary>
    public DateTime? LastAlarmAt { get; private set; }

    protected WorkshopDeviceHeartbeat()
    {
    }

    public WorkshopDeviceHeartbeat(Guid id, Guid deviceId, string deviceCode, DateTime lastHeartbeatAt)
        : base(id)
    {
        DeviceId = deviceId;
        DeviceCode = deviceCode;
        LastHeartbeatAt = lastHeartbeatAt;
        AlarmLevel = 0;
    }

    /// <summary>
    /// 收到心跳，刷新最近心跳时间
    /// </summary>
    public void RecordHeartbeat(DateTime heartbeatAt)
    {
        LastHeartbeatAt = heartbeatAt;
    }

    /// <summary>
    /// 提升已通知的报警级别
    /// </summary>
    public void RaiseAlarmLevel(int level, DateTime alarmAt)
    {
        AlarmLevel = level;
        LastAlarmAt = alarmAt;
    }

    /// <summary>
    /// 心跳恢复，复位报警级别，便于下一轮断线能重新报警
    /// </summary>
    public void ResetAlarm()
    {
        AlarmLevel = 0;
    }
}
