using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using System.Globalization;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;

namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 后台任务调度器：定期扫描待处理文件，解析 CSV 并批量写入遥测数据
/// </summary>
public class TelemetryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TelemetryWorker> _logger;

    public TelemetryWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<TelemetryWorker> logger)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
        Timer.Period = 5000; // 每 5 秒轮询一次
    }

    /// <summary>
    /// 启动时先恢复所有卡死的 Processing 任务，再开始正式调度
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await ResetStuckTasksAsync();
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 周期性执行的核心逻辑
    /// </summary>
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        try
        {
            // 第一阶段：通过仓储原子性地领取任务（状态已被改为 Processing）
            using var scope = _scopeFactory.CreateScope();
            var taskRepo = scope.ServiceProvider.GetRequiredService<ITelemetryTaskRepository>();
            var tasks = await taskRepo.ClaimPendingTasksAsync(take: 5);

            if (tasks.Count == 0) return;

            // 第二阶段：逐个任务独立处理（可选改为并发，详见下方注释）
            foreach (var task in tasks)
            {
                using var processScope = _scopeFactory.CreateScope();
                await ProcessTaskAsync(task.Id, processScope.ServiceProvider);
            }

            // 可选：轻量并发处理（如果文件较大、解析耗时，可开启）
            // var semaphore = new SemaphoreSlim(3);
            // var handlers = tasks.Select(async t =>
            // {
            //     await semaphore.WaitAsync();
            //     try
            //     {
            //         using var processScope = _scopeFactory.CreateScope();
            //         await ProcessTaskAsync(t.Id, processScope.ServiceProvider);
            //     }
            //     finally { semaphore.Release(); }
            // });
            // await Task.WhenAll(handlers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DoWorkAsync 发生异常，Worker 将继续运行");
        }
    }

    /// <summary>
    /// 处理单个任务：从 Blob 读取 CSV → 批量 COPY 插入 → 更新任务状态
    /// 使用 Npgsql 事务确保 COPY 和状态更新完全原子性
    /// </summary>
    private async Task ProcessTaskAsync(long taskId, IServiceProvider sp)
    {
        var taskRepo = sp.GetRequiredService<ITelemetryTaskRepository>();
        var blobContainer = sp.GetRequiredService<IBlobContainer>();
        var dbContext = sp.GetRequiredService<TestWorkshopDbContext>();

        // 重新加载最新状态，防止多实例并发脏写
        var task = await taskRepo.GetAsync(taskId);
        if (task == null || task.Status != 1)
        {
            _logger.LogWarning("任务 {TaskId} 状态非 Processing，跳过", taskId);
            return;
        }

        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        // 重点：统一使用 Npgsql 事务，并同步给 EF，保证一致性
        await using var transaction = await conn.BeginTransactionAsync();
        await dbContext.Database.UseTransactionAsync(transaction);

        try
        {
            // 读取 Blob 流（记得释放）
            await using var stream = await blobContainer.GetAsync(task.BlobName);
            var deviceMap = await GetDeviceMapAsync(dbContext);
            var recordCount = await BulkInsertFromStreamAsync(dbContext, stream, deviceMap, conn, transaction);

            // 更新任务为成功
            task.Status = 2;
            task.ProcessedAt = DateTime.UtcNow;
            task.RecordCount = recordCount;
            await taskRepo.UpdateAsync(task);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // 删除 Blob（非关键操作，失败不影响任务状态）
            try
            {
                await blobContainer.DeleteAsync(task.BlobName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除 Blob 失败 {BlobName}", task.BlobName);
            }

            _logger.LogInformation("✅ 任务 {TaskId} 处理完成，记录数 {Count}", task.Id, recordCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "❌ 任务 {TaskId} 处理失败", task.Id);

            // 重试逻辑（独立 scope，避免上下文污染）
            using var retryScope = _scopeFactory.CreateScope();
            var retryRepo = retryScope.ServiceProvider.GetRequiredService<ITelemetryTaskRepository>();
            var latest = await retryRepo.GetAsync(taskId);
            if (latest == null) return;

            latest.RetryCount++;
            latest.Error = ex.Message;

            if (latest.RetryCount >= 3)
            {
                latest.Status = 3;   // 标记为最终失败
            }
            else
            {
                latest.Status = 0;   // 退回 Pending，等待重试
                // 指数退避：2^RetryCount 分钟
                latest.NextRetryTime = DateTime.UtcNow.AddSeconds(Math.Pow(2, latest.RetryCount) * 60);
            }
            await retryRepo.UpdateAsync(latest);
        }
    }

    /// <summary>
    /// 使用 PostgreSQL COPY 命令高性能批量导入遥测记录
    /// </summary>
    private async Task<int> BulkInsertFromStreamAsync(
        TestWorkshopDbContext db,
        Stream csvStream,
        Dictionary<string, Guid> deviceMap,
        NpgsqlConnection conn,
        NpgsqlTransaction transaction)
    {
        var entityType = db.Model.FindEntityType(typeof(DeviceTelemetry));
        var tableName = entityType?.GetTableName() ?? "AppDeviceTelemetries"; // 兜底

        using var writer = conn.BeginBinaryImport(
            $"COPY \"{tableName}\" (\"DeviceId\",\"Metric\",\"Value\",\"Timestamp\") FROM STDIN (FORMAT BINARY)");

        int count = 0, skipped = 0;
        using var reader = new StreamReader(csvStream);

        const int maxRows = 1_000_000;

        while (!reader.EndOfStream)
        {
            if (count >= maxRows)
            {
                _logger.LogWarning("任务 CSV 超过行数上限 {MaxRows}，中断读取", maxRows);
                break;
            }

            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 4)
            {
                skipped++;
                continue;
            }

            try
            {
                var deviceCode = parts[0].Trim();
                var metric = parts[1].Trim();
                var value = double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                var timestamp = DateTime.Parse(parts[3].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

                if (!deviceMap.TryGetValue(deviceCode, out var deviceId))
                {
                    _logger.LogWarning("未知设备码 {Code}", deviceCode);
                    skipped++;
                    continue;
                }

                writer.StartRow();
                writer.Write(deviceId);
                writer.Write(metric);
                writer.Write(value);
                writer.Write(timestamp, NpgsqlDbType.TimestampTz);
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CSV 行解析失败: {Line}", line);
                skipped++;
            }
        }

        writer.Complete();
        if (skipped > 0)
            _logger.LogWarning("跳过 {Skipped} 行无效数据", skipped);

        return count;
    }

    /// <summary>
    /// 获取设备映射表（带 10 分钟内存缓存）
    /// </summary>
    private async Task<Dictionary<string, Guid>> GetDeviceMapAsync(TestWorkshopDbContext db)
    {
        const string cacheKey = "DeviceMap_TelemetryWorker";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await db.Devices.ToDictionaryAsync(d => d.Code, d => d.Id);
        }) ?? new Dictionary<string, Guid>();
    }

    /// <summary>
    /// 恢复所有卡在 Processing 状态超过 10 分钟的任务（进程崩溃导致）
    /// </summary>
    private async Task ResetStuckTasksAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITelemetryTaskRepository>();
            var db = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();

            var stuckTime = DateTime.UtcNow.AddMinutes(-10);
            var stuckTasks = await db.TelemetryTasks
                .Where(t => t.Status == 1 && t.ProcessingStartedAt < stuckTime)
                .ToListAsync();

            if (!stuckTasks.Any()) return;

            foreach (var t in stuckTasks)
            {
                t.Status = 0;   // 退回 Pending
                t.RetryCount++;
                t.NextRetryTime = DateTime.UtcNow.AddSeconds(30);
            }
            await repo.UpdateManyAsync(stuckTasks);
            _logger.LogInformation("恢复了 {Count} 个卡死的任务", stuckTasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复卡死任务时发生异常");
        }
    }
}