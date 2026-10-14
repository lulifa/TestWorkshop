namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间（组织单元）可见性判断。设备管理、报警管理等共用同一套口径，避免各写一份。
/// </summary>
public interface IWorkshopOrganizationUnitAccessChecker
{
    /// <summary>
    /// 获取当前用户可访问的组织单元；管理员返回 null 表示不限制
    /// </summary>
    Task<HashSet<Guid>> GetAccessibleOrganizationUnitIdsAsync();

    /// <summary>
    /// 校验当前用户是否可以访问指定组织单元；message 为空时使用默认提示
    /// </summary>
    Task EnsureAccessibleAsync(Guid organizationUnitId, string message = null);
}
