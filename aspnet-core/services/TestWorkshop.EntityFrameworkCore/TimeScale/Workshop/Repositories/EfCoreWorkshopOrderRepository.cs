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
                            || x.Status == WorkshopOrderStatus.PendingReview
                            || x.Status == WorkshopOrderStatus.ReviewApproved
                            || x.Status == WorkshopOrderStatus.ReviewRejected)
                        && x.TestStartedAt.HasValue
                        && x.TestStartedAt.Value <= latestStartTime
                        && (!x.TestEndedAt.HasValue || x.TestEndedAt.Value >= earliestEndTime))
            .OrderBy(x => x.TestStartedAt)
            .ToListAsync();
    }

    public async Task<List<WorkshopOrder>> GetTestReportFetchCandidatesAsync(
        DateTime earliestTestEndedAt,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var dbContext = await GetDbContextAsync();
        var fetchRecords = dbContext
            .Set<WorkshopOrderReportFetchRecord>()
            .IgnoreQueryFilters();
        return await dbSet
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted
                        && x.Status == WorkshopOrderStatus.PendingReview
                        && (x.TestReportStatus == WorkshopTestReportStatus.NotReady
                            || x.TestReportStatus == WorkshopTestReportStatus.Failed)
                        && x.TestEndedAt.HasValue
                        && x.TestEndedAt.Value >= earliestTestEndedAt
                        && !fetchRecords.Any(record =>
                            !record.IsDeleted
                            && record.OrderId == x.Id
                            && (record.Status == WorkshopTestReportFetchRecordStatus.Pending
                                || record.Status == WorkshopTestReportFetchRecordStatus.Processing)))
            .OrderBy(x => x.TestEndedAt)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResultDto<WorkshopOrder>> GetPagedListAsync(
        string orderNo = null,
        Guid? deviceId = null,
        WorkshopOrderStatus? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        ICollection<Guid> organizationUnitIds = null,
        Guid? projectSupervisorUserId = null,
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

        // 可见性：订单没存车间，通过设备归属过滤；另外单船项目主管能看到"自己负责"的订单。
        // 两个条件之间是"或"的关系，所以分三种情况拼，避免传 null 进表达式。
        if (organizationUnitIds != null && projectSupervisorUserId.HasValue)
        {
            var deviceSet = (await GetDbContextAsync()).Set<WorkshopDevice>();
            var organizationUnitIdList = organizationUnitIds;
            var supervisorUserId = projectSupervisorUserId.Value;
            query = query.Where(order =>
                deviceSet.Any(device =>
                    device.Id == order.DeviceId
                    && organizationUnitIdList.Contains(device.OrganizationUnitId))
                || order.ProjectSupervisorUserId == supervisorUserId);
        }
        else if (organizationUnitIds != null)
        {
            var deviceSet = (await GetDbContextAsync()).Set<WorkshopDevice>();
            var organizationUnitIdList = organizationUnitIds;
            query = query.Where(order =>
                deviceSet.Any(device =>
                    device.Id == order.DeviceId
                    && organizationUnitIdList.Contains(device.OrganizationUnitId)));
        }
        else if (projectSupervisorUserId.HasValue)
        {
            var supervisorUserId = projectSupervisorUserId.Value;
            query = query.Where(order => order.ProjectSupervisorUserId == supervisorUserId);
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
