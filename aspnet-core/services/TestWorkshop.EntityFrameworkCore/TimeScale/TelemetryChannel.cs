using System.Threading.Channels;

namespace TestWorkshop.EntityFrameworkCore;

public class TelemetryChannel
{

    public Channel<List<DeviceTelemetryDto>> Queue { get; }

    public TelemetryChannel()
    {
        Queue = Channel.CreateBounded<List<DeviceTelemetryDto>>(
            new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
    }

}

// DTO 用于在队列中传输解析后的数据
public class DeviceTelemetryDto
{
    public string DeviceCode { get; set; } // 下位机传过来的 Code
    public string Metric { get; set; }     // pressure / temp / flow
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}
