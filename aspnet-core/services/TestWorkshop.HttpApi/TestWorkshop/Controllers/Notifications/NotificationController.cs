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
    [Route("send-common")]
    public Task SendCommonMessageAsync(SendCommonMessageInput input)
    {
        return Service.SendCommonMessageAsync(input);
    }

    [HttpPost]
    [Route("send-broadcast")]
    public Task SendBroadCastMessageAsync(SendBroadCastMessageInput input)
    {
        return Service.SendBroadCastMessageAsync(input);
    }

    [HttpPut]
    [Route("set-read")]
    public Task SetReadAsync(NotificationCoreInput input)
    {
        return Service.SetReadAsync(input);
    }

    [HttpPut]
    [Route("set-batchread")]
    public Task SetBatchReadAsync(SetBatchReadInput input)
    {
        return Service.SetBatchReadAsync(input);
    }

    [HttpDelete]
    [Route("delete")]
    public Task DeleteAsync(NotificationDeleteInput input)
    {
        return Service.DeleteAsync(input);
    }

    [HttpGet]
    [Route("my-notification")]
    public Task<PagedResultDto<NotificationOutput>> GetMyNotificationListAsync(NotificationInput input)
    {
        return Service.GetMyNotificationListAsync(input);
    }

    [HttpGet]
    [Route("notification")]
    public Task<PagedResultDto<NotificationOutput>> GetNotificationListAsync(NotificationInput input)
    {
        return Service.GetNotificationListAsync(input);
    }

    [HttpGet]
    [Route("subscription")]
    public Task<PagedResultDto<NotificationSubscriptionOutput>> GetSubscriptionListAsync(NotificationSubscriptionInput input)
    {
        return Service.GetSubscriptionListAsync(input);
    }

}
