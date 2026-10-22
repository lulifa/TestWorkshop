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
    private readonly IWorkshopDeviceHeartbeatRepository _heartbeatRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IClock _clock;
    private readonly WorkshopAlarmOptions _alarmOptions;
    private readonly ILogger<WorkshopRealtimeDataAppService> _logger;

    public WorkshopRealtimeDataAppService(
        IWorkshopRealtimeDataRepository realtimeDataRepository,
        IWorkshopDeviceRepository deviceRepository,
        IWorkshopDeviceHeartbeatRepository heartbeatRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IClock clock,
        IOptions<WorkshopAlarmOptions> alarmOptions,
        ILogger<WorkshopRealtimeDataAppService> logger)
    {
        _realtimeDataRepository = realtimeDataRepository;
        _deviceRepository = deviceRepository;
        _heartbeatRepository = heartbeatRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _clock = clock;
        _alarmOptions = alarmOptions.Value;
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

    [Authorize]
    public virtual async Task<WorkshopRealtimeDashboardDto> GetDashboardAsync()
    {
        var now = _clock.Now;
        var timeout = TimeSpan.FromSeconds(_alarmOptions.CommunicationTimeoutSeconds);
        var onlineThreshold = now - timeout;

        var heartbeats = await _heartbeatRepository.GetListAsync(
            includeDetails: false,
            cancellationToken: default);
        var onlineHeartbeats = heartbeats
            .Where(x => x.LastHeartbeatAt > onlineThreshold)
            .GroupBy(x => x.DeviceId)
            .Select(group => group.OrderByDescending(x => x.LastHeartbeatAt).First())
            .ToList();
        var onlineDeviceIds = onlineHeartbeats
            .Select(x => x.DeviceId)
            .ToHashSet();

        if (onlineDeviceIds.Count == 0)
        {
            return new WorkshopRealtimeDashboardDto
            {
                GeneratedAt = DateTime.SpecifyKind(now, DateTimeKind.Unspecified),
                OnlineTimeoutSeconds = _alarmOptions.CommunicationTimeoutSeconds
            };
        }

        var devices = (await _deviceRepository.GetListAsync(
                includeDetails: false,
                cancellationToken: default))
            .Where(x => onlineDeviceIds.Contains(x.Id))
            .ToList();

        if (devices.Count == 0)
        {
            return new WorkshopRealtimeDashboardDto
            {
                GeneratedAt = DateTime.SpecifyKind(now, DateTimeKind.Unspecified),
                OnlineTimeoutSeconds = _alarmOptions.CommunicationTimeoutSeconds
            };
        }

        var deviceIds = devices.Select(x => x.Id).ToHashSet();
        var latestData = (await _realtimeDataRepository.GetLatestPerDeviceAsync())
            .Where(x => deviceIds.Contains(x.DeviceId))
            .ToDictionary(x => x.DeviceId);
        var organizationUnits = await _organizationUnitRepository.GetListAsync(false);
        var organizationUnitNames = organizationUnits.ToDictionary(
            x => x.Id,
            x => x.DisplayName);
        var heartbeatMap = onlineHeartbeats
            .Where(x => deviceIds.Contains(x.DeviceId))
            .ToDictionary(x => x.DeviceId);

        var items = devices
            .Select(device =>
            {
                var heartbeat = heartbeatMap[device.Id];
                latestData.TryGetValue(device.Id, out var latest);
                var elapsedSeconds = Math.Max(
                    0,
                    (long)Math.Floor((now - heartbeat.LastHeartbeatAt).TotalSeconds));
                return new WorkshopRealtimeDeviceDto
                {
                    DeviceId = device.Id,
                    DeviceCode = device.Code,
                    DeviceName = device.Name,
                    DeviceType = device.Type,
                    DeviceTypeName = device.Type.ToString(),
                    OrganizationUnitId = device.OrganizationUnitId,
                    OrganizationUnitName = organizationUnitNames.GetValueOrDefault(
                        device.OrganizationUnitId),
                    Pressure = latest?.Pressure,
                    Flow = latest?.Flow,
                    Temperature = latest?.Temperature,
                    OnlineDurationSeconds = heartbeat.TotalRuntimeSeconds + elapsedSeconds,
                    LastHeartbeatAt = heartbeat.LastHeartbeatAt,
                    DataTimestamp = latest?.Timestamp
                };
            })
            .OrderBy(x => x.OrganizationUnitName)
            .ThenBy(x => x.DeviceCode)
            .ToList();

        return new WorkshopRealtimeDashboardDto
        {
            GeneratedAt = DateTime.SpecifyKind(now, DateTimeKind.Unspecified),
            OnlineTimeoutSeconds = _alarmOptions.CommunicationTimeoutSeconds,
            Devices = items
        };
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
