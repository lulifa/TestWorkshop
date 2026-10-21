namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间开通服务。种子数据和页面初始化共用。
/// </summary>
public class WorkshopProvisioner : DomainService
{
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public WorkshopProvisioner(
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        IdentityRoleManager identityRoleManager,
        IdentityUserManager identityUserManager,
        IIdentityUserRepository identityUserRepository,
        IGuidGenerator guidGenerator,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _organizationUnitRepository = organizationUnitRepository;
        _organizationUnitManager = organizationUnitManager;
        _identityRoleManager = identityRoleManager;
        _identityUserManager = identityUserManager;
        _identityUserRepository = identityUserRepository;
        _guidGenerator = guidGenerator;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public virtual async Task<WorkshopProvisionResult> ProvisionAsync(
        WorkshopProvisionRequest request)
    {
        var displayName = NormalizeRequired(request.DisplayName, "车间名称");
        var businessCode = NormalizeRequired(request.BusinessCode, "业务编码").ToUpperInvariant();
        var accountPrefix = request.AccountPrefix.IsNullOrWhiteSpace()
            ? businessCode.ToLowerInvariant()
            : NormalizeRequired(request.AccountPrefix, "账号前缀").ToLowerInvariant();
        Check.NotNullOrWhiteSpace(request.InitialPassword, nameof(request.InitialPassword));

        var organizationUnit = await EnsureOrganizationUnitAsync(
            request.OrganizationUnitId,
            request.ParentOrganizationUnitId,
            displayName,
            businessCode);

        var definitions = new[]
        {
            new AccountDefinition(
                request.SupervisorUserId,
                $"{accountPrefix}.supervisor",
                $"{displayName}主管",
                RoleConstants.supervisor),
            new AccountDefinition(
                request.RegionalManagerUserId,
                $"{accountPrefix}.regional",
                $"{displayName}大区负责人",
                RoleConstants.regionalManager),
            new AccountDefinition(
                request.TesterUserId,
                $"{accountPrefix}.tester",
                $"{displayName}测试工程师",
                RoleConstants.tester)
        };

        var accounts = new List<WorkshopProvisionedAccount>();
        foreach (var definition in definitions)
        {
            var user = await EnsureUserAsync(
                definition,
                request.InitialPassword,
                organizationUnit.Id);
            accounts.Add(new WorkshopProvisionedAccount
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.Name,
                RoleName = definition.RoleName
            });
        }

        return new WorkshopProvisionResult
        {
            OrganizationUnitId = organizationUnit.Id,
            ParentOrganizationUnitId = organizationUnit.ParentId,
            DisplayName = displayName,
            BusinessCode = businessCode,
            Accounts = accounts
        };
    }

    private async Task<OrganizationUnit> EnsureOrganizationUnitAsync(
        Guid? requestedId,
        Guid? parentId,
        string displayName,
        string businessCode)
    {
        var allOrganizationUnits = await _organizationUnitRepository.GetListAsync(false);
        var existing = allOrganizationUnits.FirstOrDefault(x =>
            string.Equals(
                x.GetProperty<string>(OrganizationUnitConstants.BusinessCode),
                businessCode,
                StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            return existing;
        }

        var organizationUnit = new OrganizationUnit(
            requestedId ?? _guidGenerator.Create(),
            displayName,
            parentId,
            CurrentTenant.Id);
        organizationUnit.SetProperty(OrganizationUnitConstants.BusinessCode, businessCode);

        await _organizationUnitManager.CreateAsync(organizationUnit);
        await _unitOfWorkManager.Current.SaveChangesAsync();
        return organizationUnit;
    }

    private async Task<IdentityUser> EnsureUserAsync(
        AccountDefinition definition,
        string initialPassword,
        Guid organizationUnitId)
    {
        await EnsureRoleAsync(definition.RoleName);

        var user = await _identityUserManager.FindByNameAsync(definition.UserName);
        if (user == null)
        {
            user = new IdentityUser(
                definition.UserId ?? _guidGenerator.Create(),
                definition.UserName,
                $"{definition.UserName}@testworkshop.local",
                CurrentTenant.Id)
            {
                Name = definition.DisplayName
            };

            (await _identityUserManager.CreateAsync(user, initialPassword)).CheckErrors();
        }

        if (!await _identityUserManager.IsInRoleAsync(user, definition.RoleName))
        {
            (await _identityUserManager.AddToRoleAsync(user, definition.RoleName)).CheckErrors();
        }

        var organizationUnits = await _identityUserRepository.GetOrganizationUnitsAsync(
            user.Id,
            filter: null,
            skipCount: 0,
            maxResultCount: int.MaxValue);
        if (organizationUnits.All(x => x.Id != organizationUnitId))
        {
            await _identityUserManager.AddToOrganizationUnitAsync(user.Id, organizationUnitId);
        }

        await _unitOfWorkManager.Current.SaveChangesAsync();
        return user;
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await _identityRoleManager.FindByNameAsync(roleName) != null)
        {
            return;
        }

        var role = new IdentityRole(_guidGenerator.Create(), roleName, CurrentTenant.Id)
        {
            IsStatic = true,
            IsPublic = false,
            IsDefault = false
        };
        (await _identityRoleManager.CreateAsync(role)).CheckErrors();
        await _unitOfWorkManager.Current.SaveChangesAsync();
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (value.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException($"{fieldName}不能为空");
        }

        return value.Trim();
    }

    private sealed record AccountDefinition(
        Guid? UserId,
        string UserName,
        string DisplayName,
        string RoleName);
}
