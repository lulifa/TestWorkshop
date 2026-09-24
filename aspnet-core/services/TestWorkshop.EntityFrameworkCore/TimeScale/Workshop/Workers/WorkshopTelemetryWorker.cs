using Volo.Abp.MultiTenancy;

namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 后台任务调度器：定期扫描待处理文件，解析 CSV 并批量写入遥测数据
/// </summary>
public class WorkshopTelemetryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private const int MaxRowsPerFile = 1_000_000;
    private const string DeviceMapCacheKeyPrefix = "DeviceMap_TelemetryWorker_";
    private const string TelemetryInputPrefix = TestWorkshopConsts.TelemetryInputExtraPropertiesPrefix;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkshopTelemetryWorker> _logger;

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

            var tasks = await taskRepo.ClaimPendingTasksAsync(take: 5);
            if (tasks.Count == 0)
            {
                return;
            }

            foreach (var telemetryTask in tasks)
            {
                using var processScope = _scopeFactory.CreateScope();
                await ProcessTaskAsync(telemetryTask.Id, processScope.ServiceProvider, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WorkshopTelemetryWorker 执行异常，Worker 将继续运行");
        }
    }

    private async Task ProcessTaskAsync(long taskId, IServiceProvider sp, CancellationToken ct)
    {
        var taskManager = sp.GetRequiredService<IWorkshopTelemetryTaskManager>();
        var taskRepo = sp.GetRequiredService<IWorkshopTelemetryTaskRepository>();
        var db = sp.GetRequiredService<TestWorkshopDbContext>();
        var currentTenant = sp.GetRequiredService<ICurrentTenant>();

        var (telemetryTask, fileObject) = await taskManager.GetTaskWithFileAsync(taskId);
        if (telemetryTask == null || fileObject == null || telemetryTask.Status != 1)
        {
            _logger.LogWarning("任务 {TaskId} 或关联文件不存在，或状态不是 Processing，已跳过", taskId);
            return;
        }

        using (currentTenant.Change(telemetryTask.TenantId))
        {
            await db.Database.OpenConnectionAsync(ct);
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var metadata = ReadMetadata(fileObject);
                var deviceMap = await GetDeviceMapAsync(db, telemetryTask.TenantId, ct);
                if (!deviceMap.TryGetValue(metadata.DeviceCode, out var deviceId))
                {
                    throw new InvalidDataException($"未知设备编码: {metadata.DeviceCode}");
                }

                var fileManager = sp.GetRequiredService<IFileObjectManager>();
                var (stream, _, _) = await fileManager.GetFileAsync(fileObject.Id);
                await using (stream)
                {
                    var values = await ReadValuesAsync(stream, ct);
                    await UpsertTelemetryAsync(
                        db,
                        transaction.GetDbTransaction() as NpgsqlTransaction
                        ?? throw new InvalidOperationException("无法获取 NpgsqlTransaction"),
                        deviceId,
                        telemetryTask.Id,
                        metadata,
                        values,
                        ct);

                    telemetryTask.MarkAsSuccess(values.Length);
                }

                await taskRepo.UpdateAsync(telemetryTask);
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "任务 {TaskId} 处理完成，通道 {ChannelType}，采样点 {Count}",
                    taskId,
                    metadata.ChannelType,
                    telemetryTask.RecordCount);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "任务 {TaskId} 处理失败", taskId);

                telemetryTask.MarkAsFailed(ex.Message);
                await taskRepo.UpdateAsync(telemetryTask);
                await db.SaveChangesAsync(ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }

    private async Task<double[]> ReadValuesAsync(Stream csvStream, CancellationToken ct)
    {
        using var reader = new StreamReader(csvStream);
        var values = new List<double>();
        var lineNumber = 0;

        string line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (values.Count >= MaxRowsPerFile)
            {
                throw new InvalidDataException($"CSV 数据超过单文件上限 {MaxRowsPerFile} 行");
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                throw new InvalidDataException($"CSV 第 {lineNumber} 行格式错误，应为 index,value");
            }

            if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            {
                throw new InvalidDataException($"CSV 第 {lineNumber} 行 index 不是有效整数: {parts[0]}");
            }

            if (index != values.Count)
            {
                throw new InvalidDataException(
                    $"CSV 第 {lineNumber} 行 index 应为 {values.Count}，实际为 {index}");
            }

            if (!double.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidDataException($"CSV 第 {lineNumber} 行 value 不是有效数字: {parts[1]}");
            }

            values.Add(value);
        }

        if (values.Count == 0)
        {
            throw new InvalidDataException("CSV 文件不包含有效数据");
        }

        return values.ToArray();
    }

    private async Task UpsertTelemetryAsync(
        TestWorkshopDbContext db,
        NpgsqlTransaction transaction,
        Guid deviceId,
        long taskId,
        TelemetryFileMetadata metadata,
        double[] values,
        CancellationToken ct)
    {
        var entityType = db.Model.FindEntityType(typeof(WorkshopDeviceTelemetry));
        var rawTableName = entityType?.GetTableName() ?? "AppWorkshopDeviceTelemetries";
        var schema = entityType?.GetSchema();
        var fullTableName = string.IsNullOrWhiteSpace(schema)
            ? $"\"{rawTableName}\""
            : $"\"{schema}\".\"{rawTableName}\"";

        await using var command = transaction.Connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            $"""
             INSERT INTO {fullTableName}
                 ("DeviceId", "TaskId", "Timestamp", "ChannelType", "Value",
                  "TestedDeviceCode", "TestedDeviceName", "PilotSN", "ShipName", "TestBy")
             VALUES
                 (@DeviceId, @TaskId, @Timestamp, @ChannelType, @Value,
                  @TestedDeviceCode, @TestedDeviceName, @PilotSN, @ShipName, @TestBy)
             ON CONFLICT ("DeviceId", "Timestamp", "ChannelType") DO UPDATE SET
                 "TaskId" = EXCLUDED."TaskId",
                 "Value" = EXCLUDED."Value",
                 "TestedDeviceCode" = EXCLUDED."TestedDeviceCode",
                 "TestedDeviceName" = EXCLUDED."TestedDeviceName",
                 "PilotSN" = EXCLUDED."PilotSN",
                 "ShipName" = EXCLUDED."ShipName",
                 "TestBy" = EXCLUDED."TestBy";
             """;

        command.Parameters.Add(new NpgsqlParameter("DeviceId", NpgsqlDbType.Uuid) { Value = deviceId });
        command.Parameters.Add(new NpgsqlParameter("TaskId", NpgsqlDbType.Bigint) { Value = taskId });
        command.Parameters.Add(new NpgsqlParameter("Timestamp", NpgsqlDbType.TimestampTz) { Value = metadata.Timestamp });
        command.Parameters.Add(new NpgsqlParameter("ChannelType", NpgsqlDbType.Text) { Value = metadata.ChannelType });
        command.Parameters.Add(new NpgsqlParameter("Value", NpgsqlDbType.Array | NpgsqlDbType.Double) { Value = values });
        command.Parameters.Add(new NpgsqlParameter("TestedDeviceCode", NpgsqlDbType.Text) { Value = metadata.TestedDeviceCode ?? (object)DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("TestedDeviceName", NpgsqlDbType.Text) { Value = metadata.TestedDeviceName ?? (object)DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("PilotSN", NpgsqlDbType.Text) { Value = metadata.PilotSN ?? (object)DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("ShipName", NpgsqlDbType.Text) { Value = metadata.ShipName ?? (object)DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("TestBy", NpgsqlDbType.Text) { Value = metadata.TestBy ?? (object)DBNull.Value });

        await command.ExecuteNonQueryAsync(ct);
    }

    private static TelemetryFileMetadata ReadMetadata(FileObject fileObject)
    {
        var deviceCode = GetRequiredString(fileObject, "DeviceCode").Trim();
        var channelType = GetRequiredString(fileObject, "ChannelType").Trim();
        var startTimeText = GetRequiredString(fileObject, "TestInfo.StartTime").Trim();

        if (!DateTime.TryParse(
                startTimeText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var timestamp))
        {
            throw new InvalidDataException($"测试开始时间格式错误: {startTimeText}");
        }

        return new TelemetryFileMetadata
        {
            DeviceCode = deviceCode,
            ChannelType = channelType,
            Timestamp = timestamp,
            TestedDeviceCode = GetOptionalString(fileObject, "TestInfo.TestedDeviceCode"),
            TestedDeviceName = GetOptionalString(fileObject, "TestInfo.TestedDeviceName"),
            PilotSN = GetOptionalString(fileObject, "TestInfo.PilotSN"),
            ShipName = GetOptionalString(fileObject, "TestInfo.ShipName"),
            TestBy = GetOptionalString(fileObject, "TestInfo.TestBy")
        };
    }

    private static string GetRequiredString(FileObject fileObject, string propertyName)
    {
        var value = GetOptionalString(fileObject, propertyName);
        if (value.IsNullOrWhiteSpace())
        {
            throw new InvalidDataException($"文件扩展参数缺少 {TelemetryInputPrefix}{propertyName}");
        }

        return value;
    }

    private static string GetOptionalString(FileObject fileObject, string propertyName)
    {
        var value = fileObject.GetProperty(TelemetryInputPrefix + propertyName);
        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    private async Task<Dictionary<string, Guid>> GetDeviceMapAsync(
        TestWorkshopDbContext db,
        Guid? tenantId,
        CancellationToken ct)
    {
        var cacheKey = DeviceMapCacheKeyPrefix + (tenantId?.ToString() ?? "Host");
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var devices = await db.Devices.AsNoTracking().ToListAsync(ct);
            return devices.ToDictionary(d => d.Code, d => d.Id, StringComparer.OrdinalIgnoreCase);
        }) ?? new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task ResetStuckTasksAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();

            var stuckTime = DateTime.UtcNow.AddMinutes(-10);
            var retryTime = DateTime.UtcNow.AddSeconds(30);

            const string sql = """
                               UPDATE "AppWorkshopTelemetryTasks"
                               SET "Status" = 0,
                                   "RetryCount" = "RetryCount" + 1,
                                   "NextRetryTime" = @RetryTime
                               WHERE "Status" = 1
                                 AND "ProcessingStartedAt" < @StuckTime
                               """;

            var rowsAffected = await db.Database.ExecuteSqlRawAsync(
                sql,
                new object[]
                {
                    new NpgsqlParameter("@RetryTime", retryTime),
                    new NpgsqlParameter("@StuckTime", stuckTime)
                },
                ct);

            if (rowsAffected > 0)
            {
                _logger.LogInformation("恢复了 {Count} 个卡死的遥测任务", rowsAffected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复卡死遥测任务时发生异常");
        }
    }

    private sealed class TelemetryFileMetadata
    {
        public string DeviceCode { get; init; }
        public string ChannelType { get; init; }
        public DateTime Timestamp { get; init; }
        public string TestedDeviceCode { get; init; }
        public string TestedDeviceName { get; init; }
        public string PilotSN { get; init; }
        public string ShipName { get; init; }
        public string TestBy { get; init; }
    }
}
