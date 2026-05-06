namespace TestWorkshop.TimeScale;

/// <summary>
/// 遥测任务仓储接口
/// </summary>
public interface ITelemetryTaskRepository : IRepository<TelemetryTask, long>
{
    /// <summary>
    /// 原子性地锁定一批待处理任务（改为 Processing 状态）并返回。
    /// 使用 SKIP LOCKED 防止并发重复获取。
    /// </summary>
    /// <param name="take">最大获取数量</param>
    Task<List<TelemetryTask>> ClaimPendingTasksAsync(int take = 5);

    /// <summary>
    /// 根据文件名模糊搜索
    /// </summary>
    Task<List<TelemetryTask>> SearchByFileNameAsync(string fileName);

    /// <summary>
    /// 获取已过期且处于终态的任务（Success 或 Failed），用于安全清理
    /// </summary>
    Task<List<TelemetryTask>> GetExpiredCompletedTasksAsync();

    /// <summary>
    /// 分页查询任务列表
    /// </summary>
    Task<PagedResultDto<TelemetryTask>> GetListAsync(
        string fileName = null,
        int? status = null,
        int skipCount = 0,
        int maxResultCount = 10);

    /// <summary>
    /// 获取统计数据（原始数值元组）
    /// </summary>
    Task<(int TotalFiles, long TotalSize, int PendingCount, int ProcessingCount, int SuccessCount, int FailedCount, long TotalRecords)>
        GetStatisticsDataAsync();

    /// <summary>
    /// 批量更新任务（内部会调用 SaveChanges）
    /// </summary>
    Task UpdateManyAsync(List<TelemetryTask> tasks);
}