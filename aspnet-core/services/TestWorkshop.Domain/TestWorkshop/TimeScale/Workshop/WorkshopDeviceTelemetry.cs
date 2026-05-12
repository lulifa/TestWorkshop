namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备遥测数据实体 - 用于存储下位机上传的实时采集数据
/// 超级表
/// </summary>
public class WorkshopDeviceTelemetry
{
    public Guid DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Metric { get; set; } // pressure / temp / flow / vibration
    public double Value { get; set; }
    
}
