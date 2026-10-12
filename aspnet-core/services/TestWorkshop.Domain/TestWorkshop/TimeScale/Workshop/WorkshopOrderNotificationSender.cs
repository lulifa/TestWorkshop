namespace TestWorkshop.TimeScale;

/// <summary>
/// 订单相关消息通知实现。
/// 收件人 = 订单设备所属车间组织单元下，拥有“车间主管”或“大区负责人”角色的用户。
/// </summary>
public class WorkshopOrderNotificationSender :
    DomainService,
    IWorkshopOrderNotificationSender
{
    private readonly IWorkshopDeviceRepository _deviceRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IMessageManager _messageManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<WorkshopOrderNotificationSender> _logger;

    public WorkshopOrderNotificationSender(
        IWorkshopDeviceRepository deviceRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IMessageManager messageManager,
        ICurrentUser currentUser,
        ILogger<WorkshopOrderNotificationSender> logger)
    {
        _deviceRepository = deviceRepository;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _messageManager = messageManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public virtual async Task NotifyOrderCreatedAsync(WorkshopOrder order)
    {
        try
        {
            var device = await _deviceRepository.FindAsync(order.DeviceId);
            if (device == null || device.OrganizationUnitId == Guid.Empty)
            {
                _logger.LogWarning(
                    "订单 {OrderNo} 的设备未关联车间组织单元，跳过消息通知",
                    order.OrderNo);
                return;
            }

            var users = await _identityUserRepository.GetUsersInOrganizationUnitAsync(
                device.OrganizationUnitId,
                filter: null,
                skipCount: 0,
                maxResultCount: int.MaxValue);

            var receivers = new List<IdentityUser>();
            foreach (var user in users)
            {
                var roles = await _identityUserManager.GetRolesAsync(user);
                if (roles.Contains(RoleConstants.supervisor)
                    || roles.Contains(RoleConstants.regionalManager))
                {
                    receivers.Add(user);
                }
            }

            if (receivers.Count == 0)
            {
                _logger.LogWarning(
                    "订单 {OrderNo} 所属车间 {OrganizationUnitId} 下没有车间主管或大区负责人，未发送通知",
                    order.OrderNo,
                    device.OrganizationUnitId);
                return;
            }

            // 标题带上订单号，消息列表里不用点开就能分清是哪一单
            var title = $"新订单待测试：{order.OrderNo}";
            var content =
                $"订单 {order.OrderNo} 已创建，设备 {device.Code} - {device.Name}，"
                + $"要求完成测试时间：{order.RequiredCompletionTime:yyyy-MM-dd HH:mm}";

            foreach (var receiver in receivers)
            {
                await _messageManager.SendMessageAsync(
                    title,
                    content,
                    MessageType.Common,
                    MessageLevel.Information,
                    _currentUser.GetId(),
                    _currentUser.UserName ?? "system",
                    receiver.Id,
                    receiver.UserName ?? string.Empty,
                    CurrentTenant.Id);
            }

            _logger.LogInformation(
                "订单 {OrderNo} 创建通知已发送，收件人 {Count} 人",
                order.OrderNo,
                receivers.Count);
        }
        catch (Exception ex)
        {
            // 通知失败不能影响订单创建，这里只记录日志
            _logger.LogError(ex, "订单 {OrderNo} 创建通知发送失败", order.OrderNo);
        }
    }
}
