using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;

namespace TestWorkshop.EntityFrameworkCore;

public class TelemetryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TelemetryWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory scopeFactory)
        : base(timer, scopeFactory)
    {
        _scopeFactory = scopeFactory;

        Timer.Period = 5000; // 每5秒执行一次
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();

        // 👉 1. 抓取任务（只拿5个）
        var tasks = await db.TelemetryTasks
            .Where(t => t.Status == 0 && (t.NextRetryTime == null || t.NextRetryTime <= DateTime.UtcNow))
            .OrderBy(t => t.CreatedAt)
            .Take(5)
            .ToListAsync();

        if (!tasks.Any()) return;

        // 👉 2. 标记 Processing（避免重复处理）
        foreach (var task in tasks)
        {
            task.Status = 1;
        }
        await db.SaveChangesAsync();

        // 👉 3. 逐个处理
        foreach (var task in tasks)
        {
            await ProcessTask(task);
        }
    }

    private async Task ProcessTask(TelemetryTask task)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();
        var blob = scope.ServiceProvider.GetRequiredService<IBlobContainer>();

        try
        {
            // 👉 1. 读取文件
            var stream = await blob.GetAsync(task.BlobName);

            var records = await ParseCsv(stream);

            // 👉 2. 写入数据库（COPY）
            await BulkInsert(db, records);

            // 👉 3. 成功
            task.Status = 2;
            task.ProcessedAt = DateTime.UtcNow;

            // 👉 删除文件（可选）
            await blob.DeleteAsync(task.BlobName);
        }
        catch (Exception ex)
        {
            task.RetryCount++;
            task.Error = ex.Message;

            if (task.RetryCount >= 3)
            {
                task.Status = 3; // Failed
            }
            else
            {
                task.Status = 0;
                task.NextRetryTime = DateTime.UtcNow.AddMinutes(Math.Pow(2, task.RetryCount));
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task<List<DeviceTelemetryDto>> ParseCsv(Stream stream)
    {
        var result = new List<DeviceTelemetryDto>();

        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var parts = line.Split(',');

            result.Add(new DeviceTelemetryDto
            {
                DeviceCode = parts[0],
                Metric = parts[1],
                Value = double.Parse(parts[2]),
                Timestamp = DateTime.Parse(parts[3])
            });
        }

        return result;
    }

    private async Task BulkInsert(TestWorkshopDbContext db, List<DeviceTelemetryDto> dtos)
    {
        var deviceMap = await db.Devices
            .ToDictionaryAsync(d => d.Code, d => d.Id);

        using var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        await conn.OpenAsync();

        using var writer = conn.BeginBinaryImport(
            "COPY \"DeviceTelemetries\" (\"DeviceId\",\"Metric\",\"Value\",\"Timestamp\") FROM STDIN (FORMAT BINARY)"
        );

        foreach (var dto in dtos)
        {
            if (!deviceMap.TryGetValue(dto.DeviceCode, out var deviceId))
                continue;

            writer.StartRow();
            writer.Write(deviceId);
            writer.Write(dto.Metric);
            writer.Write(dto.Value);
            writer.Write(dto.Timestamp, NpgsqlTypes.NpgsqlDbType.TimestampTz);
        }

        writer.Complete();
    }

}
