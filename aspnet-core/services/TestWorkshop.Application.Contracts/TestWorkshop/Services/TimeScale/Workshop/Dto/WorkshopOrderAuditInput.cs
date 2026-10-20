using System.ComponentModel.DataAnnotations;

namespace TestWorkshop;

/// <summary>
/// 订单测试报告审核输入
/// </summary>
public class WorkshopOrderAuditInput
{
    public WorkshopOrderAuditResult? Result { get; set; }

    [StringLength(TestWorkshopConsts.MaxLength2000)]
    public string MaintenanceSuggestion { get; set; }
}
