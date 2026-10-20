namespace TestWorkshop;

/// <summary>
/// 订单测试报告审核记录
/// </summary>
public class WorkshopOrderAuditRecordDto : EntityDto<Guid>
{
    public Guid OrderId { get; set; }

    public WorkshopOrderAuditResult Result { get; set; }

    public string ResultName => Result switch
    {
        WorkshopOrderAuditResult.Approved => "通过",
        WorkshopOrderAuditResult.Rejected => "不通过",
        _ => "未知"
    };

    public string MaintenanceSuggestion { get; set; }

    public Guid AuditorUserId { get; set; }

    public string AuditorName { get; set; }

    public DateTime AuditedAt { get; set; }

    public int ReportFileCount { get; set; }
}
