using Volo.Abp.Timing;

namespace TestWorkshop;

/// <summary>
/// 测试台实时数据接收。
/// 下位机上报的物理量类型字符串在入口归一化成规范编码（见 TelemetryQuantity），
/// 认不出的类型记日志并忽略，不影响同一帧里其它已知物理量入库。
/// </summary>
public class WorkshopRealtimeDataAppService : TestWorkshopAppService, IWorkshopRealtimeDataAppService
{
    private readonly IWorkshopRealtimeDataRepository _realtimeDataRepository;
    private readonly IWorkshopDeviceRepository _deviceRepository;
    private readonly IClock _clock;
    private readonly ILogger<WorkshopRealtimeDataAppService> _logger;

    public WorkshopRealtimeDataAppService(
        IWorkshopRealtimeDataRepository realtimeDataRepository,
        IWorkshopDeviceRepository deviceRepository,
        IClock clock,
        ILogger<WorkshopRealtimeDataAppService> logger)
    {
        _realtimeDataRepository = realtimeDataRepository;
        _deviceRepository = deviceRepository;
        _clock = clock;
        _logger = logger;
    }

    [AllowAnonymous]
    public virtual async Task PushAsync(WorkshopRealtimeDataPushInput input)
    {
        if (input.DeviceCode.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("设备编码不能为空");
        }

        var device = await _deviceRepository.FindByCodeAsync(input.DeviceCode.Trim());
        if (device == null)
        {
            throw new UserFriendlyException($"未知设备编码：{input.DeviceCode}");
        }

        var data = new WorkshopRealtimeData
        {
            DeviceId = device.Id,
            DeviceCode = device.Code,
            Timestamp = ToUtc(input.Timestamp),
            EmergencyStop = input.EmergencyStop,
            // 接收时间由平台自己记，不由下位机上报
            ReceivedAt = _clock.Now
        };

        foreach (var (rawType, value) in input.Values)
        {
            var quantity = TelemetryQuantity.Find(rawType);
            if (quantity == null)
            {
                // 认不出的类型不猜，记日志后忽略（不影响其它物理量）
                _logger.LogWarning(
                    "设备 {DeviceCode} 上报了未知的物理量类型 {RawType}，已忽略",
                    device.Code,
                    rawType);
                continue;
            }

            switch (quantity.Code)
            {
                case TelemetryQuantity.Pressure:
                    data.Pressure = value;
                    break;
                case TelemetryQuantity.Flow:
                    data.Flow = value;
                    break;
                case TelemetryQuantity.Temperature:
                    data.Temperature = value;
                    break;
                case TelemetryQuantity.Torque:
                    data.Torque = value;
                    break;
                case TelemetryQuantity.Vibration:
                    data.Vibration = value;
                    break;
            }
        }

        await _realtimeDataRepository.UpsertAsync(data);
    }

    /// <summary>
    /// 下位机上报的都是本地时间。沿用现有约定：时间戳统一转成 UTC 再存。
    /// 不带时区的值不能直接入库——Npgsql 会按 UTC 处理，跟本地"当前时间"比会差整个时区，
    /// 报警的持续时间直接算错。
    /// </summary>
    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified))
        };
    }
}
