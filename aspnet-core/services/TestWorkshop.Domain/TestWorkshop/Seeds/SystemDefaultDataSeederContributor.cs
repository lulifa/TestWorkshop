namespace TestWorkshop;

public class SystemDefaultDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly string[] RolesToCreate =
    {
        "supervisor",   // 车间主管
        "tester",       // 测试工程师
        "auditor",      // 质量审核员
        "guest"         // 访客
    };
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
            await CreateRolesIfNotExistAsync(context.TenantId);
        }
    }

    private async Task CreateRolesIfNotExistAsync(Guid? tenantId)
    {
        foreach (var roleName in RolesToCreate)
        {
            var role = await IdentityRoleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole(GuidGenerator.Create(), roleName, tenantId)
                {
                    IsStatic = true,   // 静态角色，不可在UI中删除
                    IsPublic = false,  // 不允许用户公开申请（可根据需要调整）
                    IsDefault = false  // 不是默认注册角色
                };
                (await IdentityRoleManager.CreateAsync(role)).CheckErrors();
            }
        }
    }

}
