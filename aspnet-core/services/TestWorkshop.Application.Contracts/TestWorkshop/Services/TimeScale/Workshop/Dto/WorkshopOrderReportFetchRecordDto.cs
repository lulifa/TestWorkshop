namespace TestWorkshop;

/// <summary>
/// 测试报告获取请求记录。
/// </summary>
public class WorkshopOrderReportFetchRecordDto : EntityDto<Guid>
{
    public Guid OrderId { get; set; }

    public WorkshopTestReportFetchTriggerType TriggerType { get; set; }

    public Guid? RequestedByUserId { get; set; }

    public WorkshopTestReportFetchRecordStatus Status { get; set; }

    public int AttemptNo { get; set; }

    public DateTime ScheduledAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? HttpStatusCode { get; set; }

    public string ExternalReportId { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorMessage { get; set; }

    public int FileCount { get; set; }

    public bool IsRetryable { get; set; }

    public DateTime CreationTime { get; set; }
}
