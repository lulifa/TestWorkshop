namespace TestWorkshop.SignalR;

/// <summary>
/// 后台任务（没有登录用户）发送消息时使用的系统身份。
/// 该身份不需要存在于用户表中，只作为消息的发送人记录。
/// </summary>
public static class SystemUser
{
    /// <summary>
    /// 系统发送人ID（Guid.Empty，不对应任何真实用户）
    /// </summary>
    public static readonly Guid Id = Guid.Empty;

    /// <summary>
    /// 系统发送人用户名
    /// </summary>
    public const string UserName = "system";
}
