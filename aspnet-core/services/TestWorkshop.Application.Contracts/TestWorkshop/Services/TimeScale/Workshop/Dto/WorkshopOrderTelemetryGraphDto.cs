namespace TestWorkshop;

/// <summary>
/// 订单遥测图谱数据
/// </summary>
public class WorkshopOrderTelemetryGraphDto
{
    public Guid OrderId { get; set; }

    public string OrderNo { get; set; }

    public string DeviceCode { get; set; }

    public string DeviceName { get; set; }

    public string TestedDeviceCode { get; set; }

    public string TestedDeviceName { get; set; }

    public List<WorkshopTelemetryGraphGroupDto> Graphs { get; set; } = new();
}

/// <summary>
/// 一组图谱数据（按采集时间 Timestamp 分组）
/// </summary>
public class WorkshopTelemetryGraphGroupDto
{
    public DateTime Timestamp { get; set; }

    public List<WorkshopTelemetryGraphChannelDto> Channels { get; set; } = new();
}

/// <summary>
/// 图谱中的一条通道曲线
/// </summary>
public class WorkshopTelemetryGraphChannelDto
{
    public string ChannelType { get; set; }

    public int SampleCount { get; set; }

    public double[] Values { get; set; }
}
