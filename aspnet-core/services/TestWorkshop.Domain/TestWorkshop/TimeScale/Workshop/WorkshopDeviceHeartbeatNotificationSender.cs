using Volo.Abp.Timing;

namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备通信报警消息发送实现。
/// 收件人：设备所属车间组织单元下的对应角色用户，三级时额外加上全局系统管理员。
/// </summary>
public class WorkshopDeviceHeartbeatNotificationSender :
    DomainService,
    IWorkshopDeviceHeartbeatNotificationSender
{
    private readonly IWorkshopDeviceRepository _deviceRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IMessageManager _messageManager;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<WorkshopDeviceHeartbeatNotificationSender> _logger;

    public WorkshopDeviceHeartbeatNotificationSender(
        IWorkshopDeviceRepository deviceRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IMessageManager messageManager,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<WorkshopDeviceHeartbeatNotificationSender> logger)
    {
        _deviceRepository = deviceRepository;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _messageManager = messageManager;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public virtual async Task NotifyCommunicationAlarmAsync(WorkshopDeviceHeartbeat heartbeat, int level)
    {
        try
        {
            var device = await _deviceRepository.FindAsync(heartbeat.DeviceId);
            if (device == null)
            {
                _logger.LogWarning(
                    "设备 {DeviceCode} 不存在，跳过通信报警通知",
                    heartbeat.DeviceCode);
                return;
            }

            var receivers = new List<IdentityUser>();

            if (device.OrganizationUnitId == Guid.Empty)
            {
                _logger.LogWarning(
                    "设备 {DeviceCode} 未关联车间组织单元，无法发送通信报警",
                    device.Code);
            }
            else
            {
                var workshopUsers = await _identityUserRepository.GetUsersInOrganizationUnitAsync(
                    device.OrganizationUnitId,
                    filter: null,
                    skipCount: 0,
                    maxResultCount: int.MaxValue);

                var workshopRoles = GetWorkshopRoles(level);
                foreach (var user in workshopUsers)
                {
                    var roles = await _identityUserManager.GetRolesAsync(user);
                    if (roles.Any(workshopRoles.Contains))
                    {
                        receivers.Add(user);
                    }
                }
            }

            // 三级报警时追加全局系统管理员
            if (level >= 3)
            {
                var admins = await _identityUserManager.GetUsersInRoleAsync(RoleConstants.admin);
                receivers.AddRange(admins);
            }

            receivers = receivers
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();

            if (receivers.Count == 0)
            {
                _logger.LogWarning(
                    "设备 {DeviceCode} 通信报警（级别 {Level}）没有匹配到收件人，未发送通知",
                    device.Code,
                    level);
                return;
            }

            var elapsedMinutes = Math.Max(0, (_clock.Now - heartbeat.LastHeartbeatAt).TotalMinutes);
            var title = $"通信报警：设备 {device.Code} 通信中断";
            var content =
                $"设备 {device.Code} - {device.Name} 已连续 {elapsedMinutes:F0} 分钟未上报心跳"
                + $"（最近心跳时间：{heartbeat.LastHeartbeatAt:yyyy-MM-dd HH:mm:ss}），请及时检查下位机通信。";
            var messageLevel = level >= 3 ? MessageLevel.Error : MessageLevel.Warning;

            foreach (var receiver in receivers)
            {
                await _messageManager.SendMessageAsync(
                    title,
                    content,
                    MessageType.Common,
                    messageLevel,
                    _currentUser.GetId(),
                    _currentUser.UserName ?? "system",
                    receiver.Id,
                    receiver.UserName ?? string.Empty,
                    null);
            }

            _logger.LogInformation(
                "设备 {DeviceCode} 通信报警（级别 {Level}）已发送，收件人 {Count} 人",
                device.Code,
                level,
                receivers.Count);
        }
        catch (Exception ex)
        {
            // 通知失败不能影响监控 Worker，这里只记录日志
            _logger.LogError(
                ex,
                "设备 {DeviceCode} 通信报警发送失败",
                heartbeat.DeviceCode);
        }
    }

    /// <summary>
    /// 各级对应的车间角色：1=测试工程师，2/3=测试工程师+车间主管。
    /// </summary>
    private static string[] GetWorkshopRoles(int level)
    {
        return level switch
        {
            1 => new[] { RoleConstants.tester },
            _ => new[] { RoleConstants.tester, RoleConstants.supervisor }
        };
    }
}
