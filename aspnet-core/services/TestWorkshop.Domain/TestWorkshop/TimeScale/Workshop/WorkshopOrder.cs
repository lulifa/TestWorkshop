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
        Guid? tenantId = null)
        : base(id)
    {
        SetOrderNo(orderNo);
        SetDevice(deviceId);
        Status = WorkshopOrderStatus.Draft;
        TenantId = tenantId;
    }

    /// <summary>
    /// 修改未开始订单的业务信息。
    /// </summary>
    public void Update(string orderNo, Guid deviceId)
    {
        EnsureDraft();
        SetOrderNo(orderNo);
        SetDevice(deviceId);
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

        Status = WorkshopOrderStatus.Completed;
        TestEndedAt = endedAt;
    }

    /// <summary>
    /// 取消订单并释放设备。
    /// </summary>
    public void Cancel(DateTime cancelledAt)
    {
        if (Status == WorkshopOrderStatus.Completed)
        {
            throw new UserFriendlyException("已完成的订单不能取消");
        }

        if (Status == WorkshopOrderStatus.Cancelled)
        {
            return;
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

    private void EnsureDraft()
    {
        if (Status != WorkshopOrderStatus.Draft)
        {
            throw new UserFriendlyException("只有未开始的订单才能修改");
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
}
