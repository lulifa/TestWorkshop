namespace TestWorkshop;

public interface INotificationRepository : IBasicRepository<Notification, Guid>
{
    /// <summary>
    /// 分页获取消息
    /// </summary>
    Task<List<Notification>> GetListAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取消息总条数
    /// </summary>
    Task<long> GetCountAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据消息Id列表获取消息列表
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<List<Notification>> GetListAsync(List<Guid> ids);

}
