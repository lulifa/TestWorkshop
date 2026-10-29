using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Uow;

namespace TestWorkshop;

/// <summary>
/// 统一的过期数据清理任务：审计日志、安全日志、已恢复报警。
/// </summary>
public class DataRetentionCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataRetentionCleanupWorker> _logger;
    private readonly DataRetentionOptions _options;

    public DataRetentionCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        ILogger<DataRetentionCleanupWorker> logger,
        IOptions<DataRetentionOptions> options)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _options.Validate();

        Timer.Period = (int)TimeSpan
            .FromHours(_options.CleanupIntervalHours)
            .TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(
        PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var unitOfWorkManager =
                serviceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = unitOfWorkManager.Begin(
                requiresNew: true,
                isTransactional: false);
            var clock = serviceProvider.GetRequiredService<IClock>();
            var auditLogRepository =
                serviceProvider.GetRequiredService<IRepository<AuditLog, Guid>>();
            var securityLogRepository =
                serviceProvider.GetRequiredService<IRepository<IdentitySecurityLog, Guid>>();
            var alarmRepository =
                serviceProvider.GetRequiredService<IRepository<WorkshopAlarm, Guid>>();

            var now = clock.Now;
            var auditLogCutoff = now.AddDays(-_options.AuditLogDays);
            var securityLogCutoff = now.AddDays(-_options.SecurityLogDays);
            var clearedAlarmCutoff = now.AddDays(-_options.ClearedAlarmDays);
            _logger.LogInformation(
                "开始执行数据保留清理：批次大小 {BatchSize}，审计日志 {AuditLogDays} 天，安全日志 {SecurityLogDays} 天，已恢复报警 {ClearedAlarmDays} 天",
                _options.CleanupBatchSize,
                _options.AuditLogDays,
                _options.SecurityLogDays,
                _options.ClearedAlarmDays);

            await RunCleanupAsync(
                "审计日志",
                auditLogRepository,
                item => item.ExecutionTime < auditLogCutoff,
                ct);

            await RunCleanupAsync(
                "安全日志",
                securityLogRepository,
                item => item.CreationTime < securityLogCutoff,
                ct);

            await RunCleanupAsync(
                "已恢复报警",
                alarmRepository,
                item => item.Status == WorkshopAlarmStatus.Cleared
                        && item.ClearedAt.HasValue
                        && item.ClearedAt.Value < clearedAlarmCutoff,
                ct);

            await uow.CompleteAsync(ct);
            _logger.LogInformation("数据保留清理任务执行完成");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据保留清理任务执行失败，Worker 将继续运行");
        }
    }

    private async Task RunCleanupAsync<TEntity>(
        string displayName,
        IRepository<TEntity, Guid> repository,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
        where TEntity : class, IEntity<Guid>
    {
        try
        {
            var deletedCount = await DeleteInBatchesAsync(
                repository,
                predicate,
                ct);

            _logger.LogInformation(
                "{DisplayName}清理完成，共删除 {DeletedCount} 条",
                displayName,
                deletedCount);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{DisplayName}清理失败，继续执行其他清理任务",
                displayName);
        }
    }

    private async Task<int> DeleteInBatchesAsync<TEntity>(
        IRepository<TEntity, Guid> repository,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
        where TEntity : class, IEntity<Guid>
    {
        var totalDeleted = 0;

        while (!ct.IsCancellationRequested)
        {
            var query = await repository.GetQueryableAsync();
            var ids = await query
                .Where(predicate)
                .OrderBy(item => item.Id)
                .Select(item => item.Id)
                .Take(_options.CleanupBatchSize)
                .ToListAsync(ct);

            if (ids.Count == 0)
            {
                break;
            }

            await repository.DeleteDirectAsync(
                item => ids.Contains(item.Id),
                cancellationToken: ct);

            totalDeleted += ids.Count;

            if (ids.Count < _options.CleanupBatchSize)
            {
                break;
            }
        }

        return totalDeleted;
    }
}
