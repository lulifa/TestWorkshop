namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试台实时数据（超级表）。
/// 一行 = 一台测试台一次推送，多个物理量是同一行的多列；采集不到的物理量为空。
/// 注意：这是"测试台"的数据，跟图谱数据（被测设备）不是一回事。
/// </summary>
public class WorkshopRealtimeData
{
    /// <summary>
    /// 测试台设备ID（关联 WorkshopDevice.Id）
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 采集时间（下位机给），超级表分区列
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 测试台编码（冗余，便于看数据）
    /// </summary>
    public string DeviceCode { get; set; }

    /// <summary>
    /// 压力
    /// </summary>
    public decimal? Pressure { get; set; }

    /// <summary>
    /// 流量
    /// </summary>
    public decimal? Flow { get; set; }

    /// <summary>
    /// 温度
    /// </summary>
    public decimal? Temperature { get; set; }

    /// <summary>
    /// 扭矩
    /// </summary>
    public decimal? Torque { get; set; }

    /// <summary>
    /// 振动
    /// </summary>
    public decimal? Vibration { get; set; }

    /// <summary>
    /// 测试台急停：true=急停，false/null=正常
    /// </summary>
    public bool? EmergencyStop { get; set; }

    /// <summary>
    /// 服务器接收时间（排查下位机时钟漂移、迟到/补传）
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// 按物理量编码取这一帧的值；该物理量采集不到时返回 null
    /// </summary>
    public decimal? GetValue(string dataItemType)
    {
        return dataItemType switch
        {
            TelemetryQuantity.Pressure => Pressure,
            TelemetryQuantity.Flow => Flow,
            TelemetryQuantity.Temperature => Temperature,
            TelemetryQuantity.Torque => Torque,
            TelemetryQuantity.Vibration => Vibration,
            _ => null
        };
    }
}
