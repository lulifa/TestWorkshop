namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 测试报告获取记录仓储实现。
/// </summary>
public class EfCoreWorkshopOrderReportFetchRecordRepository :
    EfCoreRepository<TestWorkshopDbContext, WorkshopOrderReportFetchRecord, Guid>,
    IWorkshopOrderReportFetchRecordRepository
{
    public EfCoreWorkshopOrderReportFetchRecordRepository(
        IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<WorkshopOrderReportFetchRecord>> GetListByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAttemptCountAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.CountAsync(
            x => x.OrderId == orderId,
            cancellationToken);
    }

    public async Task<bool> HasActiveRecordAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(
            x => x.OrderId == orderId
                 && (x.Status == WorkshopTestReportFetchRecordStatus.Pending
                     || x.Status == WorkshopTestReportFetchRecordStatus.Processing),
            cancellationToken);
    }

    public async Task<List<WorkshopOrderReportFetchRecord>> ClaimPendingRecordsAsync(
        int take,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var entityType = dbContext.Model.FindEntityType(typeof(WorkshopOrderReportFetchRecord));
        var tableName = entityType?.GetTableName()
                        ?? "AppWorkshopOrderReportFetchRecords";
        var schema = entityType?.GetSchema();
        var fullTableName = schema.IsNullOrWhiteSpace()
            ? $"\"{tableName}\""
            : $"\"{schema}\".\"{tableName}\"";
        var sql = $"""
                   UPDATE {fullTableName}
                   SET "Status" = @ProcessingStatus,
                       "StartedAt" = @Now
                   WHERE "Id" IN (
                       SELECT "Id"
                       FROM {fullTableName}
                       WHERE "Status" = @PendingStatus
                         AND "ScheduledAt" <= @Now
                         AND "IsDeleted" = false
                       ORDER BY "CreationTime"
                       LIMIT @Take
                       FOR UPDATE SKIP LOCKED
                   )
                   RETURNING *
                   """;

        return await dbContext.Set<WorkshopOrderReportFetchRecord>()
            .FromSqlRaw(
                sql,
                new NpgsqlParameter("@ProcessingStatus", NpgsqlDbType.Integer)
                {
                    Value = (int)WorkshopTestReportFetchRecordStatus.Processing
                },
                new NpgsqlParameter("@PendingStatus", NpgsqlDbType.Integer)
                {
                    Value = (int)WorkshopTestReportFetchRecordStatus.Pending
                },
                new NpgsqlParameter("@Now", NpgsqlDbType.Timestamp)
                {
                    Value = DateTime.SpecifyKind(now, DateTimeKind.Unspecified)
                },
                new NpgsqlParameter("@Take", NpgsqlDbType.Integer)
                {
                    Value = take
                })
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WorkshopOrderReportFetchRecord>> GetStaleProcessingRecordsAsync(
        DateTime staleBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted
                        && x.Status == WorkshopTestReportFetchRecordStatus.Processing
                        && x.StartedAt.HasValue
                        && x.StartedAt.Value < staleBefore)
            .OrderBy(x => x.StartedAt)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }
}
