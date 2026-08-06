using TestWorkshop.SignalR;
using Volo.Abp.ObjectMapping;

namespace TestWorkshop;

[Authorize]
public class NotificationAppService : TestWorkshopAppService, INotificationAppService
{
    protected INotificationManager _notificationManager { get; }
    protected INotificationSubscriptionManager _notificationSubscriptionManager { get; }
    protected IMessageManager _messageManager { get; }

    public NotificationAppService(INotificationManager notificationManager, INotificationSubscriptionManager notificationSubscriptionManager, IMessageManager messageManager)
    {
        _notificationManager = notificationManager;
        _notificationSubscriptionManager = notificationSubscriptionManager;
        _messageManager = messageManager;
    }

    [Authorize(TestWorkshopPermissions.Notification.Create)]
    public virtual async Task SendCommonMessageAsync(SendCommonMessageInput input)
    {
        await _messageManager.SendMessageAsync(input.Title, input.Content, MessageType.Common, input.MessageLevel, CurrentUser.GetId(), CurrentUser.UserName, input.ReceiveUserId, input.ReceiveUserName, CurrentTenant.Id);
    }

    [Authorize(TestWorkshopPermissions.Notification.Create)]
    public virtual async Task SendBroadCastMessageAsync(SendBroadCastMessageInput input)
    {
        await _messageManager.SendMessageAsync(input.Title, input.Content, MessageType.BroadCast, input.MessageLevel, CurrentUser.GetId(), CurrentUser.UserName, tenantId: CurrentTenant.Id);
    }

    public virtual async Task SetReadAsync(NotificationCoreInput input)
    {
        var notification = await _notificationManager.GetAsync(input.Id);

        if (notification.MessageType == MessageType.Common)
        {
            await _notificationManager.SetReadAsync(input.Id);
        }
        else
        {
            await _notificationSubscriptionManager.SetReadAsync(CurrentUser.GetId(), CurrentUser.UserName, input.Id);
        }

    }


    public virtual async Task SetBatchReadAsync(SetBatchReadInput input)
    {
        foreach (var id in input.Ids)
        {
            await SetReadAsync(new NotificationCoreInput { Id = id });
        }
    }


    [Authorize(TestWorkshopPermissions.Notification.Delete)]
    public virtual async Task DeleteAsync(NotificationDeleteInput input)
    {
        var notification = await _notificationManager.GetAsync(input.Id);

        if (notification.MessageType == MessageType.Common)
        {
            await _notificationManager.DeleteAsync(input.Id);
        }
        else
        {
            if (input.ReceiverUserId.HasValue)
            {
                var subscription = await _notificationSubscriptionManager.FindAsync(input.ReceiverUserId.Value, input.Id);

                if (subscription != null)
                {
                    await _notificationSubscriptionManager.DeleteAsync(subscription.Id);
                }

            }
        }

    }

    public virtual async Task<PagedResultDto<NotificationOutput>> GetMyNotificationListAsync(NotificationInput input)
    {
        input.ReceiverUserId = CurrentUser.GetId();

        input.ReceiverUserName = CurrentUser.UserName;

        input.MessageType = MessageType.Common;

        var totalCount = await _notificationManager.GetCountAsync(
            input.Title,
            input.Content,
            input.SenderUserId,
            input.SenderUserName,
            input.ReceiverUserId,
            input.ReceiverUserName,
            input.Read,
            input.StartReadTime,
            input.EndReadTime,
            input.MessageType,
            input.MessageLevel);

        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var list = await _notificationManager.GetListAsync(
            input.Title,
            input.Content,
            input.SenderUserId,
            input.SenderUserName,
            input.ReceiverUserId,
            input.ReceiverUserName,
            input.Read,
            input.StartReadTime,
            input.EndReadTime,
            input.MessageType,
            input.MessageLevel,
            input.MaxResultCount,
            input.SkipCount);

        var items = ObjectMapper.Map<List<Notification>, List<NotificationOutput>>(list);

        return new PagedResultDto<NotificationOutput>(totalCount, items);

    }

    public virtual async Task<PagedResultDto<NotificationOutput>> GetNotificationListAsync(NotificationInput input)
    {
        var totalCount = await _notificationManager.GetCountAsync(
            input.Title,
            input.Content,
            input.SenderUserId,
            input.SenderUserName,
            input.ReceiverUserId,
            input.ReceiverUserName,
            input.Read,
            input.StartReadTime,
            input.EndReadTime,
            input.MessageType,
            input.MessageLevel);

        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var list = await _notificationManager.GetListAsync(
            input.Title,
            input.Content,
            input.SenderUserId,
            input.SenderUserName,
            input.ReceiverUserId,
            input.ReceiverUserName,
            input.Read,
            input.StartReadTime,
            input.EndReadTime,
            input.MessageType,
            input.MessageLevel,
            input.MaxResultCount,
            input.SkipCount);

        var items = ObjectMapper.Map<List<Notification>, List<NotificationOutput>>(list);

        return new PagedResultDto<NotificationOutput>(totalCount, items);

    }

    public virtual async Task<PagedResultDto<NotificationSubscriptionOutput>> GetSubscriptionListAsync(NotificationSubscriptionInput input)
    {
        var totalCount = await _notificationSubscriptionManager.GetCountAsync(
            input.NotificationId,
            input.ReceiverUserId,
            input.ReceiverUserName,
            input.StartReadTime,
            input.EndReadTime);

        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var list = await _notificationSubscriptionManager.GetListAsync(
            input.NotificationId,
            input.ReceiverUserId,
            input.ReceiverUserName,
            input.StartReadTime,
            input.EndReadTime,
            input.MaxResultCount,
            input.SkipCount);

        var items = ObjectMapper.Map<List<NotificationSubscription>, List<NotificationSubscriptionOutput>>(list);

        return new PagedResultDto<NotificationSubscriptionOutput>(totalCount, items);

    }








}
