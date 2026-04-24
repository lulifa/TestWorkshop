namespace TestWorkshop;

public class SystemDefaultDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    public static readonly string DefaultUserRole = "users";
    protected IGuidGenerator GuidGenerator { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IdentityUserManager IdentityUserManager { get; }
    protected IdentityRoleManager IdentityRoleManager { get; }
    protected IPermissionDataSeeder PermissionDataSeeder { get; }
    protected IPermissionManager PermissionManager { get; }

    public SystemDefaultDataSeederContributor(
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionManager permissionManager)
    {
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
        IdentityUserManager = identityUserManager;
        IdentityRoleManager = identityRoleManager;
        PermissionDataSeeder = permissionDataSeeder;
        PermissionManager = permissionManager;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (CurrentTenant.Change(context.TenantId))
        {
            await CreateDefaultRoleUser(context.TenantId);
        }
    }

    private async Task CreateDefaultRoleUser(Guid? tenantId)
    {
        var defaultRole = await IdentityRoleManager.FindByNameAsync(DefaultUserRole);

        if (defaultRole == null)
        {
            defaultRole = new IdentityRole(GuidGenerator.Create(), DefaultUserRole, tenantId)
            {
                IsStatic = true,
                IsPublic = true,
                IsDefault = true,
            };
            (await IdentityRoleManager.CreateAsync(defaultRole)).CheckErrors();

            var grantedPermissions = new List<string>()
            {
                IdentityPermissions.UserLookup.Default,
                IdentityPermissions.Users.Default,
            };

            grantedPermissions.AddRange(await GetDefaultPermissions());

            await PermissionDataSeeder.SeedAsync(
                RolePermissionValueProvider.ProviderName,
                defaultRole.Name,
                grantedPermissions,
                tenantId: tenantId);
        }
    }

    private async Task<List<string>> GetDefaultPermissions()
    {
        var allPermissions = await PermissionManager.GetAllForRoleAsync("admin");

        var allowedPermissionsPrefix = new[]
           {
                AbpProCoreConsts.ApplicationName,
                AbpProCoreConsts.ModulePlatform
            };

        var allowedPermissions = allPermissions
            .Where(p => allowedPermissionsPrefix.Any(prefix => p.Name.StartsWith(prefix)))
            .Select(p => p.Name)
            .ToList();

        return allowedPermissions;
    }

}
