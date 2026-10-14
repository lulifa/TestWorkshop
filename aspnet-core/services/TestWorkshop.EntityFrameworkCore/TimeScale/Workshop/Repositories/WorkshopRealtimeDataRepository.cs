using NpgsqlTypes;

namespace TestWorkshop.EntityFrameworkCore;

public class WorkshopRealtimeDataRepository : IWorkshopRealtimeDataRepository, ITransientDependency
{
    private const string TableName = "AppWorkshopRealtimeData";

    private readonly IDbContextProvider<TestWorkshopDbContext> _dbContextProvider;

    public WorkshopRealtimeDataRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task UpsertAsync(WorkshopRealtimeData data, CancellationToken cancellationToken = default)
    {
        var db = await _dbContextProvider.GetDbContextAsync();

        const string sql =
            $"""
             INSERT INTO "{TableName}"
                 ("DeviceId", "Timestamp", "DeviceCode", "Pressure", "Flow", "Temperature", "Torque", "Vibration",
                  "EmergencyStop", "ReceivedAt")
             VALUES
                 (@DeviceId, @Timestamp, @DeviceCode, @Pressure, @Flow, @Temperature, @Torque, @Vibration,
                  @EmergencyStop, @ReceivedAt)
             ON CONFLICT ("DeviceId", "Timestamp") DO UPDATE SET
                 "DeviceCode" = EXCLUDED."DeviceCode",
                 "Pressure" = EXCLUDED."Pressure",
                 "Flow" = EXCLUDED."Flow",
                 "Temperature" = EXCLUDED."Temperature",
                 "Torque" = EXCLUDED."Torque",
                 "Vibration" = EXCLUDED."Vibration",
                 "EmergencyStop" = EXCLUDED."EmergencyStop",
                 "ReceivedAt" = EXCLUDED."ReceivedAt"
             """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            new object[]
            {
                new NpgsqlParameter("DeviceId", NpgsqlDbType.Uuid) { Value = data.DeviceId },
                new NpgsqlParameter("Timestamp", NpgsqlDbType.TimestampTz) { Value = data.Timestamp },
                new NpgsqlParameter("DeviceCode", NpgsqlDbType.Text) { Value = data.DeviceCode },
                new NpgsqlParameter("Pressure", NpgsqlDbType.Numeric) { Value = data.Pressure ?? (object)DBNull.Value },
                new NpgsqlParameter("Flow", NpgsqlDbType.Numeric) { Value = data.Flow ?? (object)DBNull.Value },
                new NpgsqlParameter("Temperature", NpgsqlDbType.Numeric) { Value = data.Temperature ?? (object)DBNull.Value },
                new NpgsqlParameter("Torque", NpgsqlDbType.Numeric) { Value = data.Torque ?? (object)DBNull.Value },
                new NpgsqlParameter("Vibration", NpgsqlDbType.Numeric) { Value = data.Vibration ?? (object)DBNull.Value },
                new NpgsqlParameter("EmergencyStop", NpgsqlDbType.Boolean) { Value = data.EmergencyStop ?? (object)DBNull.Value },
                new NpgsqlParameter("ReceivedAt", NpgsqlDbType.TimestampTz) { Value = data.ReceivedAt }
            },
            cancellationToken);
    }

    public async Task<List<WorkshopRealtimeData>> GetLatestPerDeviceAsync(CancellationToken cancellationToken = default)
    {
        var db = await _dbContextProvider.GetDbContextAsync();

        return await db.Set<WorkshopRealtimeData>()
            .FromSqlRaw(
                $"""
                 SELECT DISTINCT ON ("DeviceId") *
                 FROM "{TableName}"
                 ORDER BY "DeviceId", "Timestamp" DESC
                 """)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
