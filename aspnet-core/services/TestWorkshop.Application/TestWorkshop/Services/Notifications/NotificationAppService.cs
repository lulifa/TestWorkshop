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
            else
            {
                var subscriptions = await _notificationSubscriptionManager.GetListAsync(
                    input.Id,
                    null,
                    null,
                    null,
                    null,
                    int.MaxValue,
                    0);
                foreach (var subscription in subscriptions)
                {
                    await _notificationSubscriptionManager.DeleteAsync(subscription.Id);
                }
                await _notificationManager.DeleteAsync(input.Id);
            }
        }

    }

    public virtual async Task SetSubscriptionReadAsync(NotificationCoreInput input)
    {
        await _notificationSubscriptionManager.SetReadAsync(
            CurrentUser.GetId(),
            CurrentUser.UserName,
            input.Id);
    }

    public virtual async Task SetSubscriptionBatchReadAsync(SetBatchReadInput input)
    {
        foreach (var id in input.Ids)
        {
            await SetSubscriptionReadAsync(new NotificationCoreInput { Id = id });
        }
    }

    public virtual async Task DeleteSubscriptionAsync(NotificationCoreInput input)
    {
        var subscription = await _notificationSubscriptionManager.FindAsync(
            CurrentUser.GetId(),
            input.Id);
        if (subscription != null)
        {
            await _notificationSubscriptionManager.DeleteAsync(subscription.Id);
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
        if (input.MessageType == MessageType.BroadCast)
        {
            var allList = await _notificationManager.GetListAsync(
                input.Title,
                input.Content,
                input.SenderUserId,
                input.SenderUserName,
                input.ReceiverUserId,
                input.ReceiverUserName,
                null,
                input.StartReadTime,
                input.EndReadTime,
                MessageType.BroadCast,
                input.MessageLevel,
                int.MaxValue,
                0);

            var broadcastItems = new List<NotificationOutput>();
            foreach (var notification in allList)
            {
                var subscription = await _notificationSubscriptionManager.FindAsync(
                    CurrentUser.GetId(),
                    notification.Id);
                var item = ObjectMapper.Map<Notification, NotificationOutput>(notification);
                item.Read = subscription?.Read ?? false;
                item.ReadTime = subscription?.ReadTime;
                broadcastItems.Add(item);
            }

            if (input.Read.HasValue)
            {
                broadcastItems = broadcastItems
                    .Where(item => item.Read == input.Read.Value)
                    .ToList();
            }

            var broadcastTotalCount = broadcastItems.Count;
            if (!input.IsPaged)
            {
                input.SkipCount = 0;
                input.MaxResultCount = int.MaxValue;
            }

            var broadcastPageItems = broadcastItems
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<NotificationOutput>(
                broadcastTotalCount,
                broadcastPageItems);
        }

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
        input.ReceiverUserId ??= CurrentUser.GetId();
        input.ReceiverUserName ??= CurrentUser.UserName;

        var list = await _notificationManager.GetListAsync(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            MessageType.BroadCast,
            null,
            int.MaxValue,
            0);

        var items = new List<NotificationSubscriptionOutput>();
        foreach (var notification in list)
        {
            var subscription = await _notificationSubscriptionManager.FindAsync(
                CurrentUser.GetId(),
                notification.Id);
            items.Add(new NotificationSubscriptionOutput
            {
                Id = notification.Id,
                NotificationId = notification.Id,
                TenantId = notification.TenantId,
                ReceiveUserId = CurrentUser.GetId(),
                ReceiveUserName = CurrentUser.UserName,
                Read = subscription?.Read ?? false,
                ReadTime = subscription?.ReadTime ?? default,
                CreationTime = notification.CreationTime,
                Title = notification.Title,
                Content = notification.Content,
                MessageType = notification.MessageType,
                MessageLevel = notification.MessageLevel,
                SenderUserId = notification.SenderUserId,
                SenderUserName = notification.SenderUserName,
            });
        }

        if (input.Read.HasValue)
        {
            items = items.Where(item => item.Read == input.Read.Value).ToList();
        }

        var totalCount = items.Count;
        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var pageItems = items
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<NotificationSubscriptionOutput>(totalCount, pageItems);

    }
}
