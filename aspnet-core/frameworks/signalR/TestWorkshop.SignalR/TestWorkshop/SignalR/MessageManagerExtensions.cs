namespace TestWorkshop.SignalR;

/// <summary>
/// IMessageManager 便捷扩展。
/// </summary>
public static class MessageManagerExtensions
{
    /// <summary>
    /// 以系统身份发送站内消息。<br/>
    /// 供后台 Worker / 定时任务使用：这类场景没有登录用户，
    /// 统一用 <see cref="SystemUser"/> 作为发送人，避免每处都手写。
    /// 注意：接收人必须是系统中真实存在的用户。
    /// </summary>
    public static Task SendSystemMessageAsync(
        this IMessageManager messageManager,
        string title,
        string content,
        MessageType messageType,
        MessageLevel messageLevel,
        Guid receiverUserId,
        string receiverUserName,
        Guid? tenantId = null,
        bool isPersistent = true)
    {
        return messageManager.SendMessageAsync(
            title,
            content,
            messageType,
            messageLevel,
            SystemUser.Id,
            SystemUser.UserName,
            receiverUserId,
            receiverUserName,
            tenantId,
            isPersistent);
    }
}
