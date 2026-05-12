namespace TestWorkshop.EntityFrameworkCore;

public class WorkshopDeviceRepository : EfCoreRepository<TestWorkshopDbContext, WorkshopDevice, Guid>, IWorkshopDeviceRepository
{
    public WorkshopDeviceRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<WorkshopDevice> FindByCodeAsync(string code)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Code == code);
    }
}
