using TestWorkshop.SignalR;

namespace TestWorkshop.EntityFrameworkCore;

public class EfCoreNotificationRepository : EfCoreRepository<TestWorkshopDbContext, Notification, Guid>, INotificationRepository
{
    public EfCoreNotificationRepository(IDbContextProvider<TestWorkshopDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<List<Notification>> GetListAsync(
            string title,
            string content,
            Guid? senderUserId,
            string senderUserName,
            Guid? receiverUserId,
            string receiverUserName,
            bool? read,
            DateTime? startReadTime,
            DateTime? endReadTime,
            MessageType? messageType,
            MessageLevel? messageLevel,
            int maxResultCount = 10,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(!title.IsNullOrWhiteSpace(), e => e.Title.Contains(title))
            .WhereIf(!content.IsNullOrWhiteSpace(), e => e.Content.Contains(content))
            .WhereIf(senderUserId.HasValue, e => e.SenderUserId == senderUserId.Value)
            .WhereIf(!senderUserName.IsNullOrWhiteSpace(), e => e.SenderUserName == senderUserName)
            .WhereIf(receiverUserId.HasValue, e => e.ReceiveUserId == receiverUserId.Value)
            .WhereIf(!receiverUserName.IsNullOrWhiteSpace(), e => e.ReceiveUserName == receiverUserName)
            .WhereIf(read.HasValue, e => e.Read == read.Value)
            .WhereIf(startReadTime.HasValue, e => e.ReadTime >= startReadTime.Value)
            .WhereIf(endReadTime.HasValue, e => e.ReadTime <= endReadTime.Value)
            .WhereIf(messageType.HasValue, e => e.MessageType == messageType.Value)
            .WhereIf(messageLevel.HasValue, e => e.MessageLevel == messageLevel.Value)
            .OrderByDescending(e => e.CreationTime)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<long> GetCountAsync(
        string title,
        string content,
        Guid? senderUserId,
        string senderUserName,
        Guid? receiverUserId,
        string receiverUserName,
        bool? read,
        DateTime? startReadTime,
        DateTime? endReadTime,
        MessageType? messageType,
        MessageLevel? messageLevel,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(!title.IsNullOrWhiteSpace(), e => e.Title.Contains(title))
            .WhereIf(!content.IsNullOrWhiteSpace(), e => e.Content.Contains(content))
            .WhereIf(senderUserId.HasValue, e => e.SenderUserId == senderUserId.Value)
            .WhereIf(!senderUserName.IsNullOrWhiteSpace(), e => e.SenderUserName == senderUserName)
            .WhereIf(receiverUserId.HasValue, e => e.ReceiveUserId == receiverUserId.Value)
            .WhereIf(!receiverUserName.IsNullOrWhiteSpace(), e => e.ReceiveUserName == receiverUserName)
            .WhereIf(read.HasValue, e => e.Read == read.Value)
            .WhereIf(startReadTime.HasValue, e => e.ReadTime >= startReadTime.Value)
            .WhereIf(endReadTime.HasValue, e => e.ReadTime <= endReadTime.Value)
            .WhereIf(messageType.HasValue, e => e.MessageType == messageType.Value)
            .WhereIf(messageLevel.HasValue, e => e.MessageLevel == messageLevel.Value)
            .CountAsync(cancellationToken);
    }

    public async Task<List<Notification>> GetListAsync(List<Guid> ids)
    {
        return await (await GetDbSetAsync()).Where(e => ids.Contains(e.Id)).ToListAsync();
    }


}
