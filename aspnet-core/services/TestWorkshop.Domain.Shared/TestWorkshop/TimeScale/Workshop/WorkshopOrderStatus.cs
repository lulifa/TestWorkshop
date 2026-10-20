namespace TestWorkshop.TimeScale;

/// <summary>
/// 订单测试状态
/// </summary>
public enum WorkshopOrderStatus
{
    /// <summary>
    /// 已录入，未开始
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 测试中，占用设备
    /// </summary>
    Testing = 1,

    /// <summary>
    /// 测试完成，等待报告审核
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// 已取消，释放设备
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// 审核通过，可下载最终报告
    /// </summary>
    ReviewApproved = 4,

    /// <summary>
    /// 审核不通过，可重新审核
    /// </summary>
    ReviewRejected = 5
}
