using Volo.Abp.Timing;

namespace TestWorkshop.TimeScale;

public class WorkshopOrderManager : DomainService, IWorkshopOrderManager
{
    private readonly IWorkshopOrderRepository _orderRepository;
    private readonly IRepository<WorkshopOrderAuditRecord, Guid> _auditRecordRepository;
    private readonly IWorkshopDeviceRepository _deviceRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly ILogger<WorkshopOrderManager> _logger;
    private readonly TimeSpan _telemetryTimeTolerance;

    public WorkshopOrderManager(
        IWorkshopOrderRepository orderRepository,
        IRepository<WorkshopOrderAuditRecord, Guid> auditRecordRepository,
        IWorkshopDeviceRepository deviceRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator,
        IClock clock,
        ILogger<WorkshopOrderManager> logger,
        IOptions<WorkshopTelemetryOptions> options)
    {
        _orderRepository = orderRepository;
        _auditRecordRepository = auditRecordRepository;
        _deviceRepository = deviceRepository;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _logger = logger;
        options.Value.Validate();
        _telemetryTimeTolerance = TimeSpan.FromSeconds(options.Value.OrderMatchToleranceSeconds);
    }

    public virtual async Task<WorkshopOrder> CreateAsync(
        string orderNo,
        Guid deviceId,
        Guid projectSupervisorUserId,
        DateTime requiredCompletionTime)
    {
        var normalizedOrderNo = NormalizeOrderNo(orderNo);
        await EnsureOrderNoAvailableAsync(normalizedOrderNo);
        await EnsureDeviceExistsAsync(deviceId);
        await EnsureProjectSupervisorAsync(projectSupervisorUserId);
        EnsureRequiredCompletionTime(requiredCompletionTime);

        var order = new WorkshopOrder(
            _guidGenerator.Create(),
            normalizedOrderNo,
            deviceId,
            projectSupervisorUserId,
            requiredCompletionTime,
            CurrentTenant.Id);

        await _orderRepository.InsertAsync(order, autoSave: true);
        _logger.LogInformation("创建测试订单成功: OrderNo={OrderNo}, DeviceId={DeviceId}", normalizedOrderNo, deviceId);
        return order;
    }

    public virtual async Task<WorkshopOrder> UpdateAsync(
        Guid id,
        string orderNo,
        Guid deviceId,
        Guid projectSupervisorUserId,
        DateTime requiredCompletionTime)
    {
        var order = await _orderRepository.GetAsync(id);
        var normalizedOrderNo = NormalizeOrderNo(orderNo);
        await EnsureOrderNoAvailableAsync(normalizedOrderNo, id);
        await EnsureDeviceExistsAsync(deviceId);
        await EnsureProjectSupervisorAsync(projectSupervisorUserId);
        EnsureRequiredCompletionTime(requiredCompletionTime);

        order.Update(normalizedOrderNo, deviceId, projectSupervisorUserId, requiredCompletionTime);
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    public virtual async Task<WorkshopOrder> StartTestAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);

        await _orderRepository.AcquireDeviceLockAsync(order.DeviceId);

        var activeOrder = await _orderRepository.FindActiveByDeviceAsync(order.DeviceId);
        if (activeOrder != null && activeOrder.Id != order.Id)
        {
            throw new UserFriendlyException($"设备已被订单 {activeOrder.OrderNo} 占用，不能重复开始测试");
        }

        order.StartTest(GetLocalNow());
        await _orderRepository.UpdateAsync(order, autoSave: true);

        _logger.LogInformation(
            "订单开始测试: OrderId={OrderId}, OrderNo={OrderNo}, DeviceId={DeviceId}",
            order.Id,
            order.OrderNo,
            order.DeviceId);
        return order;
    }

    public virtual async Task<WorkshopOrder> EndTestAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        order.EndTest(GetLocalNow());
        await _orderRepository.UpdateAsync(order, autoSave: true);

        _logger.LogInformation("订单结束测试: OrderId={OrderId}, OrderNo={OrderNo}", order.Id, order.OrderNo);
        return order;
    }

    public virtual async Task<WorkshopOrder> CancelAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        order.Cancel(GetLocalNow());
        await _orderRepository.UpdateAsync(order, autoSave: true);

        _logger.LogInformation("订单已取消: OrderId={OrderId}, OrderNo={OrderNo}", order.Id, order.OrderNo);
        return order;
    }

    public virtual async Task<WorkshopOrder> AuditAsync(
        Guid id,
        WorkshopOrderAuditResult result,
        string maintenanceSuggestion,
        Guid auditorUserId,
        string auditorName)
    {
        var order = await _orderRepository.GetAsync(id);
        order.Audit(result);

        var auditRecord = new WorkshopOrderAuditRecord(
            _guidGenerator.Create(),
            order.Id,
            result,
            maintenanceSuggestion,
            auditorUserId,
            auditorName,
            GetLocalNow(),
            order.TestReportFileId,
            CurrentTenant.Id);

        await _auditRecordRepository.InsertAsync(auditRecord, autoSave: true);
        await _orderRepository.UpdateAsync(order, autoSave: true);

        _logger.LogInformation(
            "订单报告审核完成: OrderId={OrderId}, OrderNo={OrderNo}, Result={Result}",
            order.Id,
            order.OrderNo,
            result);
        return order;
    }

    public virtual async Task<WorkshopOrder> MarkTestReportFetchingAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        order.MarkTestReportFetching();
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    public virtual async Task<WorkshopOrder> SetTestReportAsync(Guid id, Guid fileId, string fileName)
    {
        var order = await _orderRepository.GetAsync(id);
        order.SetTestReport(fileId, fileName, GetLocalNow());
        await _orderRepository.UpdateAsync(order, autoSave: true);

        _logger.LogInformation(
            "订单测试报告已就绪: OrderId={OrderId}, OrderNo={OrderNo}, FileId={FileId}",
            order.Id,
            order.OrderNo,
            fileId);
        return order;
    }

    public virtual async Task<WorkshopOrder> MarkTestReportFetchFailedAsync(Guid id, string error)
    {
        var order = await _orderRepository.GetAsync(id);
        order.MarkTestReportFetchFailed(error);
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        if (order.Status == WorkshopOrderStatus.Testing)
        {
            throw new UserFriendlyException("测试中的订单不能删除");
        }

        if (order.Status == WorkshopOrderStatus.PendingReview
            || order.Status == WorkshopOrderStatus.ReviewApproved
            || order.Status == WorkshopOrderStatus.ReviewRejected)
        {
            throw new UserFriendlyException("测试已完成或已进入审核流程的订单不能删除");
        }

        await _orderRepository.DeleteAsync(order, autoSave: true);
    }

    public virtual async Task<WorkshopOrder> ResolveTelemetryOrderAsync(
        Guid deviceId,
        DateTime dataStartTime,
        DateTime dataEndTime)
    {
        if (deviceId == Guid.Empty)
        {
            throw new UserFriendlyException("设备ID不能为空");
        }

        if (dataEndTime < dataStartTime)
        {
            throw new UserFriendlyException("数据结束时间不能早于开始时间");
        }

        var candidates = await _orderRepository.GetTelemetryCandidatesAsync(
            deviceId,
            dataStartTime - _telemetryTimeTolerance,
            dataEndTime + _telemetryTimeTolerance);

        var matchedOrders = candidates
            .Where(order => IsTelemetryInOrderWindow(order, dataStartTime, dataEndTime))
            .ToList();

        if (matchedOrders.Count == 1)
        {
            return matchedOrders[0];
        }

        // 匹配不到订单、或匹配到多个订单，都不再拒绝上传：
        // 任务照常接收，OrderId 留空，由前端在遥测任务列表里标记为未匹配。
        _logger.LogWarning(
            "遥测数据未唯一匹配订单，按未匹配接收。DeviceId={DeviceId}, Time={StartTime}~{EndTime}, MatchedCount={MatchedCount}, MatchedOrders={MatchedOrders}",
            deviceId,
            dataStartTime,
            dataEndTime,
            matchedOrders.Count,
            string.Join(", ", matchedOrders.Select(x => x.OrderNo)));
        return null;
    }

    private async Task EnsureOrderNoAvailableAsync(string orderNo, Guid? excludeId = null)
    {
        var existing = await _orderRepository.FindByOrderNoAsync(orderNo);
        if (existing != null && (!excludeId.HasValue || existing.Id != excludeId.Value))
        {
            throw new UserFriendlyException($"订单号已存在: {orderNo}");
        }
    }

    private async Task EnsureDeviceExistsAsync(Guid deviceId)
    {
        var device = await _deviceRepository.FindAsync(deviceId);
        if (device == null)
        {
            throw new UserFriendlyException($"设备不存在: {deviceId}");
        }
    }

    /// <summary>
    /// 单船项目主管必须是拥有“单船项目主管”角色的用户
    /// </summary>
    private async Task EnsureProjectSupervisorAsync(Guid projectSupervisorUserId)
    {
        if (projectSupervisorUserId == Guid.Empty)
        {
            throw new UserFriendlyException("单船项目主管不能为空");
        }

        var user = await _identityUserRepository.FindAsync(projectSupervisorUserId);
        if (user == null)
        {
            throw new UserFriendlyException("单船项目主管不存在");
        }

        if (!await _identityUserManager.IsInRoleAsync(user, RoleConstants.projectSupervisor))
        {
            throw new UserFriendlyException("单船项目主管必须是拥有“单船项目主管”角色的人员");
        }
    }

    private static void EnsureRequiredCompletionTime(DateTime requiredCompletionTime)
    {
        if (requiredCompletionTime == default)
        {
            throw new UserFriendlyException("要求车间完成测试时间不能为空");
        }
    }

    private bool IsTelemetryInOrderWindow(
        WorkshopOrder order,
        DateTime dataStartTime,
        DateTime dataEndTime)
    {
        if (!order.TestStartedAt.HasValue)
        {
            return false;
        }

        var orderStartTime = order.TestStartedAt.Value;
        var orderEndTime = order.TestEndedAt.HasValue
            ? order.TestEndedAt.Value
            : (DateTime?)null;

        if (dataStartTime < orderStartTime - _telemetryTimeTolerance)
        {
            return false;
        }

        return !orderEndTime.HasValue
               || dataEndTime <= orderEndTime.Value + _telemetryTimeTolerance;
    }

    private static string NormalizeOrderNo(string orderNo)
    {
        Check.NotNullOrWhiteSpace(orderNo, nameof(orderNo));
        return orderNo.Trim();
    }

    private DateTime GetLocalNow()
    {
        return DateTime.SpecifyKind(_clock.Now, DateTimeKind.Unspecified);
    }
}
