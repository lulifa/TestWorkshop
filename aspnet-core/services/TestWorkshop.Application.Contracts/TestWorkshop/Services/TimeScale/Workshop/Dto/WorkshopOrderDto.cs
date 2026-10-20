namespace TestWorkshop;

/// <summary>
/// 车间测试订单 DTO
/// </summary>
public class WorkshopOrderDto : EntityDto<Guid>
{
    public string OrderNo { get; set; }

    public Guid DeviceId { get; set; }

    public string DeviceCode { get; set; }

    public string DeviceName { get; set; }

    /// <summary>
    /// 单船项目主管用户ID
    /// </summary>
    public Guid ProjectSupervisorUserId { get; set; }

    /// <summary>
    /// 单船项目主管姓名
    /// </summary>
    public string ProjectSupervisorName { get; set; }

    /// <summary>
    /// 要求车间完成测试时间
    /// </summary>
    public DateTime RequiredCompletionTime { get; set; }

    public WorkshopOrderStatus Status { get; set; }

    public string StatusName => Status switch
    {
        WorkshopOrderStatus.Draft => "已录入未测试",
        WorkshopOrderStatus.Testing => "测试中",
        WorkshopOrderStatus.PendingReview => "测试完成未审核",
        WorkshopOrderStatus.ReviewApproved => "审核通过",
        WorkshopOrderStatus.ReviewRejected => "审核不通过",
        WorkshopOrderStatus.Cancelled => "已取消",
        _ => "未知"
    };

    public DateTime? TestStartedAt { get; set; }

    public DateTime? TestEndedAt { get; set; }

    /// <summary>
    /// 测试报告获取状态
    /// </summary>
    public WorkshopTestReportStatus TestReportStatus { get; set; }

    /// <summary>
    /// 测试报告获取完成时间
    /// </summary>
    public DateTime? TestReportFetchedAt { get; set; }

    /// <summary>
    /// 测试报告文件数量
    /// </summary>
    public int TestReportFileCount { get; set; }

    /// <summary>
    /// 是否已有可用的测试报告
    /// </summary>
    public bool HasTestReport =>
        TestReportStatus == WorkshopTestReportStatus.Ready
        && TestReportFileCount > 0;

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}
