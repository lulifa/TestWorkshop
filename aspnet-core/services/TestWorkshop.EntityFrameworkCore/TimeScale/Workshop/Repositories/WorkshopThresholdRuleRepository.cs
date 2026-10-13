namespace TestWorkshop.EntityFrameworkCore;

public class WorkshopThresholdRuleRepository
    : EfCoreRepository<TestWorkshopDbContext, WorkshopThresholdRule, Guid>,
      IWorkshopThresholdRuleRepository
{
    public WorkshopThresholdRuleRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<WorkshopThresholdRule>> GetListByDeviceTypeAsync(DeviceTypeEnum deviceType)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.DeviceType == deviceType)
            .ToListAsync();
    }

    public async Task<WorkshopThresholdRule> FindAsync(DeviceTypeEnum deviceType, string dataItemType)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .FirstOrDefaultAsync(x => x.DeviceType == deviceType && x.DataItemType == dataItemType);
    }
}
