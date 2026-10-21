namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试报告获取记录仓储。
/// </summary>
public interface IWorkshopOrderReportFetchRecordRepository :
    IRepository<WorkshopOrderReportFetchRecord, Guid>
{
    Task<List<WorkshopOrderReportFetchRecord>> GetListByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<int> GetAttemptCountAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveRecordAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<List<WorkshopOrderReportFetchRecord>> ClaimPendingRecordsAsync(
        int take,
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<List<WorkshopOrderReportFetchRecord>> GetStaleProcessingRecordsAsync(
        DateTime staleBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default);
}
