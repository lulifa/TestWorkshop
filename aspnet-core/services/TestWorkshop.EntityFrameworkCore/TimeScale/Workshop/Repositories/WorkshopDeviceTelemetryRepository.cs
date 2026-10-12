namespace TestWorkshop.EntityFrameworkCore;

/// <summary>
/// 设备遥测数据仓储实现（TimescaleDB 超级表）
/// </summary>
public class WorkshopDeviceTelemetryRepository :
    IWorkshopDeviceTelemetryRepository,
    ITransientDependency
{
    private readonly IDbContextProvider<TestWorkshopDbContext> _dbContextProvider;

    public WorkshopDeviceTelemetryRepository(
        IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<WorkshopDeviceTelemetry>> GetListByTaskIdsAsync(List<long> taskIds)
    {
        if (taskIds == null || taskIds.Count == 0)
        {
            return new List<WorkshopDeviceTelemetry>();
        }

        var dbContext = await _dbContextProvider.GetDbContextAsync();
        return await dbContext.Set<WorkshopDeviceTelemetry>()
            .AsNoTracking()
            .Where(x => taskIds.Contains(x.TaskId))
            .ToListAsync();
    }
}
