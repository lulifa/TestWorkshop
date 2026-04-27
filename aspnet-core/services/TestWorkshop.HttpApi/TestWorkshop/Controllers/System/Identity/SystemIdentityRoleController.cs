namespace TestWorkshop;

/// <summary>
/// 角色管理
/// </summary>
[Route("api/system/identity/roles")]
public class SystemIdentityRoleController : TestWorkshopCrudController<
    ISystemIdentityRoleAppService,
    IdentityRoleDto,
    GetIdentityRolesInput,
    IdentityRoleCreateDto,
    IdentityRoleUpdateDto>
{
    public SystemIdentityRoleController(ISystemIdentityRoleAppService appService) : base(appService)
    {
    }

    [HttpGet]
    [Route("{id}/organization-units")]
    public virtual async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id)
    {
        return await AppService.GetOrganizationUnitsAsync(id);
    }

    [HttpPut]
    [Route("{id}/organization-units")]
    public virtual async Task SetOrganizationUnitsAsync(Guid id, IdentityRoleAddOrRemoveOrganizationUnitDto input)
    {
        await AppService.SetOrganizationUnitsAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}/organization-units/{ouId}")]
    public virtual async Task RemoveOrganizationUnitsAsync(Guid id, Guid ouId)
    {
        await AppService.RemoveOrganizationUnitsAsync(id, ouId);
    }

    [HttpGet]
    [Route("all")]
    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        return await AppService.GetAllListAsync();
    }

}
