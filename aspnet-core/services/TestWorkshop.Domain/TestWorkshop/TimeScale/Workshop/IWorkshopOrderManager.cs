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

    Task<WorkshopOrder> ResolveTelemetryOrderAsync(
        Guid deviceId,
        DateTime dataStartTime,
        DateTime dataEndTime);
}
