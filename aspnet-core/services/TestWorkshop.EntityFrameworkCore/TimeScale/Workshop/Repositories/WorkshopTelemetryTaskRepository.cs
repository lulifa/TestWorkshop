namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 遥测任务仓储实现 - EF Core
/// </summary>
public class WorkshopTelemetryTaskRepository :
    EfCoreRepository<TestWorkshopDbContext, WorkshopTelemetryTask, long>,
    IWorkshopTelemetryTaskRepository
{
    public WorkshopTelemetryTaskRepository(
        IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    /// <summary>
    /// 原子操作：锁定并获取待处理任务，立即标记为 Processing
    /// </summary>
    public async Task<List<WorkshopTelemetryTask>> ClaimPendingTasksAsync(int take = 5)
    {
        var dbContext = await GetDbContextAsync();

        var entityType = dbContext.Model.FindEntityType(typeof(WorkshopTelemetryTask));
        var tableName = entityType?.GetTableName();
        var schema = entityType?.GetSchema();
        var fullTableName = string.IsNullOrWhiteSpace(schema)
            ? $@"""{tableName}"""
            : $@"""{schema}"".""{tableName}""";

        var now = DateTime.UtcNow;

        // ✅ 去掉 AND ""IsDeleted"" = false
        var sql = $@"
            UPDATE {fullTableName}
            SET ""Status"" = 1,
                ""ProcessingStartedAt"" = @Now
            WHERE ""Id"" IN (
                SELECT ""Id"" FROM {fullTableName}
                WHERE ""Status"" = 0
                  AND (""NextRetryTime"" IS NULL OR ""NextRetryTime"" <= @Now)
                ORDER BY ""CreatedAt""
                LIMIT @Take
                FOR UPDATE SKIP LOCKED
            )
            RETURNING *";

        var tasks = await dbContext.TelemetryTasks
            .FromSqlRaw(sql,
                new NpgsqlParameter("@Now", now),
                new NpgsqlParameter("@Take", take))
            .IgnoreQueryFilters()
            .ToListAsync();

        return tasks;
    }

    // ========== 查询方法（需 JOIN FileObject） ==========

    /// <summary>
    /// 根据文件名搜索（JOIN FileObject 表）
    /// </summary>
    public async Task<List<WorkshopTelemetryTask>> SearchByFileNameAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return new List<WorkshopTelemetryTask>();

        var dbContext = await GetDbContextAsync();

        // ✅ 去掉 where task.IsDeleted == false
        var query = from task in dbContext.TelemetryTasks
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id
                    where file.FileName.Contains(fileName)
                    orderby task.CreatedAt descending
                    select task;

        return await query.ToListAsync();
    }

    /// <summary>
    /// 分页查询任务列表（JOIN FileObject 表获取文件信息）
    /// </summary>
    public async Task<PagedResultDto<WorkshopTelemetryTask>> GetPagedListAsync(
        string? fileName = null,
        int? status = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {
        var dbContext = await GetDbContextAsync();

        // ✅ 去掉 where task.IsDeleted == false
        var query = from task in dbContext.TelemetryTasks
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id
                    select new { Task = task, File = file };

        // 按文件名过滤
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            query = query.Where(x => x.File.FileName.Contains(fileName));
        }

        // 按状态过滤
        if (status.HasValue)
        {
            query = query.Where(x => x.Task.Status == status.Value);
        }

        // 统计总数
        var totalCount = await query.CountAsync();

        // 分页查询
        var items = await query
            .OrderByDescending(x => x.Task.CreatedAt)
            .Skip(skipCount)
            .Take(maxResultCount)
            .Select(x => x.Task)
            .ToListAsync();

        return new PagedResultDto<WorkshopTelemetryTask>(totalCount, items);
    }

    /// <summary>
    /// 获取任务关联的 FileObject（用于展示文件名/大小）
    /// </summary>
    public async Task<FileObject> GetFileObjectAsync(Guid fileObjectId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.FileObjects
            .FirstOrDefaultAsync(f => f.Id == fileObjectId);
    }

    // ========== 统计方法（JOIN FileObject 表） ==========

    /// <summary>
    /// 获取统计信息（JOIN FileObject 表获取文件大小）
    /// </summary>
    public async Task<(int TotalFiles, long TotalSize, int PendingCount, int ProcessingCount, int SuccessCount, int FailedCount, long TotalRecords)>
        GetStatisticsDataAsync()
    {
        var dbContext = await GetDbContextAsync();

        // ✅ 去掉 where task.IsDeleted == false
        var query = from task in dbContext.TelemetryTasks
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id
                    select new { Task = task, File = file };

        // 统计各项数据（一次查询完成）
        var totalFiles = await query.CountAsync();
        var totalSize = await query.SumAsync(x => x.File.FileSize);
        var pendingCount = await query.CountAsync(x => x.Task.Status == 0);
        var processingCount = await query.CountAsync(x => x.Task.Status == 1);
        var successCount = await query.CountAsync(x => x.Task.Status == 2);
        var failedCount = await query.CountAsync(x => x.Task.Status == 3);
        var totalRecords = await query
            .Where(x => x.Task.Status == 2)
            .SumAsync(x => x.Task.RecordCount ?? 0);

        return (totalFiles, totalSize, pendingCount, processingCount, successCount, failedCount, totalRecords);
    }

    // ========== 过期任务查询（返回 Task + FileObject，用于级联删除） ==========

    /// <summary>
    /// 获取已过期且可清理的任务（返回 Task 和关联的 FileObject）
    /// </summary>
    public async Task<List<(WorkshopTelemetryTask Task, FileObject FileObject)>> GetExpiredCompletedTasksAsync(int take = 100)
    {
        var dbContext = await GetDbContextAsync();

        var now = DateTime.UtcNow;

        // ✅ 没有 IsDeleted 条件
        var query = from task in dbContext.TelemetryTasks
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id
                    where task.ExpiresAt <= now
                      && (task.Status == 2 || task.Status == 3) // Success 或 Failed
                    orderby task.CreatedAt
                    select new { Task = task, File = file };

        var results = await query.Take(take).ToListAsync();

        return results.Select(x => (x.Task, x.File)).ToList();
    }

    /// <summary>
    /// 批量更新
    /// </summary>
    public async Task UpdateManyAsync(List<WorkshopTelemetryTask> tasks)
    {
        if (tasks == null || tasks.Count == 0) return;

        var dbSet = await GetDbSetAsync();
        dbSet.UpdateRange(tasks);
        var dbContext = await GetDbContextAsync();
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 物理删除任务（真删除）
    /// </summary>
    public async Task DeleteAsync(WorkshopTelemetryTask task)
    {
        var dbSet = await GetDbSetAsync();
        dbSet.Remove(task);
        var dbContext = await GetDbContextAsync();
        await dbContext.SaveChangesAsync();
    }
}