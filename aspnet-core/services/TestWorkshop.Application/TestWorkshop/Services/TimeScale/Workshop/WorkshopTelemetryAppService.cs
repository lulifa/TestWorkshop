using System.Reflection;

using Volo.Abp.Auditing;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace TestWorkshop;

/// <summary>
/// 遥测服务应用
/// </summary>
[Authorize(Roles = RoleConstants.admin)]
public class WorkshopTelemetryAppService : TestWorkshopAppService, IWorkshopTelemetryAppService
{
    private readonly IWorkshopTelemetryTaskManager _taskManager;
    private readonly IWorkshopTelemetryTaskRepository _taskRepository;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly IWorkshopDeviceRepository _workshopDeviceRepository;
    private readonly IWorkshopDeviceHeartbeatRepository _heartbeatRepository;
    private readonly IWorkshopOrderManager _orderManager;
    private readonly IWorkshopOrderRepository _orderRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly WorkshopAlarmOptions _alarmOptions;

    public WorkshopTelemetryAppService(
        IWorkshopTelemetryTaskManager taskManager,
        IWorkshopTelemetryTaskRepository taskRepository,
        IFileObjectRepository fileObjectRepository,
        IWorkshopDeviceRepository workshopDeviceRepository,
        IWorkshopDeviceHeartbeatRepository heartbeatRepository,
        IWorkshopOrderManager orderManager,
        IWorkshopOrderRepository orderRepository,
        ICurrentTenant currentTenant,
        IClock clock,
        IGuidGenerator guidGenerator,
        IOptions<WorkshopAlarmOptions> alarmOptions)
    {
        _taskManager = taskManager;
        _taskRepository = taskRepository;
        _fileObjectRepository = fileObjectRepository;
        _workshopDeviceRepository = workshopDeviceRepository;
        _heartbeatRepository = heartbeatRepository;
        _orderManager = orderManager;
        _orderRepository = orderRepository;
        _currentTenant = currentTenant;
        _clock = clock;
        _guidGenerator = guidGenerator;
        _alarmOptions = alarmOptions.Value;
    }

    /// <summary>
    /// 下位机心跳：按设备编码登记在线状态（刷新最近心跳时间），并返回服务器时间供校时。
    /// 设备编码未知时直接报错，避免把无效请求登记为在线。
    /// </summary>
    [AllowAnonymous]
    [DisableAuditing]
    public virtual async Task<TelemetryOnlineResultDto> OnlineAsync(string deviceCode)
    {
        if (deviceCode.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("设备编码不能为空");
        }

        var device = await _workshopDeviceRepository.FindByCodeAsync(deviceCode.Trim());
        if (device == null)
        {
            throw new UserFriendlyException($"未知设备编码：{deviceCode}");
        }

        var now = _clock.Now;

        // 设备编码以 WorkshopDevice.Code 为准，避免大小写/空格不一致导致重复行。
        var heartbeat = await _heartbeatRepository.FindByDeviceCodeAsync(device.Code);
        if (heartbeat == null)
        {
            heartbeat = new WorkshopDeviceHeartbeat(_guidGenerator.Create(), device.Id, device.Code, now);
            await _heartbeatRepository.InsertAsync(heartbeat);
        }
        else
        {
            heartbeat.RecordHeartbeat(
                now,
                TimeSpan.FromSeconds(_alarmOptions.CommunicationTimeoutSeconds));
            await _heartbeatRepository.UpdateAsync(heartbeat);
        }

        return new TelemetryOnlineResultDto
        {
            Online = true,
            ServerTime = DateTime.SpecifyKind(now, DateTimeKind.Unspecified),
            Timestamp = new DateTimeOffset(now.ToUniversalTime()).ToUnixTimeMilliseconds()
        };
    }

    /// <summary>
    /// 上传遥测文件
    /// </summary>
    [AllowAnonymous]
    public async Task<WorkshopTelemetryTaskDto> UploadAsync(IFormFile file, WorkshopTelemetryFileInput input)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("请选择有效的文件");
        if (input == null)
            throw new UserFriendlyException("请提供遥测文件参数");

        NormalizeTelemetryInput(input);

        if (input.DeviceCode.IsNullOrWhiteSpace())
            throw new UserFriendlyException("设备编码不能为空");
        if (input.ChannelType.IsNullOrWhiteSpace())
            throw new UserFriendlyException("通道类型不能为空");
        if (input.FileTime.IsNullOrWhiteSpace())
            throw new UserFriendlyException("文件生成时间不能为空");
        if (input.TestInfo == null)
            throw new UserFriendlyException("测试信息不能为空");

        var device = await _workshopDeviceRepository.FindByCodeAsync(input.DeviceCode);
        if (device == null)
            throw new UserFriendlyException($"设备编码不存在: {input.DeviceCode}");

        var (dataStartTime, dataEndTime) = ResolveTelemetryTimeRange(input);
        var order = await _orderManager.ResolveTelemetryOrderAsync(
            device.Id,
            dataStartTime,
            dataEndTime);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv")
            throw new UserFriendlyException("仅支持 .csv 文件");

        var extraProperties = BuildTelemetryExtraProperties(input);
        var task = await _taskManager.CreateTaskFromFileAsync(
            stream: file.OpenReadStream(),
            fileName: file.FileName,
            contentType: file.ContentType,
            orderId: order?.Id,
            extraProperties: extraProperties
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        var fileObject = await _fileObjectRepository.GetAsync(task.FileObjectId);

        var dto = new WorkshopTelemetryTaskDto
        {
            Id = task.Id,
            FileObjectId = task.FileObjectId,
            OrderId = order?.Id,
            OrderNo = order?.OrderNo,
            FileName = fileObject?.FileName,
            FileSize = fileObject?.FileSize ?? 0,
            Status = task.Status,
            RetryCount = task.RetryCount,
            Error = task.Error,
            RecordCount = task.RecordCount,
            CreatedAt = task.CreatedAt,
            ProcessedAt = task.ProcessedAt,
            ExpiresAt = task.ExpiresAt
        };
        FillTelemetryExtraProperties(dto, fileObject);
        return dto;
    }

    /// <summary>
    /// 获取任务详情
    /// </summary>
    public async Task<WorkshopTelemetryTaskDto> GetAsync(long id)
    {
        var (task, fileObject) = await _taskManager.GetTaskWithFileAsync(id);
        var order = task.OrderId.HasValue
            ? await _orderRepository.FindAsync(task.OrderId.Value)
            : null;

        var dto = new WorkshopTelemetryTaskDto
        {
            Id = task.Id,
            FileObjectId = task.FileObjectId,
            OrderId = task.OrderId,
            OrderNo = order?.OrderNo,
            ReplacedByTaskId = task.ReplacedByTaskId,
            FileName = fileObject?.FileName,
            FileSize = fileObject?.FileSize ?? 0,
            Status = task.Status,
            RetryCount = task.RetryCount,
            Error = task.Error,
            RecordCount = task.RecordCount,
            CreatedAt = task.CreatedAt,
            ProcessedAt = task.ProcessedAt,
            ExpiresAt = task.ExpiresAt
        };
        FillTelemetryExtraProperties(dto, fileObject);
        return dto;
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetListAsync(WorkshopTelemetryTaskListInput input)
    {
        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        // ✅ 用 Repository 已有的方法
        var result = await _taskRepository.GetPagedListAsync(
            input.OrderId,
            input.OrderNo,
            input.FileName,
            input.Status,
            input.StartTime,
            input.EndTime,
            input.TestedDeviceCode,
            input.HasOrder,
            input.OrganizationUnitId,
            input.SkipCount,
            input.MaxResultCount);

        var orderIds = result.Items
            .Where(x => x.OrderId.HasValue)
            .Select(x => x.OrderId.Value)
            .Distinct()
            .ToList();
        var orders = orderIds.Count == 0
            ? new List<WorkshopOrder>()
            : await _orderRepository.GetListAsync(x => orderIds.Contains(x.Id));
        var orderMap = orders.ToDictionary(x => x.Id);

        var dtos = new List<WorkshopTelemetryTaskDto>();
        foreach (var task in result.Items)
        {
            var fileObject = await _fileObjectRepository.FindAsync(task.FileObjectId);
            var order = task.OrderId.HasValue
                ? orderMap.GetValueOrDefault(task.OrderId.Value)
                : null;
            var dto = new WorkshopTelemetryTaskDto
            {
                Id = task.Id,
                FileObjectId = task.FileObjectId,
                OrderId = task.OrderId,
                OrderNo = order?.OrderNo,
                ReplacedByTaskId = task.ReplacedByTaskId,
                FileName = fileObject?.FileName,
                FileSize = fileObject?.FileSize ?? 0,
                Status = task.Status,
                RetryCount = task.RetryCount,
                Error = task.Error,
                RecordCount = task.RecordCount,
                CreatedAt = task.CreatedAt,
                ProcessedAt = task.ProcessedAt,
                ExpiresAt = task.ExpiresAt
            };
            FillTelemetryExtraProperties(dto, fileObject);
            dtos.Add(dto);
        }

        return new PagedResultDto<WorkshopTelemetryTaskDto>(result.TotalCount, dtos);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public async Task<WorkshopTelemetryStatisticsDto> GetStatisticsAsync(WorkshopTelemetryTaskListInput input)
    {
        var (totalFiles, totalSize, pendingCount, processingCount, successCount, failedCount, replacedCount, unmatchedOrderCount, totalRecords)
            = await _taskRepository.GetStatisticsDataAsync(
                input.OrderId,
                input.OrderNo,
                input.FileName,
                input.Status,
                input.StartTime,
                input.EndTime,
                input.TestedDeviceCode,
                input.HasOrder,
                input.OrganizationUnitId);

        return new WorkshopTelemetryStatisticsDto
        {
            TotalFiles = totalFiles,
            TotalSize = totalSize,
            PendingCount = pendingCount,
            ProcessingCount = processingCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
            ReplacedCount = replacedCount,
            UnmatchedOrderCount = unmatchedOrderCount,
            TotalRecords = totalRecords
        };
    }

    /// <summary>
    /// 删除任务（级联删除 FileObject 和物理文件）
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        await _taskManager.DeleteTaskAsync(id);
    }

    /// <summary>
    /// 批量删除任务（级联删除 FileObject 和物理文件）
    /// </summary>
    public async Task DeleteManyAsync(WorkshopTelemetryBatchDeleteInput input)
    {
        if (input?.Ids == null || input.Ids.Count == 0)
        {
            return;
        }

        await _taskManager.DeleteTasksAsync(input.Ids);
    }

    /// <summary>
    /// 重新处理失败的任务
    /// </summary>
    public async Task RetryAsync(long id)
    {
        await _taskManager.RetryTaskAsync(id);
    }

    /// <summary>
    /// 把上传参数扁平化写入 FileObject.ExtraProperties，后续新增简单字段无需调整持久化代码。
    /// </summary>
    private static ExtraPropertyDictionary BuildTelemetryExtraProperties(object input)
    {
        var result = new ExtraPropertyDictionary();
        AddTelemetryExtraProperties(result, input, TestWorkshopConsts.TelemetryInputExtraPropertiesPrefix);
        return result;
    }

    private static void NormalizeTelemetryInput(WorkshopTelemetryFileInput input)
    {
        input.ChannelType = input.ChannelType?.Trim();
        input.CurrentFile = input.CurrentFile?.Trim();
        input.DeviceCode = input.DeviceCode?.Trim();
        input.FileTime = input.FileTime?.Trim();
        input.Group = input.Group?.Trim();
        input.IfSource = input.IfSource?.Trim();
        input.Source = input.Source?.Trim();

        if (input.TestInfo == null)
        {
            return;
        }

        input.TestInfo.PilotSN = input.TestInfo.PilotSN?.Trim();
        input.TestInfo.ShipName = input.TestInfo.ShipName?.Trim();
        input.TestInfo.StartTime = input.TestInfo.StartTime?.Trim();
        input.TestInfo.EndTime = input.TestInfo.EndTime?.Trim();
        input.TestInfo.StartTime2 = input.TestInfo.StartTime2?.Trim();
        input.TestInfo.EndTime2 = input.TestInfo.EndTime2?.Trim();
        input.TestInfo.TestBy = input.TestInfo.TestBy?.Trim();
        input.TestInfo.TestDate = input.TestInfo.TestDate?.Trim();
        input.TestInfo.TestedDeviceCode = input.TestInfo.TestedDeviceCode?.Trim();
        input.TestInfo.TestedDeviceName = input.TestInfo.TestedDeviceName?.Trim();
    }

    private static (DateTime StartTime, DateTime EndTime) ResolveTelemetryTimeRange(
        WorkshopTelemetryFileInput input)
    {
        var startTime = ParseTelemetryTime(
            input.TestInfo.StartTime.IsNullOrWhiteSpace()
                ? input.FileTime
                : input.TestInfo.StartTime,
            "测试开始时间");

        var endTimeText = input.TestInfo.EndTime2.IsNullOrWhiteSpace()
            ? input.TestInfo.EndTime
            : input.TestInfo.EndTime2;
        var endTime = ParseTelemetryTime(
            endTimeText.IsNullOrWhiteSpace() ? input.FileTime : endTimeText,
            "测试结束时间");

        if (endTime < startTime)
        {
            throw new UserFriendlyException("测试结束时间不能早于测试开始时间");
        }

        return (startTime, endTime);
    }

    private static DateTime ParseTelemetryTime(string value, string fieldName)
    {
        if (value.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException($"{fieldName}不能为空");
        }

        if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var parsed))
        {
            throw new UserFriendlyException($"{fieldName}格式错误: {value}");
        }

        return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
    }

    private static void FillTelemetryExtraProperties(
        WorkshopTelemetryTaskDto dto,
        FileObject fileObject)
    {
        dto.ExtraProperties.Clear();
        if (fileObject == null)
        {
            return;
        }

        foreach (var item in fileObject.ExtraProperties)
        {
            dto.ExtraProperties[item.Key] = item.Value;
        }

        dto.TestedDeviceCode = fileObject.GetProperty(
            TestWorkshopConsts.TelemetryInputExtraPropertiesPrefix + "TestInfo.TestedDeviceCode") as string;
        dto.TestedDeviceName = fileObject.GetProperty(
            TestWorkshopConsts.TelemetryInputExtraPropertiesPrefix + "TestInfo.TestedDeviceName") as string;
    }

    private static void AddTelemetryExtraProperties(
        ExtraPropertyDictionary target,
        object source,
        string prefix)
    {
        if (source == null)
        {
            return;
        }

        foreach (var property in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(source);
            var key = prefix + property.Name;
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (IsSimpleValue(propertyType))
            {
                // 即使值为 null 也保留键，保证详情页能看到完整的请求参数结构。
                target[key] = value;
            }
            else if (value != null)
            {
                AddTelemetryExtraProperties(target, value, key + ".");
            }
        }
    }

    private static bool IsSimpleValue(Type type)
    {
        return type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(Guid)
               || type.IsEnum
               || type.IsPrimitive;
    }
}
