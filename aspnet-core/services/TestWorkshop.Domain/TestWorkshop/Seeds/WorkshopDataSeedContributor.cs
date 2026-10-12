namespace TestWorkshop;

/// <summary>
/// 初始化默认组织单元和车间设备
/// </summary>
public class WorkshopDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly Guid CompanyOrganizationUnitId =
        Guid.Parse("3a23e341-cb1a-8a31-dc7f-399cc3e5aabe");

    private static readonly Guid HuangdaoOrganizationUnitId =
        Guid.Parse("3a23e341-f6ed-c532-6f66-c7df2c72cc31");

    private static readonly Guid NingboOrganizationUnitId =
        Guid.Parse("3a23e342-24be-80f4-d036-8aeb3cf29fb2");

    private static readonly Guid HuangdaoFivaDeviceId =
        Guid.Parse("3a23e343-7306-67e3-7171-97055130f7aa");

    private static readonly Guid HuangdaoSupervisorUserId =
        Guid.Parse("3a243501-1d0e-4f2a-9a11-000000000001");

    private static readonly Guid HuangdaoRegionalManagerUserId =
        Guid.Parse("3a243502-1d0e-4f2a-9a11-000000000002");

    private static readonly Guid NingboSupervisorUserId =
        Guid.Parse("3a243503-1d0e-4f2a-9a11-000000000003");

    private static readonly Guid NingboRegionalManagerUserId =
        Guid.Parse("3a243504-1d0e-4f2a-9a11-000000000004");

    private static readonly Guid ProjectSupervisorUserId =
        Guid.Parse("3a243505-1d0e-4f2a-9a11-000000000005");

    // 种子用户统一初始密码，后续再按需要调整
    // 注意：必须满足 Identity 密码策略（至少一个大写、一个小写、一个数字、一个非字母数字）
    private const string DefaultUserPassword = "1q2w3E*";

    private readonly ICurrentTenant _currentTenant;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IWorkshopDeviceRepository _workshopDeviceRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IGuidGenerator _guidGenerator;

    public WorkshopDataSeedContributor(
        ICurrentTenant currentTenant,
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        IWorkshopDeviceRepository workshopDeviceRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager,
        IIdentityUserRepository identityUserRepository,
        IGuidGenerator guidGenerator)
    {
        _currentTenant = currentTenant;
        _organizationUnitRepository = organizationUnitRepository;
        _organizationUnitManager = organizationUnitManager;
        _workshopDeviceRepository = workshopDeviceRepository;
        _unitOfWorkManager = unitOfWorkManager;
        _identityUserManager = identityUserManager;
        _identityRoleManager = identityRoleManager;
        _identityUserRepository = identityUserRepository;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId.HasValue)
        {
            return;
        }

        using (_currentTenant.Change(context.TenantId))
        {
            var company = await EnsureOrganizationUnitAsync(
                id: CompanyOrganizationUnitId,
                displayName: "青岛儒海船舶股份有限公司",
                parentId: null,
                businessCode: null,
                tenantId: context.TenantId);

            var huangdao = await EnsureOrganizationUnitAsync(
                id: HuangdaoOrganizationUnitId,
                displayName: "黄岛车间",
                parentId: company.Id,
                businessCode: "HD",
                tenantId: context.TenantId);

            var ningbo = await EnsureOrganizationUnitAsync(
                id: NingboOrganizationUnitId,
                displayName: "宁波车间",
                parentId: company.Id,
                businessCode: "NB",
                tenantId: context.TenantId);

            await EnsureWorkshopDeviceAsync(huangdao.Id, context.TenantId);

            // 每个车间各挂一个车间主管和一个大区负责人
            await EnsureUserAsync(
                HuangdaoSupervisorUserId, "hd.supervisor", "黄岛车间主管",
                "hd.supervisor@testworkshop.local", RoleConstants.supervisor, huangdao.Id, context.TenantId);
            await EnsureUserAsync(
                HuangdaoRegionalManagerUserId, "hd.regional", "黄岛车间大区负责人",
                "hd.regional@testworkshop.local", RoleConstants.regionalManager, huangdao.Id, context.TenantId);
            await EnsureUserAsync(
                NingboSupervisorUserId, "nb.supervisor", "宁波车间主管",
                "nb.supervisor@testworkshop.local", RoleConstants.supervisor, ningbo.Id, context.TenantId);
            await EnsureUserAsync(
                NingboRegionalManagerUserId, "nb.regional", "宁波车间大区负责人",
                "nb.regional@testworkshop.local", RoleConstants.regionalManager, ningbo.Id, context.TenantId);

            // 单船项目主管是订单上的必选人员，先给一个默认账号
            await EnsureUserAsync(
                ProjectSupervisorUserId, "project.supervisor", "单船项目主管",
                "project.supervisor@testworkshop.local", RoleConstants.projectSupervisor, null, context.TenantId);
        }
    }

    private async Task EnsureUserAsync(
        Guid id,
        string userName,
        string displayName,
        string email,
        string roleName,
        Guid? organizationUnitId,
        Guid? tenantId)
    {
        await EnsureRoleAsync(roleName, tenantId);

        var user = await _identityUserManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = new IdentityUser(id, userName, email, tenantId)
            {
                Name = displayName
            };

            (await _identityUserManager.CreateAsync(user, DefaultUserPassword)).CheckErrors();
        }

        if (!await _identityUserManager.IsInRoleAsync(user, roleName))
        {
            (await _identityUserManager.AddToRoleAsync(user, roleName)).CheckErrors();
        }

        if (organizationUnitId.HasValue)
        {
            var organizationUnits = await _identityUserRepository.GetOrganizationUnitsAsync(
                user.Id,
                filter: null,
                skipCount: 0,
                maxResultCount: int.MaxValue);
            if (organizationUnits.All(x => x.Id != organizationUnitId.Value))
            {
                await _identityUserManager.AddToOrganizationUnitAsync(user.Id, organizationUnitId.Value);
            }
        }

        await _unitOfWorkManager.Current.SaveChangesAsync();
    }

    /// <summary>
    /// 角色可能由其他种子贡献者创建，这里保证存在，避免依赖执行顺序
    /// </summary>
    private async Task EnsureRoleAsync(string roleName, Guid? tenantId)
    {
        var role = await _identityRoleManager.FindByNameAsync(roleName);
        if (role != null)
        {
            return;
        }

        role = new IdentityRole(_guidGenerator.Create(), roleName, tenantId)
        {
            IsStatic = true,
            IsPublic = false,
            IsDefault = false
        };

        (await _identityRoleManager.CreateAsync(role)).CheckErrors();
        await _unitOfWorkManager.Current.SaveChangesAsync();
    }

    private async Task<OrganizationUnit> EnsureOrganizationUnitAsync(
        Guid id,
        string displayName,
        Guid? parentId,
        string businessCode,
        Guid? tenantId)
    {
        var organizationUnit = await _organizationUnitRepository.FindAsync(id);
        if (organizationUnit != null)
        {
            return organizationUnit;
        }

        organizationUnit = new OrganizationUnit(
            id,
            displayName,
            parentId,
            tenantId);

        if (!businessCode.IsNullOrWhiteSpace())
        {
            organizationUnit.SetProperty(OrganizationUnitConstants.BusinessCode, businessCode);
        }

        await _organizationUnitManager.CreateAsync(organizationUnit);
        await _unitOfWorkManager.Current.SaveChangesAsync();
        return organizationUnit;
    }

    private async Task EnsureWorkshopDeviceAsync(Guid organizationUnitId, Guid? tenantId)
    {
        var device = await _workshopDeviceRepository.FindByCodeAsync("HD-FIVA-001");
        if (device != null)
        {
            return;
        }

        device = new WorkshopDevice(HuangdaoFivaDeviceId)
        {
            Code = "HD-FIVA-001",
            Name = "FIVA测试台",
            Type = DeviceTypeEnum.FIVA,
            Model = "NAB-II-60",
            OrganizationUnitId = organizationUnitId,
            TenantId = tenantId
        };

        await _workshopDeviceRepository.InsertAsync(device);
        await _unitOfWorkManager.Current.SaveChangesAsync();
    }
}
