namespace TestWorkshop.SignalR;

[HubRoute("SignalR/Notification")]
[Authorize]
[DisableAuditing]
public class NotificationHub : AbpHub<INotificationHub>
{
}
