namespace TestWorkshop;

/// <summary>
/// 通告消息
/// </summary>
[Route("api/platform/notification")]
public class NotificationController : TestWorkshopController, INotificationAppService
{
    protected INotificationAppService Service { get; }
    public NotificationController(INotificationAppService service)
    {
        Service = service;
    }

    [HttpPost]
    [Route("SendCommon")]
    public Task SendCommonMessageAsync(SendCommonMessageInput input)
    {
        return Service.SendCommonMessageAsync(input);
    }

    [HttpPost]
    [Route("SendBroadCast")]
    public Task SendBroadCastMessageAsync(SendBroadCastMessageInput input)
    {
        return Service.SendBroadCastMessageAsync(input);
    }

    [HttpPut]
    [Route("SetRead")]
    public Task SetReadAsync(NotificationCoreInput input)
    {
        return Service.SetReadAsync(input);
    }

    [HttpPut]
    [Route("SetBatchRead")]
    public Task SetBatchReadAsync(SetBatchReadInput input)
    {
        return Service.SetBatchReadAsync(input);
    }

    [HttpDelete]
    [Route("Delete")]
    public Task DeleteAsync(NotificationDeleteInput input)
    {
        return Service.DeleteAsync(input);
    }

    [HttpGet]
    [Route("GetMyNotification")]
    public Task<PagedResultDto<NotificationOutput>> GetMyNotificationListAsync(NotificationInput input)
    {
        return Service.GetMyNotificationListAsync(input);
    }

    [HttpGet]
    [Route("GetNotification")]
    public Task<PagedResultDto<NotificationOutput>> GetNotificationListAsync(NotificationInput input)
    {
        return Service.GetNotificationListAsync(input);
    }

    [HttpGet]
    [Route("GetSubscription")]
    public Task<PagedResultDto<NotificationSubscriptionOutput>> GetSubscriptionListAsync(NotificationSubscriptionInput input)
    {
        return Service.GetSubscriptionListAsync(input);
    }

}
