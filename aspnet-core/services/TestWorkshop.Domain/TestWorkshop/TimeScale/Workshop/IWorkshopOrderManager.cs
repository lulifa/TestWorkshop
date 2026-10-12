namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间测试订单领域服务
/// </summary>
public interface IWorkshopOrderManager
{
    Task<WorkshopOrder> CreateAsync(string orderNo, Guid deviceId);

    Task<WorkshopOrder> UpdateAsync(Guid id, string orderNo, Guid deviceId);

    Task<WorkshopOrder> StartTestAsync(Guid id);

    Task<WorkshopOrder> EndTestAsync(Guid id);

    Task<WorkshopOrder> CancelAsync(Guid id);

    Task DeleteAsync(Guid id);

    /// <summary>
    /// 按设备与数据时间区间匹配订单。
    /// 匹配不到订单、或匹配到多个订单时返回 null，
    /// 由调用方按“未匹配订单”接收数据，不再拒绝上传。
    /// </summary>
    Task<WorkshopOrder> ResolveTelemetryOrderAsync(
        Guid deviceId,
        DateTime dataStartTime,
        DateTime dataEndTime);
}
