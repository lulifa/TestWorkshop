namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备遥测数据实体 - 用于存储下位机上传的实时采集数据
/// 超级表
/// </summary>
public class WorkshopDeviceTelemetry
{
    // 基础信息
    public Guid DeviceId { get; set; }
    public DateTime Timestamp { get; set; }

    // 采集指标
    public TelemetryMetricType MetricType { get; set; }
    public double Value { get; set; }

    // 被测试产品/设备信息（动态，每次不同）
    public string TestedDeviceCode { get; set; }
    public string TestedDeviceName { get; set; }

}
