using Microsoft.Extensions.Logging;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace TestWorkshop;

public class LogCleanupBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LogCleanupBackgroundWorker> _logger;
    private const int RetentionDays = 365;

    public LogCleanupBackgroundWorker(
      AbpAsyncTimer timer,
      IServiceScopeFactory scopeFactory,
      ILogger<LogCleanupBackgroundWorker> logger)
      : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        Timer.Period = (int)new TimeSpan(24, 0, 0).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken;

        try
        {
            using var scope = _scopeFactory.CreateScope();

            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var auditLogRepo = scope.ServiceProvider.GetRequiredService<IRepository<AuditLog, Guid>>();
            var securityLogRepo = scope.ServiceProvider.GetRequiredService<IRepository<IdentitySecurityLog, Guid>>();

            var cutoffTime = clock.Now.AddDays(-RetentionDays);
            _logger.LogInformation("开始清理过期日志（保留 {RetentionDays} 天，截止 {CutoffTime:yyyy-MM-dd HH:mm:ss}）", RetentionDays, cutoffTime);

            // 清理审计日志
            await auditLogRepo.DeleteDirectAsync(item => item.ExecutionTime < cutoffTime, ct);

            // 清理安全日志
            await securityLogRepo.DeleteDirectAsync(item => item.CreationTime < cutoffTime, ct);

            _logger.LogInformation("过期日志清理完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期日志时发生异常，Worker 将继续运行");
        }
    }
}
