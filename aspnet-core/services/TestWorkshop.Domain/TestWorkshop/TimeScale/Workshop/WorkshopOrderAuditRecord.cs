namespace TestWorkshop.TimeScale;

/// <summary>
/// 订单测试报告审核记录。记录只追加，不覆盖历史审核结果。
/// </summary>
public class WorkshopOrderAuditRecord : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid OrderId { get; private set; }

    public WorkshopOrderAuditResult Result { get; private set; }

    public string MaintenanceSuggestion { get; private set; }

    public Guid AuditorUserId { get; private set; }

    public string AuditorName { get; private set; }

    public DateTime AuditedAt { get; private set; }

    /// <summary>
    /// 审核时对应的报告文件，避免后续报告更新导致记录无法追溯。
    /// </summary>
    public Guid? ReportFileId { get; private set; }

    public Guid? TenantId { get; protected set; }

    protected WorkshopOrderAuditRecord()
    {
    }

    public WorkshopOrderAuditRecord(
        Guid id,
        Guid orderId,
        WorkshopOrderAuditResult result,
        string maintenanceSuggestion,
        Guid auditorUserId,
        string auditorName,
        DateTime auditedAt,
        Guid? reportFileId,
        Guid? tenantId = null)
        : base(id)
    {
        if (orderId == Guid.Empty)
        {
            throw new UserFriendlyException("订单不能为空");
        }

        if (auditorUserId == Guid.Empty)
        {
            throw new UserFriendlyException("审核人不能为空");
        }

        Check.NotNullOrWhiteSpace(auditorName, nameof(auditorName));
        if (auditedAt == default)
        {
            throw new UserFriendlyException("审核时间不能为空");
        }

        OrderId = orderId;
        Result = result;
        MaintenanceSuggestion = NormalizeText(maintenanceSuggestion);
        AuditorUserId = auditorUserId;
        AuditorName = auditorName.Trim();
        AuditedAt = DateTime.SpecifyKind(auditedAt, DateTimeKind.Unspecified);
        ReportFileId = reportFileId;
        TenantId = tenantId;
    }

    private static string NormalizeText(string value)
    {
        return value.IsNullOrWhiteSpace() ? null : value.Trim();
    }
}
