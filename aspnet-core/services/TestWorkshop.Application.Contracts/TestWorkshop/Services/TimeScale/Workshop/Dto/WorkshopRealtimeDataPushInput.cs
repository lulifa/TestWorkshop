namespace TestWorkshop;

/// <summary>
/// 下位机推送一帧测试台实时数据
/// </summary>
public class WorkshopRealtimeDataPushInput
{
    /// <summary>
    /// 测试台设备编码
    /// </summary>
    [Required]
    [StringLength(64)]
    public string DeviceCode { get; set; }

    /// <summary>
    /// 采集时间（下位机给）
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 物理量：key = 下位机上报的类型字符串（按约定，入口会归一化成规范编码），value = 数值。
    /// 采集不到的物理量不传即可。
    /// </summary>
    public Dictionary<string, decimal?> Values { get; set; } = new();

    /// <summary>
    /// 测试台急停：true=急停，不传/false=正常
    /// </summary>
    public bool? EmergencyStop { get; set; }
}
