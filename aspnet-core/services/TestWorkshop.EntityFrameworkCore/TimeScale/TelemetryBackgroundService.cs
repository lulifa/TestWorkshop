namespace TestWorkshop.EntityFrameworkCore;

public class TelemetryBackgroundService : BackgroundService
{
    private readonly TelemetryChannel _channel;
    private readonly IServiceProvider _serviceProvider;

    public TelemetryBackgroundService(TelemetryChannel channel, IServiceProvider serviceProvider)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var batch in _channel.Queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TestWorkshopDbContext>();

            await BulkInsert(dbContext, batch);
        }
    }


    private async Task BulkInsert(TestWorkshopDbContext dbContext, List<DeviceTelemetryDto> batch)
    {
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await conn.OpenAsync();

        using var writer = conn.BeginBinaryImport(
            "COPY \"DeviceTelemetries\" (\"DeviceId\",\"Metric\",\"Value\",\"Timestamp\") FROM STDIN (FORMAT BINARY)"
        );

        foreach (var dto in batch)
        {
            var device = await dbContext.Devices.FirstOrDefaultAsync(d => d.Code == dto.DeviceCode);
            if (device == null) continue;

            writer.StartRow();
            writer.Write(device.Id);
            writer.Write(dto.Metric);
            writer.Write(dto.Value);
            writer.Write(dto.Timestamp, NpgsqlTypes.NpgsqlDbType.TimestampTz);
        }

        writer.Complete();
    }

}
