namespace TestWorkshop;

[Authorize(IdentityPermissions.Roles.Default)]
public class SystemIdentityRoleAppService : TestWorkshopAppService, ISystemIdentityRoleAppService
{
    protected IdentityRoleAppService IdentityRoleAppService { get; }
    protected IIdentityRoleRepository IdentityRoleRepository { get; }
    protected OrganizationUnitManager OrganizationUnitManager { get; }
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    public SystemIdentityRoleAppService(
        IdentityRoleAppService identityRoleAppService,
        IIdentityRoleRepository roleRepository,
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository)
    {
        IdentityRoleAppService = identityRoleAppService;
        IdentityRoleRepository = roleRepository;
        OrganizationUnitManager = organizationUnitManager;
        OrganizationUnitRepository = organizationUnitRepository;
    }


    #region 角色拓展组织单元

    [Authorize(TestWorkshopPermissions.Roles.ManageOrganizationUnits)]
    public virtual async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id)
    {
        var origanizationUnits = await IdentityRoleRepository.GetOrganizationUnitsAsync(id);

        return new ListResultDto<OrganizationUnitDto>(
            ObjectMapper.Map<List<OrganizationUnit>, List<OrganizationUnitDto>>(origanizationUnits));
    }

    [Authorize(TestWorkshopPermissions.Roles.ManageOrganizationUnits)]
    public virtual async Task SetOrganizationUnitsAsync(Guid id, IdentityRoleAddOrRemoveOrganizationUnitDto input)
    {
        var origanizationUnits = await IdentityRoleRepository.GetOrganizationUnitsAsync(id, true);

        var notInRoleOuIds = input.OrganizationUnitIds.Where(ouid => !origanizationUnits.Any(ou => ou.Id.Equals(ouid)));

        foreach (var ouId in notInRoleOuIds)
        {
            await OrganizationUnitManager.AddRoleToOrganizationUnitAsync(id, ouId);
        }

        var removeRoleOriganzationUnits = origanizationUnits.Where(ou => !input.OrganizationUnitIds.Contains(ou.Id));
        foreach (var origanzationUnit in removeRoleOriganzationUnits)
        {
            origanzationUnit.RemoveRole(id);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [Authorize(TestWorkshopPermissions.Roles.ManageOrganizationUnits)]
    public virtual async Task RemoveOrganizationUnitsAsync(Guid id, Guid ouId)
    {
        await OrganizationUnitManager.RemoveRoleFromOrganizationUnitAsync(id, ouId);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    #endregion


    #region Abp角色拓展方法

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        return await IdentityRoleAppService.GetAllListAsync();
    }



    [Authorize(IdentityPermissions.Roles.Create)]
    public virtual async Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        return await IdentityRoleAppService.CreateAsync(input);
    }

    [Authorize(IdentityPermissions.Roles.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await IdentityRoleAppService.DeleteAsync(id);
    }

    public virtual async Task<IdentityRoleDto> GetAsync(Guid id)
    {
        return await IdentityRoleAppService.GetAsync(id);
    }

    [Authorize(IdentityPermissions.Roles.Update)]
    public virtual async Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        return await IdentityRoleAppService.UpdateAsync(id, input);
    }

    public virtual async Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRolesInput input)
    {
        return await IdentityRoleAppService.GetListAsync(input);
    }

    #endregion

}
