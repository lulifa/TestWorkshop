namespace TestWorkshop.EntityFrameworkCore;

public class EfCoreNotificationSubscriptionRepository : EfCoreRepository<TestWorkshopDbContext, NotificationSubscription, Guid>, INotificationSubscriptionRepository
{
    public EfCoreNotificationSubscriptionRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<List<NotificationSubscription>> GetListAsync(Guid notificationId, Guid? receiverUserId, string receiverUserName, DateTime? startReadTime, DateTime? endReadTime, int maxResultCount = 10, int skipCount = 0, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(e => e.NotificationId == notificationId)
            .WhereIf(receiverUserId.HasValue, e => e.ReceiveUserId == receiverUserId.Value)
            .WhereIf(!receiverUserName.IsNullOrWhiteSpace(), e => e.ReceiveUserName == receiverUserName)
            .WhereIf(startReadTime.HasValue, e => e.ReadTime >= startReadTime.Value)
            .WhereIf(endReadTime.HasValue, e => e.ReadTime <= endReadTime.Value)
            .OrderByDescending(e => e.CreationTime)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<long> GetCountAsync(Guid notificationId, Guid? receiverUserId, string receiverUserName, DateTime? startReadTime, DateTime? endReadTime, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(e => e.NotificationId == notificationId)
            .WhereIf(receiverUserId.HasValue, e => e.ReceiveUserId == receiverUserId.Value)
            .WhereIf(!receiverUserName.IsNullOrWhiteSpace(), e => e.ReceiveUserName == receiverUserName)
            .WhereIf(startReadTime.HasValue, e => e.ReadTime >= startReadTime.Value)
            .WhereIf(endReadTime.HasValue, e => e.ReadTime <= endReadTime.Value)
            .OrderByDescending(e => e.CreationTime)
            .CountAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<NotificationSubscription> FindAsync(Guid receiverUserId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync()).FirstOrDefaultAsync(e => e.ReceiveUserId == receiverUserId && e.NotificationId == notificationId, GetCancellationToken(cancellationToken));
    }

    public async Task<List<NotificationSubscription>> GetListAsync(List<Guid> notificationId, Guid receiverUserId, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(e => e.ReceiveUserId == receiverUserId)
            .Where(e => notificationId.Contains(e.NotificationId))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

}
