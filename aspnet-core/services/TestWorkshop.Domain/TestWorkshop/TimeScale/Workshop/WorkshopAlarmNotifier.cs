namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警消息发送实现。
/// 收件人：报警测试台所属车间组织单元下的对应角色用户，三级时额外加上全局系统管理员。
/// </summary>
public class WorkshopAlarmNotifier : DomainService, IWorkshopAlarmNotifier
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IMessageManager _messageManager;
    private readonly ILogger<WorkshopAlarmNotifier> _logger;

    public WorkshopAlarmNotifier(
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IMessageManager messageManager,
        ILogger<WorkshopAlarmNotifier> logger)
    {
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _messageManager = messageManager;
        _logger = logger;
    }

    public virtual async Task NotifyAsync(WorkshopAlarm alarm, int level, DateTime now)
    {
        try
        {
            var receivers = await ResolveReceiversAsync(alarm.OrganizationUnitId, level);
            if (receivers.Count == 0)
            {
                _logger.LogWarning(
                    "报警 {AlarmType}/{DeviceCode}/{DataItemType}（级别 {Level}）没有匹配到收件人，未发送通知",
                    alarm.AlarmType,
                    alarm.DeviceCode,
                    alarm.DataItemType,
                    level);
                return;
            }

            var elapsedMinutes = Math.Max(0, (now - alarm.StartedAt).TotalMinutes);
            var title = $"【{GetTypeDisplayName(alarm.AlarmType)}】{alarm.DeviceCode} {alarm.Content}";
            var content = BuildContent(alarm, elapsedMinutes, recovered: false);
            var messageLevel = level >= 3 ? MessageLevel.Error : MessageLevel.Warning;

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
                "报警 {AlarmType}/{DeviceCode}/{DataItemType} 级别 {Level} 已发送，收件人 {Count} 人",
                alarm.AlarmType,
                alarm.DeviceCode,
                alarm.DataItemType,
                level,
                receivers.Count);
        }
        catch (Exception ex)
        {
            // 通知失败不能影响报警判定，这里只记录日志
            _logger.LogError(ex, "报警消息发送失败 {DeviceCode}", alarm.DeviceCode);
        }
    }

    public virtual async Task NotifyRecoveredAsync(WorkshopAlarm alarm, DateTime now)
    {
        try
        {
            // 恢复消息发给"恢复前那一级"覆盖到的人
            var receivers = await ResolveReceiversAsync(alarm.OrganizationUnitId, Math.Max(1, alarm.Level));
            if (receivers.Count == 0)
            {
                return;
            }

            var title = $"【{GetTypeDisplayName(alarm.AlarmType)}已恢复】{alarm.DeviceCode} {alarm.Content}";
            var content = BuildContent(alarm, 0, recovered: true);

            foreach (var receiver in receivers)
            {
                await _messageManager.SendSystemMessageAsync(
                    title,
                    content,
                    MessageType.Common,
                    MessageLevel.Information,
                    receiver.Id,
                    receiver.UserName ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复消息发送失败 {DeviceCode}", alarm.DeviceCode);
        }
    }

    private static string BuildContent(WorkshopAlarm alarm, double elapsedMinutes, bool recovered)
    {
        return recovered
            ? $"测试台 {alarm.DeviceCode} 的{alarm.Content}已恢复。"
            : $"测试台 {alarm.DeviceCode} 出现{alarm.Content}，已持续 {elapsedMinutes:F0} 分钟，请及时处理。";
    }

    /// <summary>
    /// 各级对应的角色：1=测试工程师，2/3=测试工程师+车间主管，三级追加系统管理员。
    /// </summary>
    private async Task<List<IdentityUser>> ResolveReceiversAsync(Guid organizationUnitId, int level)
    {
        var receivers = new List<IdentityUser>();

        if (organizationUnitId == Guid.Empty)
        {
            _logger.LogWarning("报警所属车间为空，无法确定收件人");
        }
        else
        {
            var workshopUsers = await _identityUserRepository.GetUsersInOrganizationUnitAsync(
                organizationUnitId,
                filter: null,
                skipCount: 0,
                maxResultCount: int.MaxValue);

            var workshopRoles = level <= 1
                ? new[] { RoleConstants.tester }
                : new[] { RoleConstants.tester, RoleConstants.supervisor };

            foreach (var user in workshopUsers)
            {
                var roles = await _identityUserManager.GetRolesAsync(user);
                if (roles.Any(workshopRoles.Contains))
                {
                    receivers.Add(user);
                }
            }
        }

        if (level >= 3)
        {
            receivers.AddRange(await _identityUserManager.GetUsersInRoleAsync(RoleConstants.admin));
        }

        return receivers
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();
    }

    private static string GetTypeDisplayName(WorkshopAlarmType alarmType)
    {
        return alarmType switch
        {
            WorkshopAlarmType.Threshold => "阈值报警",
            WorkshopAlarmType.Relation => "关联报警",
            WorkshopAlarmType.Device => "设备报警",
            WorkshopAlarmType.Communication => "通信报警",
            _ => "报警"
        };
    }
}
