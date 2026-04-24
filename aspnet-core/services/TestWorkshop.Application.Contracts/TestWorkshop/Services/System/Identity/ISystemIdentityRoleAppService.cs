namespace TestWorkshop;

public interface ISystemIdentityRoleAppService
    : ICrudAppService<
        IdentityRoleDto,
        Guid,
        GetIdentityRolesInput,
        IdentityRoleCreateDto,
        IdentityRoleUpdateDto>
{

    /// <summary>
    /// 获取角色所在的组织架构
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id);

    /// <summary>
    /// 设置角色所在的组织架构
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task SetOrganizationUnitsAsync(Guid id, IdentityRoleAddOrRemoveOrganizationUnitDto input);

    /// <summary>
    /// 从组织架构中移除角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ouId"></param>
    /// <returns></returns>
    Task RemoveOrganizationUnitsAsync(Guid id, Guid ouId);

    /// <summary>
    /// 获取所有角色列表
    /// </summary>
    /// <returns></returns>
    Task<ListResultDto<IdentityRoleDto>> GetAllListAsync();
}
