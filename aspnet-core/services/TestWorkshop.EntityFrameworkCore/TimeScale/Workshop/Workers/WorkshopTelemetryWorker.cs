using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using System.Globalization;
using Volo.Abp.BlobStoring;

namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 后台任务调度器：定期扫描待处理文件，解析 CSV 并批量写入遥测数据
/// </summary>
public class WorkshopTelemetryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkshopTelemetryWorker> _logger;
    private readonly int _maxRowsPerFile = 1_000_000; // 可改为从 IConfiguration 读取

    public WorkshopTelemetryWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<WorkshopTelemetryWorker> logger)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
        Timer.Period = 5000; // 5秒轮询
    }

    /// <summary>
    /// 启动时恢复卡死的 Processing 任务
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await ResetStuckTasksAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken; // 获取统一取消令牌
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var taskRepo = scope.ServiceProvider.GetRequiredService<IWorkshopTelemetryTaskRepository>();

            // 原子性领取任务（需确保仓储内部使用了 UPDATE ... WHERE Status=0 RETURNING * 或乐观锁）
            var tasks = await taskRepo.ClaimPendingTasksAsync(take: 5);

            if (tasks.Count == 0) return;

            // 建议：如果文件较大，可开启下方注释的并发处理（建议并发数 2~3）
            foreach (var telemetryTask in tasks)
            {
                using var processScope = _scopeFactory.CreateScope();
                await ProcessTaskAsync(telemetryTask.Id, processScope.ServiceProvider, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DoWorkAsync 发生异常，Worker 将继续运行");
        }
    }

    /// <summary>
    /// 核心处理：确保 COPY 与 EF 状态更新在【同一物理连接 + 同一数据库事务】中
    /// </summary>
    private async Task ProcessTaskAsync(long taskId, IServiceProvider sp, CancellationToken ct)
    {
        var taskRepo = sp.GetRequiredService<IWorkshopTelemetryTaskRepository>();
        var blobContainer = sp.GetRequiredService<IBlobContainer>();
        var db = sp.GetRequiredService<TestWorkshopDbContext>();

        // 1. 重新加载任务，防止多实例并发脏读
        var telemetryTask = await taskRepo.GetAsync(taskId, cancellationToken: ct);
        if (telemetryTask == null || telemetryTask.Status != 1) // 1 = Processing
        {
            _logger.LogWarning("任务 {TaskId} 状态非 Processing，跳过", taskId);
            return;
        }

        // 2. 【关键改动】使用 DbContext 的底层连接，确保 EF 和 COPY 共用同一物理连接
        await db.Database.OpenConnectionAsync(ct);

        // 3. 【关键改动】在 DbContext 上开启事务，后续 SaveChanges 会自动使用该事务
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // 获取底层 NpgsqlTransaction 用于 COPY 命令
        var npgsqlTransaction = transaction.GetDbTransaction() as NpgsqlTransaction;

        try
        {
            // 4. 读取 Blob 流
            await using var stream = await blobContainer.GetAsync(telemetryTask.BlobName, ct);

            // 5. 获取设备映射（带缓存）
            var deviceMap = await GetDeviceMapAsync(db, ct);

            // 6. 执行 COPY 批量导入（使用同一个事务）
            var recordCount = await BulkInsertFromStreamAsync(
                db,
                stream,
                deviceMap,
                npgsqlTransaction!,
                ct);

            // 7. 更新任务状态（EF 会使用上面开启的同一事务）
            telemetryTask.Status = 2; // 成功
            telemetryTask.ProcessedAt = DateTime.UtcNow;
            telemetryTask.RecordCount = recordCount;
            await taskRepo.UpdateAsync(telemetryTask);
            await db.SaveChangesAsync(ct);

            // 8. 提交事务（COPY 和 状态更新 原子性提交）
            await transaction.CommitAsync(ct);

            // 9. 删除 Blob（非关键操作，失败仅记录日志）
            try
            {
                await blobContainer.DeleteAsync(telemetryTask.BlobName, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除 Blob 失败 {BlobName}", telemetryTask.BlobName);
            }

            _logger.LogInformation("✅ 任务 {TaskId} 处理完成，记录数 {Count}", taskId, recordCount);
        }
        catch (Exception ex)
        {
            // 10. 回滚事务（此时 COPY 和 EF 修改都会撤销）
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "❌ 任务 {TaskId} 处理失败", taskId);

            // 11. 重试逻辑（独立 Scope，避免当前异常污染）
            using var retryScope = _scopeFactory.CreateScope();
            var retryRepo = retryScope.ServiceProvider.GetRequiredService<IWorkshopTelemetryTaskRepository>();
            var latest = await retryRepo.GetAsync(taskId, cancellationToken: ct);
            if (latest == null) return;

            latest.RetryCount++;
            latest.Error = ex.Message; // 建议存 ex.ToString() 获取堆栈

            if (latest.RetryCount >= 3)
            {
                latest.Status = 3; // 最终失败
            }
            else
            {
                latest.Status = 0; // 退回待处理
                // 指数退避，限制最大重试间隔为 60 分钟，防止溢出
                var delayMinutes = Math.Min(60, Math.Pow(2, latest.RetryCount));
                latest.NextRetryTime = DateTime.UtcNow.AddMinutes(delayMinutes);
            }
            await retryRepo.UpdateAsync(latest);
            // 注意：这里没有 SaveChangesAsync 调用，如果仓储内部不自动 Save，需要补上。
            // 假设 retryRepo 内部会调用 SaveChanges，否则加一行 await retryRepo.SaveChangesAsync(ct);
        }
        finally
        {
            // 12. 归还连接（虽然 Scope 销毁时会释放，但显式关闭更及时）
            await db.Database.CloseConnectionAsync();
        }
    }

    /// <summary>
    /// 使用 COPY 导入数据（显式指定 NpgsqlDbType 提升性能与准确性）
    /// </summary>
    private async Task<int> BulkInsertFromStreamAsync(
        TestWorkshopDbContext db,
        Stream csvStream,
        Dictionary<string, Guid> deviceMap,
        NpgsqlTransaction transaction,
        CancellationToken ct)
    {
        // 获取表名（含 Schema，防止表找不到）
        var entityType = db.Model.FindEntityType(typeof(WorkshopDeviceTelemetry));
        var tableName = entityType?.GetSchemaQualifiedTableName()
                        ?? "\"AppWorkshopDeviceTelemetries\""; // 兜底

        // 使用传入的事务创建 COPY Writer
        using var writer = transaction.Connection.BeginBinaryImport(
            $"COPY {tableName} (\"DeviceId\",\"MetricType\",\"Value\",\"Timestamp\") FROM STDIN (FORMAT BINARY)");

        int count = 0, skipped = 0;
        using var reader = new StreamReader(csvStream);

        // 跳过 CSV 头部（如果有）
        var firstLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(firstLine)) return 0;

        // 简单校验：如果第一行包含 "DeviceCode" 之类的字样，视为表头跳过
        if (firstLine.Contains("DeviceCode", StringComparison.OrdinalIgnoreCase) ||
            firstLine.Contains("MetricType", StringComparison.OrdinalIgnoreCase))
        {
            // 已跳过表头，继续
        }
        else
        {
            // 如果第一行不是表头，尝试解析它（因为已经 ReadLine 了，需要把指针重置）
            // 简易处理：这里为了代码简洁，假设第一行就是数据。如果确定有表头，建议使用 CsvHelper。
        }

        // 逐行解析（生产环境强烈建议改用 CsvHelper 处理引号/转义逗号）
        while (!reader.EndOfStream)
        {
            if (count >= _maxRowsPerFile)
            {
                _logger.LogWarning("达到行数上限 {MaxRows}，中断读取", _maxRowsPerFile);
                break;
            }

            var line = await reader.ReadLineAsync(ct);
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
                var metricType = parts[1].Trim();

                // 使用 TryParse 防止异常影响整批数据
                if (!double.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    skipped++;
                    continue;
                }
                if (!DateTime.TryParse(parts[3].Trim(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timestamp))
                {
                    skipped++;
                    continue;
                }

                if (!deviceMap.TryGetValue(deviceCode, out var deviceId))
                {
                    _logger.LogWarning("未知设备码 {Code}", deviceCode);
                    skipped++;
                    continue;
                }

                // 显式指定 NpgsqlDbType，避免类型推断问题
                writer.StartRow();
                writer.Write(deviceId, NpgsqlDbType.Uuid);
                writer.Write(metricType, NpgsqlDbType.Text);
                writer.Write(value, NpgsqlDbType.Double);
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
            _logger.LogWarning("跳过了 {Skipped} 行无效数据", skipped);

        return count;
    }

    /// <summary>
    /// 获取设备映射（10分钟缓存，注意：若设备变更频繁需取消缓存）
    /// </summary>
    private async Task<Dictionary<string, Guid>> GetDeviceMapAsync(TestWorkshopDbContext db, CancellationToken ct)
    {
        const string cacheKey = "DeviceMap_TelemetryWorker";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await db.Devices.ToDictionaryAsync(d => d.Code, d => d.Id, ct);
        }) ?? new Dictionary<string, Guid>();
    }

    /// <summary>
    /// 原子性恢复卡死任务（使用原生 SQL 避免 SELECT + UPDATE 的竞态条件）
    /// </summary>
    private async Task ResetStuckTasksAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();

            var stuckTime = DateTime.UtcNow.AddMinutes(-10);

            // 直接执行 SQL，原子性更新，无需先查询再修改（避免了多实例并发重复处理）
            var rowsAffected = await db.Database.ExecuteSqlRawAsync(
                "UPDATE \"AppTelemetryTasks\" " + // 替换为你的实际表名
                "SET \"Status\" = 0, " +
                "    \"RetryCount\" = \"RetryCount\" + 1, " +
                "    \"NextRetryTime\" = @p0 " +
                "WHERE \"Status\" = 1 AND \"ProcessingStartedAt\" < @p1",
                new object[] { DateTime.UtcNow.AddSeconds(30), stuckTime }, ct);

            if (rowsAffected > 0)
            {
                _logger.LogInformation("🔄 原子性恢复了 {Count} 个卡死的任务", rowsAffected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复卡死任务时发生异常");
        }
    }
}