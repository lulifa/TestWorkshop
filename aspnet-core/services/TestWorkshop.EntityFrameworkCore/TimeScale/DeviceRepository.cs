namespace TestWorkshop.EntityFrameworkCore;

public class DeviceRepository : EfCoreRepository<TestWorkshopDbContext, Device, Guid>, IDeviceRepository
{
    public DeviceRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Device> FindByCodeAsync(string code)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task<List<Device>> GetByOrganizationUnitIdAsync(Guid orgId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(d => d.OrganizationUnitId == orgId).ToListAsync();
    }
}
