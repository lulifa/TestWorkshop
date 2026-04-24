namespace TestWorkshop;

public interface ISystemOrganizationUnitAppService : 
    ICrudAppService<OrganizationUnitDto,
                    Guid,
                    OrganizationUnitGetByPagedDto,
                    OrganizationUnitCreateDto,
                    OrganizationUnitUpdateDto>
{
    /// <summary>
    /// 查询组织机构列表
    /// </summary>
    /// <returns></returns>
    Task<ListResultDto<OrganizationUnitDto>> GetAllListAsync();

    /// <summary>
    /// 获取指定父级的最后一个子节点，如果没有则返回null
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns></returns>
    Task<OrganizationUnitDto> GetLastChildOrNullAsync(Guid? parentId);

    /// <summary>
    /// 移动组织机构
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task MoveAsync(Guid id, OrganizationUnitMoveDto input);

    /// <summary>
    /// 查询根组织机构列表
    /// </summary>
    /// <returns></returns>
    Task<ListResultDto<OrganizationUnitDto>> GetRootAsync();

    /// <summary>
    /// 查询下级组织机构列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<ListResultDto<OrganizationUnitDto>> FindChildrenAsync(OrganizationUnitGetChildrenDto input);

    /// <summary>
    /// 获取组织机构关联的角色名称列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ListResultDto<string>> GetRoleNamesAsync(Guid id);

    /// <summary>
    /// 查询未加入组织机构的角色列表
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<IdentityRoleDto>> GetUnaddedRolesAsync(Guid id, OrganizationUnitGetUnaddedRoleByPagedDto input);

    /// <summary>
    /// 获取组织机构已关联的角色列表
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<IdentityRoleDto>> GetRolesAsync(Guid id, PagedAndSortedResultRequestDto input);

    /// <summary>
    /// 角色添加到组织机构
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddRolesAsync(Guid id, OrganizationUnitAddRoleDto input);

    /// <summary>
    /// 查询未添加到组织机构的用户列表
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<IdentityUserDto>> GetUnaddedUsersAsync(Guid id, OrganizationUnitGetUnaddedUserByPagedDto input);

    /// <summary>
    /// 查询组织机构已关联的用户列表
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<IdentityUserDto>> GetUsersAsync(Guid id, GetIdentityUsersInput input);

    /// <summary>
    /// 用户添加到组织机构
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddUsersAsync(Guid id, OrganizationUnitAddUserDto input);
}
