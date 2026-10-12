namespace TestWorkshop.TimeScale;

/// <summary>
/// 订单相关消息通知
/// </summary>
public interface IWorkshopOrderNotificationSender
{
    /// <summary>
    /// 订单创建后通知“设备所属车间”的车间主管与大区负责人
    /// </summary>
    Task NotifyOrderCreatedAsync(WorkshopOrder order);
}
