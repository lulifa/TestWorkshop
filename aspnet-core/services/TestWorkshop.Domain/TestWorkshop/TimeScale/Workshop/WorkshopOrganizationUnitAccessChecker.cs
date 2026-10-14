namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间可见性判断实现：管理员不限制；其他人的可见范围 = 自己所属的组织单元。
/// </summary>
public class WorkshopOrganizationUnitAccessChecker :
    DomainService,
    IWorkshopOrganizationUnitAccessChecker
{
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentUser _currentUser;

    public WorkshopOrganizationUnitAccessChecker(
        IdentityUserManager userManager,
        ICurrentUser currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public virtual async Task<HashSet<Guid>> GetAccessibleOrganizationUnitIdsAsync()
    {
        if (_currentUser.IsInRole(RoleConstants.admin))
        {
            return null;
        }

        if (_currentUser.Id == null)
        {
            return new HashSet<Guid>();
        }

        var user = await _userManager.GetByIdAsync(_currentUser.GetId());
        var organizationUnits = await _userManager.GetOrganizationUnitsAsync(user);
        return organizationUnits.Select(x => x.Id).ToHashSet();
    }

    public virtual async Task EnsureAccessibleAsync(Guid organizationUnitId)
    {
        var accessibleIds = await GetAccessibleOrganizationUnitIdsAsync();
        if (accessibleIds != null && !accessibleIds.Contains(organizationUnitId))
        {
            throw new UserFriendlyException("无权访问该车间的数据");
        }
    }
}
