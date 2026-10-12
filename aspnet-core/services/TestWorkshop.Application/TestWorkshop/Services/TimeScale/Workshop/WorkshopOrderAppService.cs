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
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IWorkshopOrderNotificationSender _orderNotificationSender;

    public WorkshopOrderAppService(
        IWorkshopOrderManager orderManager,
        IWorkshopOrderRepository orderRepository,
        IWorkshopDeviceRepository deviceRepository,
        IWorkshopTelemetryTaskRepository taskRepository,
        IWorkshopDeviceTelemetryRepository telemetryRepository,
        IFileObjectRepository fileObjectRepository,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IWorkshopOrderNotificationSender orderNotificationSender)
    {
        _orderManager = orderManager;
        _orderRepository = orderRepository;
        _deviceRepository = deviceRepository;
        _taskRepository = taskRepository;
        _telemetryRepository = telemetryRepository;
        _fileObjectRepository = fileObjectRepository;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _orderNotificationSender = orderNotificationSender;
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

        var supervisorIds = result.Items
            .Select(x => x.ProjectSupervisorUserId)
            .Distinct()
            .ToList();
        var supervisors = supervisorIds.Count == 0
            ? new List<IdentityUser>()
            : await _identityUserRepository.GetListByIdListAsync(supervisorIds);
        var supervisorMap = supervisors.ToDictionary(x => x.Id);

        var items = result.Items
            .Select(order => ToDto(
                order,
                deviceMap.GetValueOrDefault(order.DeviceId),
                GetUserName(
                    supervisorMap.GetValueOrDefault(order.ProjectSupervisorUserId))))
            .ToList();

        return new PagedResultDto<WorkshopOrderDto>(result.TotalCount, items);
    }

    public virtual async Task<WorkshopOrderDto> GetAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.dispatcher)]
    public virtual async Task<WorkshopOrderDto> CreateAsync(WorkshopOrderCreateDto input)
    {
        var order = await _orderManager.CreateAsync(
            input.OrderNo,
            input.DeviceId,
            input.ProjectSupervisorUserId,
            input.RequiredCompletionTime);
        var device = await _deviceRepository.FindAsync(order.DeviceId);

        await _orderNotificationSender.NotifyOrderCreatedAsync(order);

        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.dispatcher)]
    public virtual async Task<WorkshopOrderDto> UpdateAsync(Guid id, WorkshopOrderUpdateDto input)
    {
        var order = await _orderManager.UpdateAsync(
            id,
            input.OrderNo,
            input.DeviceId,
            input.ProjectSupervisorUserId,
            input.RequiredCompletionTime);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.dispatcher)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _orderManager.DeleteAsync(id);
    }

    /// <summary>
    /// 可选的单船项目主管（拥有“单船项目主管”角色的用户）
    /// </summary>
    public virtual async Task<List<WorkshopOrderProjectSupervisorDto>> GetProjectSupervisorOptionsAsync()
    {
        var users = await _identityUserManager.GetUsersInRoleAsync(RoleConstants.projectSupervisor);

        return users
            .OrderBy(x => GetUserName(x))
            .Select(x => new WorkshopOrderProjectSupervisorDto
            {
                UserId = x.Id,
                UserName = x.UserName,
                Name = GetUserName(x)
            })
            .ToList();
    }

    // 开始/结束测试：测试工程师、车间主管、管理员
    // （车间主管是“要求完成测试时间”的责任人，暂定可操作，后续开会再确认）
    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> StartTestAsync(Guid id)
    {
        var order = await _orderManager.StartTestAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> EndTestAsync(Guid id)
    {
        var order = await _orderManager.EndTestAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    // 取消订单同样只有测试工程师和管理员可以操作
    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> CancelAsync(Guid id)
    {
        var order = await _orderManager.CancelAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
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
            input.OrderNo,
            input.FileName,
            input.Status,
            input.StartTime,
            input.EndTime,
            input.TestedDeviceCode,
            input.HasOrder,
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
            null,
            TelemetryTaskStatusSuccess,
            null,
            null,
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

    private static WorkshopOrderDto ToDto(
        WorkshopOrder order,
        WorkshopDevice device,
        string projectSupervisorName = null)
    {
        return new WorkshopOrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            DeviceId = order.DeviceId,
            DeviceCode = device?.Code,
            DeviceName = device?.Name,
            ProjectSupervisorUserId = order.ProjectSupervisorUserId,
            ProjectSupervisorName = projectSupervisorName,
            RequiredCompletionTime = order.RequiredCompletionTime,
            Status = order.Status,
            TestStartedAt = order.TestStartedAt,
            TestEndedAt = order.TestEndedAt,
            CreationTime = order.CreationTime,
            LastModificationTime = order.LastModificationTime
        };
    }

    private static string GetUserName(IdentityUser user)
    {
        if (user == null)
        {
            return null;
        }

        return user.Name.IsNullOrWhiteSpace() ? user.UserName : user.Name;
    }

    private async Task<string> GetProjectSupervisorNameAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await _identityUserRepository.FindAsync(userId);
        return GetUserName(user);
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
}
