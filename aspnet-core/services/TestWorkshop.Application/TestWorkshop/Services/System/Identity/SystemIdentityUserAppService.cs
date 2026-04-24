namespace TestWorkshop;

[Authorize(IdentityPermissions.Users.Default)]
public class SystemIdentityUserAppService : TestWorkshopAppService, ISystemIdentityUserAppService
{
    protected IdentityUserAppService IdentityUserAppService { get; }
    protected IdentityUserManager UserManager { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }

    public SystemIdentityUserAppService(
        IdentityUserAppService identityUserAppService,
        IdentityUserManager userManager,
        IOptions<IdentityOptions> identityOptions)
    {
        IdentityUserAppService = identityUserAppService;
        UserManager = userManager;
        IdentityOptions = identityOptions;
    }


    #region 用户拓展高级方法

    [Authorize(TestWorkshopPermissions.Users.ResetPassword)]
    public async virtual Task ChangePasswordAsync(Guid id, IdentityUserSetPasswordInput input)
    {
        var user = await GetUserAsync(id);

        if (user.IsExternal)
        {
            throw new BusinessException(code: IdentityErrorCodes.ExternalUserPasswordChange);
        }

        if (user.PasswordHash == null)
        {
            (await UserManager.AddPasswordAsync(user, input.Password)).CheckErrors();
        }
        else
        {
            var token = await UserManager.GeneratePasswordResetTokenAsync(user);

            (await UserManager.ResetPasswordAsync(user, token, input.Password)).CheckErrors();
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public async virtual Task LockAsync(Guid id, int seconds)
    {
        var user = await GetUserAsync(id);

        var endDate = new DateTimeOffset(Clock.Now).AddSeconds(seconds);

        (await UserManager.SetLockoutEndDateAsync(user, endDate)).CheckErrors();

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public async virtual Task UnLockAsync(Guid id)
    {
        var user = await GetUserAsync(id);

        (await UserManager.SetLockoutEndDateAsync(user, null)).CheckErrors();

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected async virtual Task<IdentityUser> GetUserAsync(Guid id)
    {
        await IdentityOptions.SetAsync();
        var user = await UserManager.GetByIdAsync(id);

        return user;
    }

    #endregion


    #region 用户拓展组织单元

    public async virtual Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id)
    {
        var user = await UserManager.GetByIdAsync(id);

        var origanizationUnits = await UserManager.GetOrganizationUnitsAsync(user);

        return new ListResultDto<OrganizationUnitDto>(
            ObjectMapper.Map<List<OrganizationUnit>, List<OrganizationUnitDto>>(origanizationUnits));
    }

    [Authorize(TestWorkshopPermissions.Users.ManageOrganizationUnits)]
    public async virtual Task SetOrganizationUnitsAsync(Guid id, IdentityUserOrganizationUnitUpdateDto input)
    {
        var user = await UserManager.GetByIdAsync(id);

        await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnitIds);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [Authorize(TestWorkshopPermissions.Users.ManageOrganizationUnits)]
    public async virtual Task RemoveOrganizationUnitsAsync(Guid id, Guid ouId)
    {
        await UserManager.RemoveFromOrganizationUnitAsync(id, ouId);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    #endregion


    #region Abp用户拓展方法

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id)
    {
        return await IdentityUserAppService.GetRolesAsync(id);
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync()
    {
        return await IdentityUserAppService.GetAssignableRolesAsync();
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public virtual async Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
    {
        await IdentityUserAppService.UpdateRolesAsync(id, input);
    }

    public virtual async Task<IdentityUserDto> FindByUsernameAsync(string userName)
    {
        return await IdentityUserAppService.FindByUsernameAsync(userName);
    }

    public virtual async Task<IdentityUserDto> FindByEmailAsync(string email)
    {
        return await IdentityUserAppService.FindByEmailAsync(email);
    }



    [Authorize(IdentityPermissions.Users.Create)]
    public virtual async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        return await IdentityUserAppService.CreateAsync(input);
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await IdentityUserAppService.DeleteAsync(id);
    }

    public virtual async Task<IdentityUserDto> GetAsync(Guid id)
    {
        return await IdentityUserAppService.GetAsync(id);
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public virtual async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        return await IdentityUserAppService.UpdateAsync(id, input);
    }

    public virtual async Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
    {
        return await IdentityUserAppService.GetListAsync(input);
    }

    #endregion

}
