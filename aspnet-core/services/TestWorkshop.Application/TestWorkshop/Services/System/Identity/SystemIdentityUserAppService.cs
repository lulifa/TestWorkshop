namespace TestWorkshop;

[Authorize]
public class SystemIdentityUserAppService : TestWorkshopAppService, ISystemIdentityUserAppService
{
    protected IdentityUserAppService IdentityUserAppService { get; }
    protected IdentityUserManager UserManager { get; }
    protected IOpenIddictTokenManager TokenManager { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected IIdentityUserRepository UserRepository { get; }

    public SystemIdentityUserAppService(
        IdentityUserAppService identityUserAppService,
        IdentityUserManager userManager,
        IOptions<IdentityOptions> identityOptions,
        IIdentityUserRepository userRepository,
        IOpenIddictTokenManager tokenManager)
    {
        IdentityUserAppService = identityUserAppService;
        UserManager = userManager;
        IdentityOptions = identityOptions;
        UserRepository = userRepository;
        TokenManager = tokenManager;
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

        await foreach (var token in TokenManager.FindBySubjectAsync(user.Id.ToString()))
        {
            await TokenManager.TryRevokeAsync(token);
        }
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


    #region 当前用户资料

    public virtual async Task ChangeCurrentUserPasswordAsync(IdentityUserChangePasswordInput input)
    {
        await IdentityOptions.SetAsync();
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());

        if (user.IsExternal)
        {
            throw new BusinessException(code: IdentityErrorCodes.ExternalUserPasswordChange);
        }

        (await UserManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword)).CheckErrors();
        await CurrentUnitOfWork.SaveChangesAsync();

        await foreach (var token in TokenManager.FindBySubjectAsync(user.Id.ToString()))
        {
            await TokenManager.TryRevokeAsync(token);
        }
    }

    public virtual async Task<IdentityUserDto> GetCurrentUserProfileAsync()
    {
        await IdentityOptions.SetAsync();
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());
        return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
    }

    public virtual async Task<IdentityUserDto> UpdateCurrentUserProfileAsync(IdentityUserProfileInput input)
    {
        await IdentityOptions.SetAsync();
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());

        user.Name = input.Name;
        user.Surname = input.Surname;

        if (!string.Equals(user.Email, input.Email, StringComparison.OrdinalIgnoreCase))
        {
            (await UserManager.SetEmailAsync(user, input.Email)).CheckErrors();
        }

        (await UserManager.UpdateAsync(user)).CheckErrors();

        await CurrentUnitOfWork.SaveChangesAsync();
        return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
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
    public virtual async Task<SystemIdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        var dto = ObjectMapper.Map<IdentityUserDto, SystemIdentityUserDto>(
            await IdentityUserAppService.CreateAsync(input));

        dto.RoleNames = input.RoleNames?.ToList() ?? new List<string>();

        return dto;
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await IdentityUserAppService.DeleteAsync(id);
    }

    public virtual async Task<SystemIdentityUserDto> GetAsync(Guid id)
    {
        var dto = ObjectMapper.Map<IdentityUserDto, SystemIdentityUserDto>(
            await IdentityUserAppService.GetAsync(id));

        dto.RoleNames = (await UserRepository.GetRoleNamesAsync(id)).ToList();

        return dto;
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public virtual async Task<SystemIdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        var dto = ObjectMapper.Map<IdentityUserDto, SystemIdentityUserDto>(
            await IdentityUserAppService.UpdateAsync(id, input));

        dto.RoleNames = input.RoleNames?.ToList() ?? new List<string>();

        return dto;
    }

    public virtual async Task<PagedResultDto<SystemIdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
    {
        var result = await IdentityUserAppService.GetListAsync(input);

        var userIds = result.Items.Select(user => user.Id).ToList();
        var userRoleNames = await UserRepository.GetRoleNamesAsync(userIds);
        var roleNamesMap = userRoleNames.ToDictionary(item => item.Id, item => item.RoleNames);

        var items = result.Items.Select(user =>
        {
            var dto = ObjectMapper.Map<IdentityUserDto, SystemIdentityUserDto>(user);

            dto.RoleNames = roleNamesMap.TryGetValue(user.Id, out var roleNames)
                ? roleNames.ToList()
                : new List<string>();

            return dto;
        }).ToList();

        return new PagedResultDto<SystemIdentityUserDto>(result.TotalCount, items);
    }

    #endregion

}
