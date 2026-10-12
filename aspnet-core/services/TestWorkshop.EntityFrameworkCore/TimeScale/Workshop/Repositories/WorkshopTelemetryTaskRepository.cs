namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 遥测任务仓储实现 - EF Core
/// </summary>
public class WorkshopTelemetryTaskRepository :
    EfCoreRepository<TestWorkshopDbContext, WorkshopTelemetryTask, long>,
    IWorkshopTelemetryTaskRepository
{
    // 任务状态：2 = 成功
    private const int SuccessStatusValue = 2;

    // 界面上“状态”列的虚拟状态：已替换（被后一次上传顶掉的历史任务）
    private const int ReplacedStatusValue = 4;

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
    /// 分页查询任务列表（JOIN FileObject 表获取文件信息）
    /// </summary>
    public async Task<PagedResultDto<WorkshopTelemetryTask>> GetPagedListAsync(
        Guid? orderId = null,
        string orderNo = null,
        string fileName = null,
        int? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string testedDeviceCode = null,
        bool? hasOrder = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {
        var dbContext = await GetDbContextAsync();

        // ✅ 去掉 where task.IsDeleted == false
        var query = from task in dbContext.TelemetryTasks
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id into fileGroup
                    from file in fileGroup.DefaultIfEmpty()
                    join order in dbContext.Orders on task.OrderId equals (Guid?)order.Id into orderGroup
                    from order in orderGroup.DefaultIfEmpty()
                    select new { Task = task, File = file, Order = order };

        if (orderId.HasValue)
        {
            query = query.Where(x => x.Task.OrderId == orderId.Value);
        }

        // 按订单号模糊匹配（未关联订单的任务不会命中）
        if (!orderNo.IsNullOrWhiteSpace())
        {
            var orderNoFilter = orderNo.Trim();
            query = query.Where(x => x.Order != null && x.Order.OrderNo.Contains(orderNoFilter));
        }

        // 按文件名过滤
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            query = query.Where(x => x.File != null && x.File.FileName.Contains(fileName));
        }

        // 按状态过滤。口径与界面上“状态”列的显示保持一致：
        //   ReplacedStatusValue 是界面的虚拟状态“已替换”，对应被后一次上传顶掉的历史任务；
        //   “成功”需要排除这些历史任务，因为它们显示为“已替换”而不是“成功”。
        if (status.HasValue)
        {
            if (status.Value == ReplacedStatusValue)
            {
                query = query.Where(x => x.Task.ReplacedByTaskId != null);
            }
            else if (status.Value == SuccessStatusValue)
            {
                query = query.Where(x => x.Task.Status == SuccessStatusValue
                                         && x.Task.ReplacedByTaskId == null);
            }
            else
            {
                query = query.Where(x => x.Task.Status == status.Value);
            }
        }

        if (startTime.HasValue)
        {
            query = query.Where(x => x.Task.CreatedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.Task.CreatedAt <= endTime.Value);
        }

        // 是否关联订单：未匹配订单的数据即非法数据
        if (hasOrder.HasValue)
        {
            query = hasOrder.Value
                ? query.Where(x => x.Task.OrderId != null)
                : query.Where(x => x.Task.OrderId == null);
        }

        // 被测设备编码存在 FileObject.ExtraProperties 的 JSON 文本里，用 LIKE 匹配。
        if (!testedDeviceCode.IsNullOrWhiteSpace())
        {
            var pattern = $"%\"{TestWorkshopConsts.TelemetryInputExtraPropertiesPrefix}TestInfo.TestedDeviceCode\":\"{testedDeviceCode.Trim()}%";
            var fileObjectIds = await GetMatchingFileObjectIdsAsync(dbContext, pattern);
            query = fileObjectIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(x => x.File != null && fileObjectIds.Contains(x.File.Id));
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
    public async Task<(int TotalFiles, long TotalSize, int PendingCount, int ProcessingCount, int SuccessCount, int FailedCount, int ReplacedCount, int UnmatchedOrderCount, long TotalRecords)>
        GetStatisticsDataAsync()
    {
        var dbContext = await GetDbContextAsync();

        // ✅ 去掉 where task.IsDeleted == false
        var query = from task in dbContext.TelemetryTasks
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id into fileGroup
                    from file in fileGroup.DefaultIfEmpty()
                    select new { Task = task, File = file };

        // 统计各项数据（一次查询完成）
        var totalFiles = await query.CountAsync();
        var totalSize = (await query.SumAsync(x => x.File != null ? (long?)x.File.FileSize : null)) ?? 0;
        var pendingCount = await query.CountAsync(x => x.Task.Status == 0);
        var processingCount = await query.CountAsync(x => x.Task.Status == 1);
        // “成功”与列表口径一致：排除被替换的历史任务（它们显示为“已替换”）
        var successCount = await query.CountAsync(
            x => x.Task.Status == SuccessStatusValue && x.Task.ReplacedByTaskId == null);
        var failedCount = await query.CountAsync(x => x.Task.Status == 3);
        var replacedCount = await query.CountAsync(x => x.Task.ReplacedByTaskId != null);
        var unmatchedOrderCount = await query.CountAsync(x => x.Task.OrderId == null);
        var totalRecords = await query
            .Where(x => x.Task.Status == 2)
            .SumAsync(x => x.Task.RecordCount ?? 0);

        return (totalFiles, totalSize, pendingCount, processingCount, successCount, failedCount, replacedCount, unmatchedOrderCount, totalRecords);
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
                    join file in dbContext.FileObjects on task.FileObjectId equals file.Id into fileGroup
                    from file in fileGroup.DefaultIfEmpty()
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

    /// <summary>
    /// 按 FileObject.ExtraProperties（JSON 文本）里的关键字找出匹配的文件ID。
    /// 这里走原生 SQL，返回的是全集，最终结果仍由外层的任务查询做租户过滤。
    /// </summary>
    private static async Task<List<Guid>> GetMatchingFileObjectIdsAsync(
        TestWorkshopDbContext dbContext,
        string pattern)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(FileObject));
        var rawTableName = entityType?.GetTableName() ?? "AppFileObjects";
        var schema = entityType?.GetSchema();
        var fullTableName = string.IsNullOrWhiteSpace(schema)
            ? $@"""{rawTableName}"""
            : $@"""{schema}"".""{rawTableName}""";

        return await dbContext.Database
            .SqlQueryRaw<Guid>(
                $@"SELECT ""Id"" AS ""Value"" FROM {fullTableName} WHERE ""ExtraProperties"" LIKE {{0}}",
                pattern)
            .ToListAsync();
    }

}
