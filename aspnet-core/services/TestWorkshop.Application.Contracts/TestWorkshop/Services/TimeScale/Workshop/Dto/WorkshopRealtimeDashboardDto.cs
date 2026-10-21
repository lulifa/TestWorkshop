namespace TestWorkshop;

/// <summary>
/// 实时数据分析页数据。
/// </summary>
public class WorkshopRealtimeDashboardDto
{
    public DateTime GeneratedAt { get; set; }

    public int RefreshAfterSeconds { get; set; } = 60;

    public int OnlineTimeoutSeconds { get; set; }

    public List<WorkshopRealtimeDeviceDto> Devices { get; set; } = [];
}

/// <summary>
/// 在线设备实时指标。
/// </summary>
public class WorkshopRealtimeDeviceDto
{
    public Guid DeviceId { get; set; }

    public string DeviceCode { get; set; }

    public string DeviceName { get; set; }

    public DeviceTypeEnum DeviceType { get; set; }

    public string DeviceTypeName { get; set; }

    public Guid OrganizationUnitId { get; set; }

    public string OrganizationUnitName { get; set; }

    public decimal? Pressure { get; set; }

    public decimal? Flow { get; set; }

    public decimal? Temperature { get; set; }

    public long OnlineDurationSeconds { get; set; }

    public DateTime LastHeartbeatAt { get; set; }

    public DateTime? DataTimestamp { get; set; }
}
