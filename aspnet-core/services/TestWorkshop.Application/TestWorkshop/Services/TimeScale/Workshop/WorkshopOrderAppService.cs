namespace TestWorkshop;

/// <summary>
/// 车间测试订单应用服务
/// </summary>
[Authorize]
public class WorkshopOrderAppService : TestWorkshopAppService, IWorkshopOrderAppService
{
    // 遥测任务状态：0=待处理 1=处理中 2=成功 3=失败
    private const int TelemetryTaskStatusSuccess = 2;

    private readonly IWorkshopOrderManager _orderManager;
    private readonly IWorkshopOrderRepository _orderRepository;
    private readonly IWorkshopDeviceRepository _deviceRepository;
    private readonly IWorkshopTelemetryTaskRepository _taskRepository;
    private readonly IWorkshopDeviceTelemetryRepository _telemetryRepository;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;

    public WorkshopOrderAppService(
        IWorkshopOrderManager orderManager,
        IWorkshopOrderRepository orderRepository,
        IWorkshopDeviceRepository deviceRepository,
        IWorkshopTelemetryTaskRepository taskRepository,
        IWorkshopDeviceTelemetryRepository telemetryRepository,
        IFileObjectRepository fileObjectRepository,
        IAsyncQueryableExecuter asyncQueryableExecuter)
    {
        _orderManager = orderManager;
        _orderRepository = orderRepository;
        _deviceRepository = deviceRepository;
        _taskRepository = taskRepository;
        _telemetryRepository = telemetryRepository;
        _fileObjectRepository = fileObjectRepository;
        _asyncQueryableExecuter = asyncQueryableExecuter;
    }

    public virtual async Task<PagedResultDto<WorkshopOrderDto>> GetListAsync(WorkshopOrderListInput input)
    {
        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var result = await _orderRepository.GetPagedListAsync(
            input.OrderNo,
            input.DeviceId,
            input.Status,
            input.StartTime,
            input.EndTime,
            input.SkipCount,
            input.MaxResultCount);

        var deviceIds = result.Items.Select(x => x.DeviceId).Distinct().ToList();
        var deviceQuery = await _deviceRepository.GetQueryableAsync();
        var devices = await _asyncQueryableExecuter.ToListAsync(
            deviceQuery.Where(x => deviceIds.Contains(x.Id)));
        var deviceMap = devices.ToDictionary(x => x.Id);

        var items = result.Items
            .Select(order => ToDto(
                order,
                deviceMap.GetValueOrDefault(order.DeviceId)))
            .ToList();

        return new PagedResultDto<WorkshopOrderDto>(result.TotalCount, items);
    }

    public virtual async Task<WorkshopOrderDto> GetAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device);
    }

    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task<WorkshopOrderDto> CreateAsync(WorkshopOrderCreateDto input)
    {
        var order = await _orderManager.CreateAsync(input.OrderNo, input.DeviceId);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device);
    }

    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task<WorkshopOrderDto> UpdateAsync(Guid id, WorkshopOrderUpdateDto input)
    {
        var order = await _orderManager.UpdateAsync(id, input.OrderNo, input.DeviceId);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device);
    }

    [Authorize(Roles = RoleConstants.admin)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _orderManager.DeleteAsync(id);
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> StartTestAsync(Guid id)
    {
        var order = await _orderManager.StartTestAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device);
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> EndTestAsync(Guid id)
    {
        var order = await _orderManager.EndTestAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device);
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor)]
    public virtual async Task<WorkshopOrderDto> CancelAsync(Guid id)
    {
        var order = await _orderManager.CancelAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device);
    }

    public virtual async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetTelemetryTasksAsync(
        Guid id,
        WorkshopTelemetryTaskListInput input)
    {
        var order = await _orderRepository.GetAsync(id);

        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var result = await _taskRepository.GetPagedListAsync(
            order.Id,
            input.FileName,
            input.Status,
            input.StartTime,
            input.EndTime,
            input.SkipCount,
            input.MaxResultCount);

        var items = new List<WorkshopTelemetryTaskDto>();
        foreach (var task in result.Items)
        {
            var fileObject = await _fileObjectRepository.FindAsync(task.FileObjectId);
            var dto = new WorkshopTelemetryTaskDto
            {
                Id = task.Id,
                FileObjectId = task.FileObjectId,
                OrderId = order.Id,
                OrderNo = order.OrderNo,
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
            items.Add(dto);
        }

        return new PagedResultDto<WorkshopTelemetryTaskDto>(result.TotalCount, items);
    }

    public virtual async Task<WorkshopOrderTelemetryGraphDto> GetTelemetryGraphAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);

        // 只取处理成功的任务，失败/待处理的任务没有波形数据。
        var taskResult = await _taskRepository.GetPagedListAsync(
            order.Id,
            null,
            TelemetryTaskStatusSuccess,
            null,
            null,
            0,
            int.MaxValue);

        var taskIds = taskResult.Items.Select(x => x.Id).ToList();
        var telemetries = await _telemetryRepository.GetListByTaskIdsAsync(taskIds);

        // 联合主键是 (DeviceId, Timestamp, ChannelType)：
        // 一张图谱 = 一个 (DeviceId, Timestamp) 分组，组内每个 ChannelType 一条曲线。
        var graphs = telemetries
            .GroupBy(x => new { x.DeviceId, x.Timestamp })
            .OrderBy(x => x.Key.Timestamp)
            .Select(group => new WorkshopTelemetryGraphGroupDto
            {
                Timestamp = group.Key.Timestamp,
                Channels = group
                    .OrderBy(x => x.ChannelType)
                    .Select(x => new WorkshopTelemetryGraphChannelDto
                    {
                        ChannelType = x.ChannelType,
                        SampleCount = x.Value?.Length ?? 0,
                        Values = x.Value
                    })
                    .ToList()
            })
            .ToList();

        var first = telemetries.FirstOrDefault();

        return new WorkshopOrderTelemetryGraphDto
        {
            OrderId = order.Id,
            OrderNo = order.OrderNo,
            DeviceCode = device?.Code,
            DeviceName = device?.Name,
            TestedDeviceCode = first?.TestedDeviceCode,
            TestedDeviceName = first?.TestedDeviceName,
            Graphs = graphs
        };
    }

    private static WorkshopOrderDto ToDto(WorkshopOrder order, WorkshopDevice device)
    {
        return new WorkshopOrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            DeviceId = order.DeviceId,
            DeviceCode = device?.Code,
            DeviceName = device?.Name,
            Status = order.Status,
            TestStartedAt = order.TestStartedAt,
            TestEndedAt = order.TestEndedAt,
            CreationTime = order.CreationTime,
            LastModificationTime = order.LastModificationTime
        };
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
    }
}
