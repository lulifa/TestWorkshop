namespace TestWorkshop;

public class NotificationManager : DomainService, INotificationManager
{
    private readonly INotificationRepository _notificationRepository;

    private readonly ICurrentUser _currentUser;

    public NotificationManager(INotificationRepository notificationRepository, ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 分页获取消息
    /// </summary>
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
        int skipCount = 0)
    {
        var list = await _notificationRepository.GetListAsync(title, content, senderUserId, senderUserName, receiverUserId, receiverUserName, read, startReadTime, endReadTime, messageType, messageLevel, maxResultCount, skipCount);
        return list;
    }

    /// <summary>
    /// 获取消息总条数
    /// </summary>
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
        MessageLevel? messageLevel)
    {
        return await _notificationRepository.GetCountAsync(title, content, senderUserId, senderUserName, receiverUserId, receiverUserName, read, startReadTime, endReadTime, messageType, messageLevel);
    }

    public async Task CreateAsync(Guid id, string title, string content, MessageType messageType, MessageLevel level, Guid? receiveUserId, string receiveUserName)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException();
        }

        var entity = new Notification(id, title, content, messageType, level, _currentUser.Id.Value, _currentUser.UserName, receiveUserId, receiveUserName, tenantId: CurrentTenant?.Id);

        await _notificationRepository.InsertAsync(entity);
    }


    public async Task<Notification> GetAsync(Guid id)
    {
        var notification = await _notificationRepository.FindAsync(id);
        if (notification == null) throw new NotificationDomainException(TestWorkshopErrorCodes.MessageNotExist);
        return notification;
    }

    public async Task<List<Notification>> GetListAsync(List<Guid> ids)
    {
        var notifications = await _notificationRepository.GetListAsync(ids);
        return notifications;
    }

    public async Task DeleteAsync(Guid id)
    {
        var notification = await _notificationRepository.FindAsync(id);
        if (notification == null) throw new NotificationDomainException(TestWorkshopErrorCodes.MessageNotExist);
        await _notificationRepository.DeleteAsync(notification.Id);
    }

    /// <summary>
    /// 消息设置为已读
    /// </summary>
    /// <param name="id">消息Id</param>
    public async Task SetReadAsync(Guid id)
    {
        if (_currentUser is not { IsAuthenticated: true }) throw new AbpAuthorizationException();

        var notification = await _notificationRepository.FindAsync(id);

        if (notification == null) throw new NotificationDomainException(TestWorkshopErrorCodes.MessageNotExist);
        if (notification.Read)
        {
            return;
        }

        if (notification.MessageType == MessageType.BroadCast)
        {
            return;
        }

        notification.SetRead(true, Clock.Now);

        await _notificationRepository.UpdateAsync(notification);
    }

}
