namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备心跳状态 - 每台下位机设备一行。
/// 下位机调用 online 接口时按设备编码 upsert 刷新 <see cref="LastHeartbeatAt"/>，
/// 并累计有效心跳间隔到 <see cref="TotalRuntimeSeconds"/>。
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
    /// 第一次心跳时间（服务器时间）
    /// </summary>
    public DateTime FirstHeartbeatAt { get; private set; }

    /// <summary>
    /// 累计运行秒数。相邻心跳间隔在允许范围内时才累加。
    /// </summary>
    public long TotalRuntimeSeconds { get; private set; }

    protected WorkshopDeviceHeartbeat()
    {
    }

    public WorkshopDeviceHeartbeat(Guid id, Guid deviceId, string deviceCode, DateTime lastHeartbeatAt)
        : base(id)
    {
        DeviceId = deviceId;
        DeviceCode = deviceCode;
        LastHeartbeatAt = lastHeartbeatAt;
        FirstHeartbeatAt = lastHeartbeatAt;
    }

    /// <summary>
    /// 收到心跳，刷新最近心跳时间
    /// </summary>
    public void RecordHeartbeat(DateTime heartbeatAt, TimeSpan heartbeatTimeout)
    {
        if (heartbeatAt <= LastHeartbeatAt)
        {
            return;
        }

        var gap = heartbeatAt - LastHeartbeatAt;
        if (gap < heartbeatTimeout)
        {
            TotalRuntimeSeconds += (long)Math.Round(gap.TotalSeconds);
        }

        LastHeartbeatAt = heartbeatAt;
    }
}
