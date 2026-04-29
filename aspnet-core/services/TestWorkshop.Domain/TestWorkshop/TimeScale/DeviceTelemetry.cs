namespace TestWorkshop.TimeScale;

public class DeviceTelemetry
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Metric { get; set; } // pressure / temp / flow / vibration
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}
