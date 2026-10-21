namespace TestWorkshop.TimeScale;

/// <summary>
/// 一次外部测试报告获取请求的执行记录。
/// </summary>
public class WorkshopOrderReportFetchRecord : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid OrderId { get; private set; }

    public WorkshopTestReportFetchTriggerType TriggerType { get; private set; }

    public Guid? RequestedByUserId { get; private set; }

    public WorkshopTestReportFetchRecordStatus Status { get; private set; }

    public int AttemptNo { get; private set; }

    public DateTime ScheduledAt { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public int? HttpStatusCode { get; private set; }

    public string ExternalReportId { get; private set; }

    public string ErrorCode { get; private set; }

    public string ErrorMessage { get; private set; }

    public int FileCount { get; private set; }

    public bool IsRetryable { get; private set; } = true;

    public Guid? TenantId { get; protected set; }

    protected WorkshopOrderReportFetchRecord()
    {
    }

    public WorkshopOrderReportFetchRecord(
        Guid id,
        Guid orderId,
        WorkshopTestReportFetchTriggerType triggerType,
        Guid? requestedByUserId,
        int attemptNo,
        DateTime scheduledAt,
        Guid? tenantId = null)
        : base(id)
    {
        if (orderId == Guid.Empty)
        {
            throw new UserFriendlyException("订单ID不能为空");
        }

        OrderId = orderId;
        TriggerType = triggerType;
        RequestedByUserId = requestedByUserId;
        AttemptNo = Math.Max(attemptNo, 1);
        ScheduledAt = NormalizeTime(scheduledAt);
        Status = WorkshopTestReportFetchRecordStatus.Pending;
        TenantId = tenantId;
    }

    public void MarkProcessing(DateTime startedAt)
    {
        if (Status != WorkshopTestReportFetchRecordStatus.Pending)
        {
            throw new UserFriendlyException("只有排队中的获取记录才能开始处理");
        }

        StartedAt = NormalizeTime(startedAt);
        Status = WorkshopTestReportFetchRecordStatus.Processing;
    }

    public void MarkNoReport(DateTime completedAt, string externalReportId = null)
    {
        EnsureProcessing();
        CompletedAt = NormalizeTime(completedAt);
        ExternalReportId = externalReportId?.Trim();
        Status = WorkshopTestReportFetchRecordStatus.NoReport;
    }

    public void MarkSucceeded(
        DateTime completedAt,
        int fileCount,
        string externalReportId = null)
    {
        EnsureProcessing();
        if (fileCount <= 0)
        {
            throw new UserFriendlyException("报告文件数量必须大于 0");
        }

        CompletedAt = NormalizeTime(completedAt);
        FileCount = fileCount;
        IsRetryable = false;
        ExternalReportId = externalReportId?.Trim();
        ErrorCode = null;
        ErrorMessage = null;
        HttpStatusCode = null;
        Status = WorkshopTestReportFetchRecordStatus.Succeeded;
    }

    public void MarkFailed(
        DateTime completedAt,
        string errorCode,
        string errorMessage,
        int? httpStatusCode = null,
        bool isRetryable = false)
    {
        EnsureProcessing();
        CompletedAt = NormalizeTime(completedAt);
        ErrorCode = Truncate(errorCode, 64);
        ErrorMessage = Truncate(errorMessage, 2000);
        HttpStatusCode = httpStatusCode;
        IsRetryable = isRetryable;
        Status = WorkshopTestReportFetchRecordStatus.Failed;
    }

    private void EnsureProcessing()
    {
        if (Status != WorkshopTestReportFetchRecordStatus.Processing)
        {
            throw new UserFriendlyException("获取记录当前不在处理中");
        }
    }

    private static DateTime NormalizeTime(DateTime value)
    {
        if (value == default)
        {
            throw new UserFriendlyException("时间不能为空");
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.IsNullOrWhiteSpace())
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
