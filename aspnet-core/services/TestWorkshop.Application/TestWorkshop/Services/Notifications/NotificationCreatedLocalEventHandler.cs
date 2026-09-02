namespace TestWorkshop;

public class NotificationCreatedLocalEventHandler : ILocalEventHandler<CreatedNotificationLocalEvent>, ITransientDependency
{

    private readonly INotificationManager _notificationManager;

    public NotificationCreatedLocalEventHandler(INotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }

    public virtual async Task HandleEventAsync(CreatedNotificationLocalEvent eventData)
    {
        await _notificationManager.CreateAsync(
            eventData.Id,
            eventData.Title,
            eventData.Content,
            eventData.MessageType,
            eventData.MessageLevel,
            eventData.ReceiveUserId,
            eventData.ReceiveUserName);
    }

}
