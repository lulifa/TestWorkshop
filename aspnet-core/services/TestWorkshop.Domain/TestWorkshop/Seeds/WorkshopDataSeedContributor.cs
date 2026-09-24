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

    private readonly ICurrentTenant _currentTenant;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IWorkshopDeviceRepository _workshopDeviceRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public WorkshopDataSeedContributor(
        ICurrentTenant currentTenant,
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        IWorkshopDeviceRepository workshopDeviceRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _currentTenant = currentTenant;
        _organizationUnitRepository = organizationUnitRepository;
        _organizationUnitManager = organizationUnitManager;
        _workshopDeviceRepository = workshopDeviceRepository;
        _unitOfWorkManager = unitOfWorkManager;
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

            await EnsureOrganizationUnitAsync(
                id: NingboOrganizationUnitId,
                displayName: "宁波车间",
                parentId: company.Id,
                businessCode: "NB",
                tenantId: context.TenantId);

            await EnsureWorkshopDeviceAsync(huangdao.Id, context.TenantId);
        }
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
