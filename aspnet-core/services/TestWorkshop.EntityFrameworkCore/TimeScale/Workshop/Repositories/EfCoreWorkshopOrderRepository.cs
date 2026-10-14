namespace TestWorkshop.EntityFrameworkCore;

public class EfCoreWorkshopOrderRepository :
    EfCoreRepository<TestWorkshopDbContext, WorkshopOrder, Guid>,
    IWorkshopOrderRepository
{
    public EfCoreWorkshopOrderRepository(
        IDbContextProvider<TestWorkshopDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<WorkshopOrder> FindByOrderNoAsync(string orderNo)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.OrderNo == orderNo);
    }

    public async Task<WorkshopOrder> FindActiveByDeviceAsync(Guid deviceId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            x => x.DeviceId == deviceId && x.Status == WorkshopOrderStatus.Testing);
    }

    public async Task AcquireDeviceLockAsync(Guid deviceId)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtextextended({0}, 0))",
            deviceId.ToString());
    }

    public async Task<List<WorkshopOrder>> GetTelemetryCandidatesAsync(
        Guid deviceId,
        DateTime earliestEndTime,
        DateTime latestStartTime)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId
                        && (x.Status == WorkshopOrderStatus.Testing
                            || x.Status == WorkshopOrderStatus.Completed)
                        && x.TestStartedAt.HasValue
                        && x.TestStartedAt.Value <= latestStartTime
                        && (!x.TestEndedAt.HasValue || x.TestEndedAt.Value >= earliestEndTime))
            .OrderBy(x => x.TestStartedAt)
            .ToListAsync();
    }

    public async Task<PagedResultDto<WorkshopOrder>> GetPagedListAsync(
        string orderNo = null,
        Guid? deviceId = null,
        WorkshopOrderStatus? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        ICollection<Guid> organizationUnitIds = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (!orderNo.IsNullOrWhiteSpace())
        {
            var filter = orderNo.Trim();
            query = query.Where(x => x.OrderNo.Contains(filter));
        }

        if (deviceId.HasValue)
        {
            query = query.Where(x => x.DeviceId == deviceId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (startTime.HasValue)
        {
            query = query.Where(
                x => x.TestStartedAt.HasValue && x.TestStartedAt.Value >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(
                x => x.TestStartedAt.HasValue && x.TestStartedAt.Value <= endTime.Value);
        }

        if (organizationUnitIds != null)
        {
            // 订单本身没存车间，通过设备归属过滤；传空集合表示无权查看任何订单
            var deviceSet = (await GetDbContextAsync()).Set<WorkshopDevice>();
            query = query.Where(order =>
                deviceSet.Any(device =>
                    device.Id == order.DeviceId
                    && organizationUnitIds.Contains(device.OrganizationUnitId)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<WorkshopOrder>(totalCount, items);
    }
}
