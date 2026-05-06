using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;

namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 遥测任务仓储实现 - EF Core
/// </summary>
public class TelemetryTaskRepository :
    EfCoreRepository<TestWorkshopDbContext, TelemetryTask, long>,
    ITelemetryTaskRepository
{
    private readonly ICurrentTenant _currentTenant;
    public TelemetryTaskRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider, ICurrentTenant currentTenant)
        : base(dbContextProvider)
    {
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 原子操作：锁定并获取待处理任务，立即标记为 Processing
    /// </summary>
    public async Task<List<TelemetryTask>> ClaimPendingTasksAsync(int take = 5)
    {
        var dbContext = await GetDbContextAsync();

        // 短事务，锁定行后马上修改状态
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        // FOR UPDATE SKIP LOCKED 防止并发重复获取
        var sql = @"
                SELECT * FROM ""TelemetryTasks""
                WHERE ""Status"" = 0
                  AND (""NextRetryTime"" IS NULL OR ""NextRetryTime"" <= @Now)
                ORDER BY ""CreatedAt""
                LIMIT @Take
                FOR UPDATE SKIP LOCKED";

        var tasks = await dbContext.TelemetryTasks
            .FromSqlRaw(sql,
                new NpgsqlParameter("@Now", DateTime.UtcNow),
                new NpgsqlParameter("@Take", take))
            .ToListAsync();

        if (tasks.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var t in tasks)
            {
                t.Status = 1;                  // Processing
                t.ProcessingStartedAt = now;
            }
            await dbContext.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return tasks;
    }

    public async Task<List<TelemetryTask>> SearchByFileNameAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return new List<TelemetryTask>();

        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(t => t.FileName.Contains(fileName) && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 只获取已过期且状态为 Success(2) 或 Failed(3) 的任务，用于安全清理
    /// </summary>
    public async Task<List<TelemetryTask>> GetExpiredCompletedTasksAsync()
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(t => t.ExpiresAt <= DateTime.UtcNow
                        && !t.IsDeleted
                        && (t.Status == 2 || t.Status == 3))
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResultDto<TelemetryTask>> GetListAsync(
        string fileName = null,
        int? status = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(t => !t.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(fileName))
            query = query.Where(t => t.FileName.Contains(fileName));
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<TelemetryTask>(totalCount, items);
    }

    public async Task<(int TotalFiles, long TotalSize, int PendingCount, int ProcessingCount, int SuccessCount, int FailedCount, long TotalRecords)>
        GetStatisticsDataAsync()
    {
        var dbSet = await GetDbSetAsync();

        var totalFiles = await dbSet.CountAsync(t => !t.IsDeleted);
        var totalSize = await dbSet.Where(t => !t.IsDeleted).SumAsync(t => t.FileSize);
        var pendingCount = await dbSet.CountAsync(t => t.Status == 0);
        var processingCount = await dbSet.CountAsync(t => t.Status == 1);
        var successCount = await dbSet.CountAsync(t => t.Status == 2);
        var failedCount = await dbSet.CountAsync(t => t.Status == 3);
        var totalRecords = await dbSet
            .Where(t => t.Status == 2)
            .SumAsync(t => t.RecordCount ?? 0);

        return (totalFiles, totalSize, pendingCount, processingCount, successCount, failedCount, totalRecords);
    }

    public async Task UpdateManyAsync(List<TelemetryTask> tasks)
    {
        if (tasks == null || tasks.Count == 0) return;

        var dbSet = await GetDbSetAsync();
        dbSet.UpdateRange(tasks);
        var dbContext = await GetDbContextAsync();
        await dbContext.SaveChangesAsync();
    }
}