namespace TestWorkshop.EntityFrameworkCore;

public class WorkshopDeviceHeartbeatRepository
    : EfCoreRepository<TestWorkshopDbContext, WorkshopDeviceHeartbeat, Guid>,
      IWorkshopDeviceHeartbeatRepository
{
    public WorkshopDeviceHeartbeatRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<WorkshopDeviceHeartbeat> FindByDeviceCodeAsync(string deviceCode)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);
    }
}
