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

    private static readonly Guid ShenzhenOrganizationUnitId =
        Guid.Parse("3a23e342-9f4b-4c51-8a67-1e2d3c4b5a60");

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

    private static readonly Guid HuangdaoTesterUserId =
        Guid.Parse("3a243506-1d0e-4f2a-9a11-000000000006");

    private static readonly Guid NingboTesterUserId =
        Guid.Parse("3a243507-1d0e-4f2a-9a11-000000000007");

    private static readonly Guid ShenzhenSupervisorUserId =
        Guid.Parse("3a243510-1d0e-4f2a-9a11-000000000010");

    private static readonly Guid ShenzhenRegionalManagerUserId =
        Guid.Parse("3a243511-1d0e-4f2a-9a11-000000000011");

    private static readonly Guid ShenzhenTesterUserId =
        Guid.Parse("3a243512-1d0e-4f2a-9a11-000000000012");

    private static readonly Guid DispatcherUserId =
        Guid.Parse("3a243508-1d0e-4f2a-9a11-000000000008");

    private static readonly Guid AuditorUserId =
        Guid.Parse("3a243509-1d0e-4f2a-9a11-000000000009");

    // 种子用户统一初始密码，后续再按需要调整
    // 注意：必须满足 Identity 密码策略（至少一个大写、一个小写、一个数字、一个非字母数字）
    private const string DefaultUserPassword = "1q2w3E*";

    /// <summary>
    /// 车间种子定义：一个车间组织单元 + 它挂载的标准人员。
    /// 新增车间时，只需在下面的 <see cref="Workshops"/> 里加一条，
    /// 组织单元、车间主管/大区负责人/测试工程师的账号与角色都会自动创建。
    /// </summary>
    private sealed class WorkshopSeedDefinition
    {
        /// <summary>账号前缀，例如 hd / nb，用于生成用户名与邮箱</summary>
        public string AccountPrefix { get; init; }

        public string BusinessCode { get; init; }

        public string DisplayName { get; init; }

        public Guid OrganizationUnitId { get; init; }

        public Guid RegionalManagerUserId { get; init; }

        public Guid SupervisorUserId { get; init; }

        public Guid TesterUserId { get; init; }
    }

    private static readonly WorkshopSeedDefinition[] Workshops =
    {
        new()
        {
            OrganizationUnitId = HuangdaoOrganizationUnitId,
            DisplayName = "黄岛车间",
            BusinessCode = "HD",
            AccountPrefix = "hd",
            SupervisorUserId = HuangdaoSupervisorUserId,
            RegionalManagerUserId = HuangdaoRegionalManagerUserId,
            TesterUserId = HuangdaoTesterUserId
        },
        new()
        {
            OrganizationUnitId = NingboOrganizationUnitId,
            DisplayName = "宁波车间",
            BusinessCode = "NB",
            AccountPrefix = "nb",
            SupervisorUserId = NingboSupervisorUserId,
            RegionalManagerUserId = NingboRegionalManagerUserId,
            TesterUserId = NingboTesterUserId
        },
        new()
        {
            OrganizationUnitId = ShenzhenOrganizationUnitId,
            DisplayName = "深圳车间",
            BusinessCode = "SZ",
            AccountPrefix = "sz",
            SupervisorUserId = ShenzhenSupervisorUserId,
            RegionalManagerUserId = ShenzhenRegionalManagerUserId,
            TesterUserId = ShenzhenTesterUserId
        }
    };

    private readonly ICurrentTenant _currentTenant;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IWorkshopDeviceRepository _workshopDeviceRepository;
    private readonly WorkshopProvisioner _workshopProvisioner;
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
        WorkshopProvisioner workshopProvisioner,
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
        _workshopProvisioner = workshopProvisioner;
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

            // 每个车间：建组织单元，并按约定挂上车间主管 / 大区负责人 / 测试工程师
            foreach (var workshop in Workshops)
            {
                await EnsureWorkshopAsync(company.Id, workshop, context.TenantId);
            }

            await EnsureWorkshopDeviceAsync(HuangdaoOrganizationUnitId, context.TenantId);

            // 单船项目主管是订单上的必选人员，先给一个默认账号
            await EnsureUserAsync(
                ProjectSupervisorUserId, "project.supervisor", "单船项目主管",
                "project.supervisor@testworkshop.local", RoleConstants.projectSupervisor, null, context.TenantId);

            // 调度：全局岗位（订单新建/编辑由调度或管理员负责），不隶属车间
            await EnsureUserAsync(
                DispatcherUserId, "dispatcher", "调度员",
                "dispatcher@testworkshop.local", RoleConstants.dispatcher, null, context.TenantId);

            // 质量审核员：全局岗位，不隶属车间
            await EnsureUserAsync(
                AuditorUserId, "auditor", "质量审核员",
                "auditor@testworkshop.local", RoleConstants.auditor, null, context.TenantId);
        }
    }

    /// <summary>
    /// 确保车间组织单元存在，并按命名约定给车间挂上标准人员：
    /// {前缀}.supervisor 车间主管、{前缀}.regional 大区负责人、{前缀}.tester 测试工程师。
    /// </summary>
    private async Task EnsureWorkshopAsync(
        Guid parentOrganizationUnitId,
        WorkshopSeedDefinition workshop,
        Guid? tenantId)
    {
        await _workshopProvisioner.ProvisionAsync(new WorkshopProvisionRequest
        {
            OrganizationUnitId = workshop.OrganizationUnitId,
            ParentOrganizationUnitId = parentOrganizationUnitId,
            DisplayName = workshop.DisplayName,
            BusinessCode = workshop.BusinessCode,
            AccountPrefix = workshop.AccountPrefix,
            InitialPassword = DefaultUserPassword,
            SupervisorUserId = workshop.SupervisorUserId,
            RegionalManagerUserId = workshop.RegionalManagerUserId,
            TesterUserId = workshop.TesterUserId
        });
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
            OrganizationUnitId = organizationUnitId,
            TenantId = tenantId
        };

        await _workshopDeviceRepository.InsertAsync(device);
        await _unitOfWorkManager.Current.SaveChangesAsync();
    }
}
