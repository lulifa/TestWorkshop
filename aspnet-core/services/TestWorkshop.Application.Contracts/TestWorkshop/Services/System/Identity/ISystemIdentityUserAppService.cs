namespace TestWorkshop;

public interface ISystemIdentityUserAppService
    : ICrudAppService<
        IdentityUserDto,
        Guid,
        GetIdentityUsersInput,
        IdentityUserCreateDto,
        IdentityUserUpdateDto>
{

    /// <summary>
    /// 更改用户密码
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task ChangePasswordAsync(Guid id, IdentityUserSetPasswordInput input);

    /// <summary>
    /// 锁定用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    Task LockAsync(Guid id, int seconds);

    /// <summary>
    /// 解锁用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task UnLockAsync(Guid id);

    /// <summary>
    /// 获取用户组织结构列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id);

    /// <summary>
    /// 设置用户组织结构
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task SetOrganizationUnitsAsync(Guid id, IdentityUserOrganizationUnitUpdateDto input);

    /// <summary>
    /// 从组织机构中移除用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ouId"></param>
    /// <returns></returns>
    Task RemoveOrganizationUnitsAsync(Guid id, Guid ouId);

    /// <summary>
    /// 获取用户角色列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id);

    /// <summary>
    /// 获取可用的角色列表
    /// </summary>
    /// <returns></returns>
    Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync();

    /// <summary>
    /// 更新用户角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input);

    /// <summary>
    /// 根据用户名查找用户
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<IdentityUserDto> FindByUsernameAsync(string userName);

    /// <summary>
    /// 根据邮箱查找用户
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<IdentityUserDto> FindByEmailAsync(string email);
}
