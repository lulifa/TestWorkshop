using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using System.Globalization;

namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 后台任务调度器：定期扫描待处理文件，解析 CSV 并批量写入遥测数据
/// </summary>
public class WorkshopTelemetryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkshopTelemetryWorker> _logger;
    private readonly int _maxRowsPerFile = 1_000_000;

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
        Timer.Period = 5000;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await ResetStuckTasksAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var ct = workerContext.CancellationToken;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var taskRepo = scope.ServiceProvider.GetRequiredService<IWorkshopTelemetryTaskRepository>();

            // ✅ 依然用原来的方法获取待处理任务（Status = 0）
            var tasks = await taskRepo.ClaimPendingTasksAsync(take: 5);
            if (tasks.Count == 0) return;

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

    private async Task ProcessTaskAsync(long taskId, IServiceProvider sp, CancellationToken ct)
    {
        // ✅ 注入新服务
        var taskManager = sp.GetRequiredService<IWorkshopTelemetryTaskManager>();      // 新增
        var taskRepo = sp.GetRequiredService<IWorkshopTelemetryTaskRepository>();
        var db = sp.GetRequiredService<TestWorkshopDbContext>();

        // ✅ 获取任务（含 FileObject 关联）
        var (telemetryTask, fileObject) = await taskManager.GetTaskWithFileAsync(taskId);
        if (telemetryTask == null || telemetryTask.Status != 1)  // Status 1 = Processing（已被 Claim 方法标记）
        {
            _logger.LogWarning("任务 {TaskId} 状态非 Processing，跳过", taskId);
            return;
        }

        await db.Database.OpenConnectionAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var npgsqlTransaction = transaction.GetDbTransaction() as NpgsqlTransaction
            ?? throw new InvalidOperationException("无法获取 NpgsqlTransaction");

        try
        {
            // ✅ 通过 FileObjectManager 获取文件流（替代原来的 blobContainer.GetAsync）
            var fileManager = sp.GetRequiredService<IFileObjectManager>();  // 新增
            var (stream, _, _) = await fileManager.GetFileAsync(fileObject.Id);

            var deviceMap = await GetDeviceMapAsync(db, ct);
            var recordCount = await BulkInsertFromStreamAsync(
                db,
                stream,
                deviceMap,
                npgsqlTransaction,
                ct);

            // ✅ 使用任务实体的行为方法更新状态（而不是直接赋值）
            telemetryTask.MarkAsSuccess(recordCount);
            await taskRepo.UpdateAsync(telemetryTask);
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            // ✅ 处理成功后，通过 FileObjectManager 删除物理文件
            try
            {
                await fileManager.DeleteFileAsync(fileObject.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除物理文件失败: {FileObjectId}", fileObject.Id);
            }

            _logger.LogInformation("✅ 任务 {TaskId} 处理完成，记录数 {Count}", taskId, recordCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "❌ 任务 {TaskId} 处理失败", taskId);

            // ✅ 使用任务实体的行为方法标记失败（包含指数退避）
            telemetryTask.MarkAsFailed(ex.Message);
            await taskRepo.UpdateAsync(telemetryTask);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private async Task<int> BulkInsertFromStreamAsync(
        TestWorkshopDbContext db,
        Stream csvStream,
        Dictionary<string, Guid> deviceMap,
        NpgsqlTransaction transaction,
        CancellationToken ct)
    {
        var entityType = db.Model.FindEntityType(typeof(WorkshopDeviceTelemetry));
        var rawTableName = entityType?.GetTableName() ?? "AppWorkshopDeviceTelemetries";
        var tableName = $"\"{rawTableName}\"";

        using var writer = transaction.Connection.BeginBinaryImport(
            $"COPY {tableName} (\"DeviceId\",\"MetricType\",\"Value\",\"Timestamp\",\"TestedDeviceCode\",\"TestedDeviceName\") FROM STDIN (FORMAT BINARY)");

        int count = 0, skipped = 0;
        using var reader = new StreamReader(csvStream);

        var firstLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(firstLine)) return 0;

        bool isHeader = firstLine.Contains("DeviceCode", StringComparison.OrdinalIgnoreCase) ||
                        firstLine.Contains("MetricType", StringComparison.OrdinalIgnoreCase);

        if (!isHeader)
        {
            ParseAndWriteLine(firstLine, writer, deviceMap, ref count, ref skipped);
        }

        while (!reader.EndOfStream)
        {
            if (count >= _maxRowsPerFile)
            {
                _logger.LogWarning("达到行数上限 {MaxRows}，中断读取", _maxRowsPerFile);
                break;
            }

            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;
            ParseAndWriteLine(line, writer, deviceMap, ref count, ref skipped);
        }

        writer.Complete();

        if (skipped > 0)
            _logger.LogWarning("跳过了 {Skipped} 行无效数据", skipped);

        if (count == 0 && skipped > 0)
            throw new InvalidDataException($"CSV 解析失败：{skipped} 行全部无效，请检查文件格式或分隔符");

        return count;
    }

    private void ParseAndWriteLine(string line, NpgsqlBinaryImporter writer,
        Dictionary<string, Guid> deviceMap, ref int count, ref int skipped)
    {
        var parts = line.Split(',');
        if (parts.Length < 6)
        {
            skipped++;
            return;
        }

        try
        {
            var deviceCode = parts[0].Trim();
            var metricType = parts[1].Trim();

            if (!double.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                skipped++;
                return;
            }
            if (!DateTime.TryParse(parts[3].Trim(), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timestamp))
            {
                skipped++;
                return;
            }

            var testedDeviceCode = parts[4].Trim();
            var testedDeviceName = parts[5].Trim();

            if (!deviceMap.TryGetValue(deviceCode, out var deviceId))
            {
                _logger.LogWarning("未知设备码 {Code}", deviceCode);
                skipped++;
                return;
            }

            writer.StartRow();
            writer.Write(deviceId, NpgsqlDbType.Uuid);
            writer.Write(metricType, NpgsqlDbType.Text);
            writer.Write(value, NpgsqlDbType.Double);
            writer.Write(timestamp, NpgsqlDbType.TimestampTz);
            writer.Write(testedDeviceCode, NpgsqlDbType.Text);
            writer.Write(testedDeviceName, NpgsqlDbType.Text);
            count++;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CSV 行解析失败: {Line}", line);
            skipped++;
        }
    }

    private async Task<Dictionary<string, Guid>> GetDeviceMapAsync(TestWorkshopDbContext db, CancellationToken ct)
    {
        const string cacheKey = "DeviceMap_TelemetryWorker";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await db.Devices.ToDictionaryAsync(d => d.Code, d => d.Id, ct);
        }) ?? new Dictionary<string, Guid>();
    }

    // ✅ ResetStuckTasksAsync 需要微调：用新的任务状态字段名
    private async Task ResetStuckTasksAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();

            var stuckTime = DateTime.UtcNow.AddMinutes(-10);
            var retryTime = DateTime.UtcNow.AddSeconds(30);

            // ⚠️ 注意：新表有 FileObjectId 字段，但 Status 字段名保持不变
            var sql = @"
                UPDATE ""AppWorkshopTelemetryTasks""
                SET ""Status"" = 0,
                    ""RetryCount"" = ""RetryCount"" + 1,
                    ""NextRetryTime"" = @RetryTime
                WHERE ""Status"" = 1
                  AND ""ProcessingStartedAt"" < @StuckTime";

            var rowsAffected = await db.Database.ExecuteSqlRawAsync(
                sql,
                new object[]
                {
                    new NpgsqlParameter("@RetryTime", retryTime),
                    new NpgsqlParameter("@StuckTime", stuckTime)
                },
                ct);

            if (rowsAffected > 0)
                _logger.LogInformation("🔄 原子性恢复了 {Count} 个卡死的任务", rowsAffected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复卡死任务时发生异常");
        }
    }

}