namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间测试订单。
/// 一个订单关联一台设备，同一设备同时只能有一个测试中订单。
/// </summary>
public class WorkshopOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNo { get; private set; }

    /// <summary>
    /// 关联设备
    /// </summary>
    public Guid DeviceId { get; private set; }

    /// <summary>
    /// 测试状态
    /// </summary>
    public WorkshopOrderStatus Status { get; private set; }

    /// <summary>
    /// 实际测试开始时间，使用本地时间保存
    /// </summary>
    public DateTime? TestStartedAt { get; private set; }

    /// <summary>
    /// 实际测试结束时间，使用本地时间保存
    /// </summary>
    public DateTime? TestEndedAt { get; private set; }

    /// <summary>
    /// 单船项目主管（拥有“单船项目主管”角色的用户）
    /// </summary>
    public Guid ProjectSupervisorUserId { get; private set; }

    /// <summary>
    /// 要求车间完成测试时间
    /// </summary>
    public DateTime RequiredCompletionTime { get; private set; }

    /// <summary>
    /// 外部测试报告获取状态
    /// </summary>
    public WorkshopTestReportStatus TestReportStatus { get; private set; }

    /// <summary>
    /// 外部测试报告文件
    /// </summary>
    public Guid? TestReportFileId { get; private set; }

    /// <summary>
    /// 外部测试报告文件名
    /// </summary>
    public string TestReportFileName { get; private set; }

    /// <summary>
    /// 测试报告获取完成时间
    /// </summary>
    public DateTime? TestReportFetchedAt { get; private set; }

    /// <summary>
    /// 测试报告获取失败原因
    /// </summary>
    public string TestReportError { get; private set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public Guid? TenantId { get; protected set; }

    protected WorkshopOrder()
    {
    }

    public WorkshopOrder(
        Guid id,
        string orderNo,
        Guid deviceId,
        Guid projectSupervisorUserId,
        DateTime requiredCompletionTime,
        Guid? tenantId = null)
        : base(id)
    {
        SetOrderNo(orderNo);
        SetDevice(deviceId);
        SetProjectSupervisor(projectSupervisorUserId);
        SetRequiredCompletionTime(requiredCompletionTime);
        Status = WorkshopOrderStatus.Draft;
        TestReportStatus = WorkshopTestReportStatus.NotReady;
        TenantId = tenantId;
    }

    /// <summary>
    /// 修改未开始订单的业务信息。
    /// </summary>
    public void Update(
        string orderNo,
        Guid deviceId,
        Guid projectSupervisorUserId,
        DateTime requiredCompletionTime)
    {
        EnsureDraft();
        SetOrderNo(orderNo);
        SetDevice(deviceId);
        SetProjectSupervisor(projectSupervisorUserId);
        SetRequiredCompletionTime(requiredCompletionTime);
    }

    /// <summary>
    /// 开始测试并占用设备。
    /// </summary>
    public void StartTest(DateTime startedAt)
    {
        EnsureDraft();
        if (startedAt == default)
        {
            throw new UserFriendlyException("测试开始时间不能为空");
        }

        Status = WorkshopOrderStatus.Testing;
        TestStartedAt = DateTime.SpecifyKind(startedAt, DateTimeKind.Unspecified);
        TestEndedAt = null;
    }

    /// <summary>
    /// 结束测试并释放设备。
    /// </summary>
    public void EndTest(DateTime endedAt)
    {
        if (Status != WorkshopOrderStatus.Testing)
        {
            throw new UserFriendlyException("只有测试中的订单才能结束测试");
        }

        if (endedAt == default)
        {
            throw new UserFriendlyException("测试结束时间不能为空");
        }

        endedAt = DateTime.SpecifyKind(endedAt, DateTimeKind.Unspecified);
        if (TestStartedAt.HasValue && endedAt < TestStartedAt.Value)
        {
            throw new UserFriendlyException("测试结束时间不能早于测试开始时间");
        }

        Status = WorkshopOrderStatus.PendingReview;
        TestEndedAt = endedAt;
        TestReportStatus = WorkshopTestReportStatus.NotReady;
        TestReportFileId = null;
        TestReportFileName = null;
        TestReportFetchedAt = null;
        TestReportError = null;
    }

    /// <summary>
    /// 取消订单并释放设备。
    /// </summary>
    public void Cancel(DateTime cancelledAt)
    {
        if (Status == WorkshopOrderStatus.Cancelled)
        {
            return;
        }

        if (Status != WorkshopOrderStatus.Draft
            && Status != WorkshopOrderStatus.Testing)
        {
            throw new UserFriendlyException("只有未开始或测试中的订单才能取消");
        }

        if (Status == WorkshopOrderStatus.Testing && TestStartedAt.HasValue)
        {
            cancelledAt = DateTime.SpecifyKind(cancelledAt, DateTimeKind.Unspecified);
            if (cancelledAt < TestStartedAt.Value)
            {
                throw new UserFriendlyException("取消时间不能早于测试开始时间");
            }

            TestEndedAt = cancelledAt;
        }

        Status = WorkshopOrderStatus.Cancelled;
    }

    /// <summary>
    /// 标记测试报告正在获取。
    /// </summary>
    public void MarkTestReportFetching()
    {
        EnsureReportCanBeUpdated();
        TestReportStatus = WorkshopTestReportStatus.Fetching;
        TestReportError = null;
    }

    /// <summary>
    /// 保存外部接口获取到的测试报告。
    /// </summary>
    public void SetTestReport(Guid fileId, string fileName, DateTime fetchedAt)
    {
        if (fileId == Guid.Empty)
        {
            throw new UserFriendlyException("测试报告文件不能为空");
        }

        Check.NotNullOrWhiteSpace(fileName, nameof(fileName));
        if (fetchedAt == default)
        {
            throw new UserFriendlyException("测试报告获取时间不能为空");
        }

        EnsureReportCanBeUpdated();
        TestReportFileId = fileId;
        TestReportFileName = fileName.Trim();
        TestReportFetchedAt = DateTime.SpecifyKind(fetchedAt, DateTimeKind.Unspecified);
        TestReportStatus = WorkshopTestReportStatus.Ready;
        TestReportError = null;
    }

    /// <summary>
    /// 标记测试报告获取失败。
    /// </summary>
    public void MarkTestReportFetchFailed(string error)
    {
        EnsureReportCanBeUpdated();
        TestReportStatus = WorkshopTestReportStatus.Failed;
        TestReportError = error?.Trim();
    }

    /// <summary>
    /// 提交报告审核。审核不通过后仍可再次审核，审核通过后锁定。
    /// </summary>
    public void Audit(WorkshopOrderAuditResult result)
    {
        if (Status != WorkshopOrderStatus.PendingReview
            && Status != WorkshopOrderStatus.ReviewRejected)
        {
            throw new UserFriendlyException("当前订单状态不允许审核");
        }

        if (TestReportStatus != WorkshopTestReportStatus.Ready
            || !TestReportFileId.HasValue)
        {
            throw new UserFriendlyException("测试报告尚未获取，不能审核");
        }

        Status = result == WorkshopOrderAuditResult.Approved
            ? WorkshopOrderStatus.ReviewApproved
            : WorkshopOrderStatus.ReviewRejected;
    }

    private void EnsureDraft()
    {
        if (Status != WorkshopOrderStatus.Draft)
        {
            throw new UserFriendlyException("只有未开始的订单才能修改");
        }
    }

    private void EnsureReportCanBeUpdated()
    {
        if (Status != WorkshopOrderStatus.PendingReview
            && Status != WorkshopOrderStatus.ReviewRejected)
        {
            throw new UserFriendlyException("当前订单状态不允许更新测试报告");
        }
    }

    private void SetOrderNo(string orderNo)
    {
        Check.NotNullOrWhiteSpace(orderNo, nameof(orderNo));
        OrderNo = orderNo.Trim();
    }

    private void SetDevice(Guid deviceId)
    {
        if (deviceId == Guid.Empty)
        {
            throw new UserFriendlyException("关联设备不能为空");
        }

        DeviceId = deviceId;
    }

    private void SetProjectSupervisor(Guid projectSupervisorUserId)
    {
        if (projectSupervisorUserId == Guid.Empty)
        {
            throw new UserFriendlyException("单船项目主管不能为空");
        }

        ProjectSupervisorUserId = projectSupervisorUserId;
    }

    private void SetRequiredCompletionTime(DateTime requiredCompletionTime)
    {
        if (requiredCompletionTime == default)
        {
            throw new UserFriendlyException("要求车间完成测试时间不能为空");
        }

        RequiredCompletionTime = DateTime.SpecifyKind(requiredCompletionTime, DateTimeKind.Unspecified);
    }
}
