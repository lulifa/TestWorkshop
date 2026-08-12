namespace TestWorkshop;

public interface INotificationAppService : IApplicationService
{

    Task SendCommonMessageAsync(SendCommonMessageInput input);

    Task SendBroadCastMessageAsync(SendBroadCastMessageInput input);

    Task SetReadAsync(NotificationCoreInput input);

    Task SetBatchReadAsync(SetBatchReadInput input);

    Task DeleteAsync(NotificationDeleteInput input);

    Task SetSubscriptionReadAsync(NotificationCoreInput input);

    Task SetSubscriptionBatchReadAsync(SetBatchReadInput input);

    Task DeleteSubscriptionAsync(NotificationCoreInput input);

    Task<PagedResultDto<NotificationOutput>> GetMyNotificationListAsync(NotificationInput input);

    Task<PagedResultDto<NotificationOutput>> GetNotificationListAsync(NotificationInput input);

    Task<PagedResultDto<NotificationSubscriptionOutput>> GetSubscriptionListAsync(NotificationSubscriptionInput input);

}
