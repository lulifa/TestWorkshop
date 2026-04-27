namespace TestWorkshop;

/// <summary>
/// 组织单元管理
/// </summary>
[Route("api/system/identity/organization-units")]
public class SystemOrganizationUnitController : TestWorkshopCrudController<
    ISystemOrganizationUnitAppService,
    OrganizationUnitDto,
    OrganizationUnitGetByPagedDto,
    OrganizationUnitCreateDto,
    OrganizationUnitUpdateDto>
{
    public SystemOrganizationUnitController(ISystemOrganizationUnitAppService appService) : base(appService)
    {
    }

    [HttpGet]
    [Route("all")]
    public async virtual Task<ListResultDto<OrganizationUnitDto>> GetAllListAsync()
    {
        return await AppService.GetAllListAsync();
    }

    [HttpGet]
    [Route("last-children")]
    public async virtual Task<OrganizationUnitDto> GetLastChildOrNullAsync(Guid? parentId)
    {
        return await AppService.GetLastChildOrNullAsync(parentId);
    }

    [HttpPut]
    [Route("{id}/move")]
    public async virtual Task MoveAsync(Guid id, OrganizationUnitMoveDto input)
    {
        await AppService.MoveAsync(id, input);
    }

    [HttpGet]
    [Route("root-node")]
    public async virtual Task<ListResultDto<OrganizationUnitDto>> GetRootAsync()
    {
        return await AppService.GetRootAsync();
    }

    [HttpGet]
    [Route("find-children")]
    public async virtual Task<ListResultDto<OrganizationUnitDto>> FindChildrenAsync(OrganizationUnitGetChildrenDto input)
    {
        return await AppService.FindChildrenAsync(input);
    }

    [HttpGet]
    [Route("{id}/role-names")]
    public async virtual Task<ListResultDto<string>> GetRoleNamesAsync(Guid id)
    {
        return await AppService.GetRoleNamesAsync(id);
    }

    [HttpGet]
    [Route("{id}/unadded-roles")]
    public async virtual Task<PagedResultDto<IdentityRoleDto>> GetUnaddedRolesAsync(Guid id, OrganizationUnitGetUnaddedRoleByPagedDto input)
    {
        return await AppService.GetUnaddedRolesAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/roles")]
    public async virtual Task<PagedResultDto<IdentityRoleDto>> GetRolesAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        return await AppService.GetRolesAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/roles")]
    public async virtual Task AddRolesAsync(Guid id, OrganizationUnitAddRoleDto input)
    {
        await AppService.AddRolesAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/unadded-users")]
    public async virtual Task<PagedResultDto<IdentityUserDto>> GetUnaddedUsersAsync(Guid id, OrganizationUnitGetUnaddedUserByPagedDto input)
    {
        return await AppService.GetUnaddedUsersAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/users")]
    public async virtual Task<PagedResultDto<IdentityUserDto>> GetUsersAsync(Guid id, GetIdentityUsersInput input)
    {
        return await AppService.GetUsersAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/users")]
    public async virtual Task AddUsersAsync(Guid id, OrganizationUnitAddUserDto input)
    {
        await AppService.AddUsersAsync(id, input);
    }

}
