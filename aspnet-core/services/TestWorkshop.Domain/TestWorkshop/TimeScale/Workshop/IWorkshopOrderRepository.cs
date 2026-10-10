namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间测试订单仓储
/// </summary>
public interface IWorkshopOrderRepository : IRepository<WorkshopOrder, Guid>
{
    Task<WorkshopOrder> FindByOrderNoAsync(string orderNo);

    Task<WorkshopOrder> FindActiveByDeviceAsync(Guid deviceId);

    Task AcquireDeviceLockAsync(Guid deviceId);

    Task<List<WorkshopOrder>> GetTelemetryCandidatesAsync(
        Guid deviceId,
        DateTime earliestEndTime,
        DateTime latestStartTime);

    Task<PagedResultDto<WorkshopOrder>> GetPagedListAsync(
        string orderNo = null,
        Guid? deviceId = null,
        WorkshopOrderStatus? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int skipCount = 0,
        int maxResultCount = 10);
}
