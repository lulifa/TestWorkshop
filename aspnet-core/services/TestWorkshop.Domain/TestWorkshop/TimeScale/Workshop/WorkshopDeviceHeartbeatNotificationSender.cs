using Volo.Abp.Timing;

namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备通信报警/恢复消息发送实现。
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
    private readonly IClock _clock;
    private readonly ILogger<WorkshopDeviceHeartbeatNotificationSender> _logger;

    public WorkshopDeviceHeartbeatNotificationSender(
        IWorkshopDeviceRepository deviceRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IMessageManager messageManager,
        IClock clock,
        ILogger<WorkshopDeviceHeartbeatNotificationSender> logger)
    {
        _deviceRepository = deviceRepository;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _messageManager = messageManager;
        _clock = clock;
        _logger = logger;
    }

    public virtual Task NotifyCommunicationAlarmAsync(WorkshopDeviceHeartbeat heartbeat, int level)
    {
        return NotifyAsync(heartbeat, level, recovered: false);
    }

    public virtual Task NotifyCommunicationRecoveredAsync(WorkshopDeviceHeartbeat heartbeat, int level)
    {
        return NotifyAsync(heartbeat, level, recovered: true);
    }

    private async Task NotifyAsync(WorkshopDeviceHeartbeat heartbeat, int level, bool recovered)
    {
        try
        {
            var device = await _deviceRepository.FindAsync(heartbeat.DeviceId);
            if (device == null)
            {
                _logger.LogWarning(
                    "设备 {DeviceCode} 不存在，跳过通信消息通知",
                    heartbeat.DeviceCode);
                return;
            }

            var receivers = await ResolveReceiversAsync(device, level);
            if (receivers.Count == 0)
            {
                _logger.LogWarning(
                    "设备 {DeviceCode} 通信消息（级别 {Level}，恢复={Recovered}）没有匹配到收件人，未发送通知",
                    device.Code,
                    level,
                    recovered);
                return;
            }

            string title;
            string content;
            MessageLevel messageLevel;
            if (recovered)
            {
                title = $"通信恢复：设备 {device.Code} 通信已恢复";
                content =
                    $"设备 {device.Code} - {device.Name} 通信已恢复正常"
                    + $"（最近心跳时间：{heartbeat.LastHeartbeatAt:yyyy-MM-dd HH:mm:ss}）。";
                messageLevel = MessageLevel.Information;
            }
            else
            {
                var elapsedMinutes = Math.Max(0, (_clock.Now - heartbeat.LastHeartbeatAt).TotalMinutes);
                title = $"通信报警：设备 {device.Code} 通信中断";
                content =
                    $"设备 {device.Code} - {device.Name} 已连续 {elapsedMinutes:F0} 分钟未上报心跳"
                    + $"（最近心跳时间：{heartbeat.LastHeartbeatAt:yyyy-MM-dd HH:mm:ss}），请及时检查下位机通信。";
                messageLevel = level >= 3 ? MessageLevel.Error : MessageLevel.Warning;
            }

            // 后台任务没有登录用户，统一用系统身份发送（见 SendSystemMessageAsync）。
            foreach (var receiver in receivers)
            {
                await _messageManager.SendSystemMessageAsync(
                    title,
                    content,
                    MessageType.Common,
                    messageLevel,
                    receiver.Id,
                    receiver.UserName ?? string.Empty);
            }

            _logger.LogInformation(
                "设备 {DeviceCode} 通信{Action}（级别 {Level}）已发送，收件人 {Count} 人",
                device.Code,
                recovered ? "恢复" : "报警",
                level,
                receivers.Count);
        }
        catch (Exception ex)
        {
            // 通知失败不能影响监控 Worker，这里只记录日志
            _logger.LogError(ex, "设备 {DeviceCode} 通信消息发送失败", heartbeat.DeviceCode);
        }
    }

    /// <summary>
    /// 按级别解析收件人：车间测试工程师/车间主管按角色取，三级追加全局系统管理员。
    /// </summary>
    private async Task<List<IdentityUser>> ResolveReceiversAsync(WorkshopDevice device, int level)
    {
        var receivers = new List<IdentityUser>();

        if (device.OrganizationUnitId == Guid.Empty)
        {
            _logger.LogWarning("设备 {DeviceCode} 未关联车间组织单元，无法确定收件人", device.Code);
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

        return receivers
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();
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
