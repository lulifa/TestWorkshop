using Volo.Abp.Domain.Repositories;

namespace TestWorkshop;

/// <summary>
/// 车间测试订单应用服务
/// </summary>
[Authorize]
public class WorkshopOrderAppService : TestWorkshopAppService, IWorkshopOrderAppService
{
    // 遥测任务状态：0=待处理 1=处理中 2=成功 3=失败
    private const int TelemetryTaskStatusSuccess = 2;

    /// <summary>
    /// 不按车间限制、可查看全部订单的岗位：管理员 + 调度 + 质量审核员。
    /// 其他角色（车间主管 / 测试工程师 / 大区负责人等）只看自己所属车间的订单。
    /// </summary>
    private static readonly string[] OrganizationUnitUnrestrictedRoles =
    {
        RoleConstants.admin,
        RoleConstants.dispatcher,
        RoleConstants.auditor
    };

    private readonly IWorkshopOrderManager _orderManager;
    private readonly IWorkshopOrderRepository _orderRepository;
    private readonly IRepository<WorkshopOrderAuditRecord, Guid> _auditRecordRepository;
    private readonly IWorkshopDeviceRepository _deviceRepository;
    private readonly IWorkshopTelemetryTaskRepository _taskRepository;
    private readonly IWorkshopDeviceTelemetryRepository _telemetryRepository;
    private readonly IFileObjectRepository _fileObjectRepository;
    private readonly IFileObjectManager _fileObjectManager;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IWorkshopOrderNotificationSender _orderNotificationSender;
    private readonly IWorkshopOrganizationUnitAccessChecker _organizationUnitAccessChecker;

    public WorkshopOrderAppService(
        IWorkshopOrderManager orderManager,
        IWorkshopOrderRepository orderRepository,
        IRepository<WorkshopOrderAuditRecord, Guid> auditRecordRepository,
        IWorkshopDeviceRepository deviceRepository,
        IWorkshopTelemetryTaskRepository taskRepository,
        IWorkshopDeviceTelemetryRepository telemetryRepository,
        IFileObjectRepository fileObjectRepository,
        IFileObjectManager fileObjectManager,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IWorkshopOrderNotificationSender orderNotificationSender,
        IWorkshopOrganizationUnitAccessChecker organizationUnitAccessChecker)
    {
        _orderManager = orderManager;
        _orderRepository = orderRepository;
        _auditRecordRepository = auditRecordRepository;
        _deviceRepository = deviceRepository;
        _taskRepository = taskRepository;
        _telemetryRepository = telemetryRepository;
        _fileObjectRepository = fileObjectRepository;
        _fileObjectManager = fileObjectManager;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _orderNotificationSender = orderNotificationSender;
        _organizationUnitAccessChecker = organizationUnitAccessChecker;
    }

    public virtual async Task<PagedResultDto<WorkshopOrderDto>> GetListAsync(WorkshopOrderListInput input)
    {
        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        // 可见性：管理员/调度/质量审核员不限；其他角色看"自己所属车间的订单" + "自己作为单船项目主管的订单"
        var (organizationUnitIds, projectSupervisorUserId) = await GetOrderVisibilityAsync();

        var result = await _orderRepository.GetPagedListAsync(
            input.OrderNo,
            input.DeviceId,
            input.Status,
            input.StartTime,
            input.EndTime,
            organizationUnitIds,
            projectSupervisorUserId,
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
        await EnsureOrderAccessibleAsync(order);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    /// <summary>
    /// 当前用户的订单可见范围：
    /// 管理员 / 调度 / 质量审核员 → 不限（都返回 null）；
    /// 其他角色 → 自己所属车间的订单，外加"自己作为单船项目主管"的订单。
    /// </summary>
    private async Task<(ICollection<Guid> OrganizationUnitIds, Guid? ProjectSupervisorUserId)> GetOrderVisibilityAsync()
    {
        if (OrganizationUnitUnrestrictedRoles.Any(CurrentUser.IsInRole))
        {
            return (null, null);
        }

        var organizationUnitIds = await _organizationUnitAccessChecker.GetAccessibleOrganizationUnitIdsAsync();
        var projectSupervisorUserId = CurrentUser.IsInRole(RoleConstants.projectSupervisor)
            ? CurrentUser.GetId()
            : (Guid?)null;

        return (organizationUnitIds, projectSupervisorUserId);
    }

    /// <summary>
    /// 校验订单对当前用户可见
    /// </summary>
    private async Task EnsureOrderAccessibleAsync(WorkshopOrder order)
    {
        if (OrganizationUnitUnrestrictedRoles.Any(CurrentUser.IsInRole))
        {
            return;
        }

        // 自己是这单的单船项目主管
        if (CurrentUser.IsInRole(RoleConstants.projectSupervisor)
            && order.ProjectSupervisorUserId == CurrentUser.GetId())
        {
            return;
        }

        var device = await _deviceRepository.FindAsync(order.DeviceId);
        if (device == null)
        {
            return;
        }

        await _organizationUnitAccessChecker.EnsureAccessibleAsync(device.OrganizationUnitId);
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

    // 开始/结束测试：管理员、车间主管、测试工程师
    // （车间主管是“要求完成测试时间”的责任人，且权限覆盖测试工程师）
    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> StartTestAsync(Guid id)
    {
        await EnsureOrderAccessibleAsync(await _orderRepository.GetAsync(id));
        var order = await _orderManager.StartTestAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> EndTestAsync(Guid id)
    {
        await EnsureOrderAccessibleAsync(await _orderRepository.GetAsync(id));
        var order = await _orderManager.EndTestAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    // 取消订单与开始/结束测试同一套角色：管理员、车间主管、测试工程师
    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.supervisor + "," + RoleConstants.tester)]
    public virtual async Task<WorkshopOrderDto> CancelAsync(Guid id)
    {
        await EnsureOrderAccessibleAsync(await _orderRepository.GetAsync(id));
        var order = await _orderManager.CancelAsync(id);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.auditor)]
    public virtual async Task<WorkshopOrderDto> AuditAsync(Guid id, WorkshopOrderAuditInput input)
    {
        if (!input.Result.HasValue
            || !Enum.IsDefined(typeof(WorkshopOrderAuditResult), input.Result.Value))
        {
            throw new UserFriendlyException("审核结果无效");
        }

        await EnsureOrderAccessibleAsync(await _orderRepository.GetAsync(id));
        var auditor = await _identityUserRepository.FindAsync(CurrentUser.GetId());
        var auditorName = GetUserName(auditor) ?? CurrentUser.UserName;
        var order = await _orderManager.AuditAsync(
            id,
            input.Result.Value,
            input.MaintenanceSuggestion,
            CurrentUser.GetId(),
            auditorName);
        var device = await _deviceRepository.FindAsync(order.DeviceId);
        return ToDto(order, device, await GetProjectSupervisorNameAsync(order.ProjectSupervisorUserId));
    }

    public virtual async Task<List<WorkshopOrderAuditRecordDto>> GetAuditRecordsAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        await EnsureOrderAccessibleAsync(order);

        var auditQuery = await _auditRecordRepository.GetQueryableAsync();
        var records = await _asyncQueryableExecuter.ToListAsync(
            auditQuery
                .Where(x => x.OrderId == id)
                .OrderByDescending(x => x.AuditedAt));

        return records
            .Select(x => new WorkshopOrderAuditRecordDto
            {
                Id = x.Id,
                OrderId = x.OrderId,
                Result = x.Result,
                MaintenanceSuggestion = x.MaintenanceSuggestion,
                AuditorUserId = x.AuditorUserId,
                AuditorName = x.AuditorName,
                AuditedAt = x.AuditedAt
            })
            .ToList();
    }

    [Authorize(
        Roles = RoleConstants.admin
                + ","
                + RoleConstants.auditor
                + ","
                + RoleConstants.dispatcher
                + ","
                + RoleConstants.projectSupervisor)]
    public virtual async Task<IRemoteStreamContent> DownloadTestReportAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        EnsureTestReportDownloadable(order);

        return await ReadTestReportAsync(order);
    }

    [Authorize(Roles = RoleConstants.admin + "," + RoleConstants.auditor)]
    public virtual async Task<IRemoteStreamContent> PreviewTestReportAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        if (order.Status != WorkshopOrderStatus.PendingReview
            && order.Status != WorkshopOrderStatus.ReviewRejected)
        {
            throw new UserFriendlyException("当前订单状态不需要预览测试报告");
        }

        EnsureTestReportReady(order);
        return await ReadTestReportAsync(order);
    }

    public virtual async Task<PagedResultDto<WorkshopTelemetryTaskDto>> GetTelemetryTasksAsync(
        Guid id,
        WorkshopTelemetryTaskListInput input)
    {
        var order = await _orderRepository.GetAsync(id);
        await EnsureOrderAccessibleAsync(order);

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
        await EnsureOrderAccessibleAsync(order);
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
            TestReportStatus = order.TestReportStatus,
            TestReportFileName = order.TestReportFileName,
            TestReportFetchedAt = order.TestReportFetchedAt,
            CreationTime = order.CreationTime,
            LastModificationTime = order.LastModificationTime
        };
    }

    private void EnsureTestReportDownloadable(WorkshopOrder order)
    {
        if (order.Status != WorkshopOrderStatus.ReviewApproved)
        {
            throw new UserFriendlyException("只有审核通过的订单才能下载最终报告");
        }

        EnsureTestReportReady(order);

        if (CurrentUser.IsInRole(RoleConstants.admin)
            || CurrentUser.IsInRole(RoleConstants.dispatcher)
            || CurrentUser.IsInRole(RoleConstants.auditor))
        {
            return;
        }

        if (CurrentUser.IsInRole(RoleConstants.projectSupervisor)
            && order.ProjectSupervisorUserId == CurrentUser.GetId())
        {
            return;
        }

        throw new UserFriendlyException("无权下载该订单的测试报告");
    }

    private static void EnsureTestReportReady(WorkshopOrder order)
    {
        if (order.TestReportStatus != WorkshopTestReportStatus.Ready
            || !order.TestReportFileId.HasValue)
        {
            throw new UserFriendlyException("测试报告尚未获取");
        }
    }

    private async Task<IRemoteStreamContent> ReadTestReportAsync(WorkshopOrder order)
    {
        var (stream, contentType, fileName) =
            await _fileObjectManager.GetFileAsync(order.TestReportFileId.Value);
        return new RemoteStreamContent(stream, fileName, contentType);
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
