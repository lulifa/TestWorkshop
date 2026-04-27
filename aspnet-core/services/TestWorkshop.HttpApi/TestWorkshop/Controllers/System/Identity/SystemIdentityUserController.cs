namespace TestWorkshop;

/// <summary>
/// 用户管理
/// </summary>
[Route("api/system/identity/users")]
public class SystemIdentityUserController : TestWorkshopCrudController<
    ISystemIdentityUserAppService,
    IdentityUserDto,
    GetIdentityUsersInput,
    IdentityUserCreateDto,
    IdentityUserUpdateDto>
{
    public SystemIdentityUserController(ISystemIdentityUserAppService appService) : base(appService)
    {
    }

    [HttpPut]
    [Route("change-password")]
    public virtual async Task ChangePasswordAsync(Guid id, IdentityUserSetPasswordInput input)
    {
        await AppService.ChangePasswordAsync(id, input);
    }

    [HttpPut]
    [Route("{id}/lock/{seconds}")]
    public virtual async Task LockAsync(Guid id, int seconds)
    {
        await AppService.LockAsync(id, seconds);
    }

    [HttpPut]
    [Route("{id}/unlock")]
    public virtual async Task UnLockAsync(Guid id)
    {
        await AppService.UnLockAsync(id);
    }

    [HttpGet]
    [Route("{id}/organization-units")]
    public virtual async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id)
    {
        return await AppService.GetOrganizationUnitsAsync(id);
    }

    [HttpPut]
    [Route("{id}/organization-units")]
    public virtual async Task SetOrganizationUnitsAsync(Guid id, IdentityUserOrganizationUnitUpdateDto input)
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
    [Route("{id}/roles")]
    public virtual async Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id)
    {
        return await AppService.GetRolesAsync(id);
    }

    [HttpGet]
    [Route("assignable-roles")]
    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync()
    {
        return await AppService.GetAssignableRolesAsync();
    }

    [HttpPut]
    [Route("{id}/roles")]
    public virtual async Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
    {
        await AppService.UpdateRolesAsync(id, input);
    }

    [HttpGet]
    [Route("by-username/{userName}")]
    public virtual async Task<IdentityUserDto> FindByUsernameAsync(string userName)
    {
        return await AppService.FindByUsernameAsync(userName);
    }

    [HttpGet]
    [Route("by-email/{email}")]
    public virtual async Task<IdentityUserDto> FindByEmailAsync(string email)
    {
        return await AppService.FindByEmailAsync(email);
    }

}
