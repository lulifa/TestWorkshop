namespace TestWorkshop.TimeScale;

/// <summary>
/// 遥测任务仓储接口
/// </summary>
public interface IWorkshopTelemetryTaskRepository : IRepository<WorkshopTelemetryTask, long>
{
    /// <summary>
    /// 原子获取待处理任务并标记为 Processing
    /// </summary>
    Task<List<WorkshopTelemetryTask>> ClaimPendingTasksAsync(int take = 5);

    /// <summary>
    /// 根据文件名搜索（JOIN FileObject）
    /// </summary>
    Task<List<WorkshopTelemetryTask>> SearchByFileNameAsync(string fileName);

    /// <summary>
    /// 分页查询任务列表（JOIN FileObject）
    /// </summary>
    Task<PagedResultDto<WorkshopTelemetryTask>> GetPagedListAsync(string fileName = null, int? status = null, DateTime? startTime = null, DateTime? endTime = null, int skipCount = 0, int maxResultCount = 10);

    /// <summary>
    /// 获取任务关联的 FileObject
    /// </summary>
    Task<FileObject> GetFileObjectAsync(Guid fileObjectId);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    Task<(int TotalFiles, long TotalSize, int PendingCount, int ProcessingCount, int SuccessCount, int FailedCount, long TotalRecords)> GetStatisticsDataAsync();

    /// <summary>
    /// 获取已过期且可清理的任务（返回 Task + FileObject）
    /// </summary>
    Task<List<(WorkshopTelemetryTask Task, FileObject FileObject)>> GetExpiredCompletedTasksAsync(int take = 100);

    /// <summary>
    /// 批量更新
    /// </summary>
    Task UpdateManyAsync(List<WorkshopTelemetryTask> tasks);

    /// <summary>
    /// 物理删除任务（真删除）
    /// </summary>
    Task DeleteAsync(WorkshopTelemetryTask task);

}
