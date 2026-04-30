namespace TestWorkshop.EntityFrameworkCore;

public class DeviceTelemetryDto
{
    public string DeviceCode { get; set; }

    public string Metric { get; set; }

    public double Value { get; set; }

    public DateTime Timestamp { get; set; }
}
