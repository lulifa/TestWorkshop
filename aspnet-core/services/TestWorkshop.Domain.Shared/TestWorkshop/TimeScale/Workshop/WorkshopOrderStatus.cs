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
    /// 已完成，释放设备
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 已取消，释放设备
    /// </summary>
    Cancelled = 3
}
