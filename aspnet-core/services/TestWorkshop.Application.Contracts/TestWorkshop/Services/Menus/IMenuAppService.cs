namespace TestWorkshop;

public interface IMenuAppService : 
    ICrudAppService<
        MenuDto,
        Guid,
        MenuGetListInput,
        MenuCreateDto,
        MenuUpdateDto>
{

    Task<ListResultDto<MenuDto>> GetCurrentUserMenuListAsync(GetMenuInput input);

    Task<ListResultDto<MenuDto>> GetAllAsync(MenuGetAllInput input);

    Task<ListResultDto<MenuDto>> GetUserMenuListAsync(MenuGetByUserInput input);

    Task<ListResultDto<MenuDto>> GetRoleMenuListAsync(MenuGetByRoleInput input);

    Task SetUserMenusAsync(UserMenuInput input);

    Task SetUserStartupAsync(Guid id, UserMenuStartupInput input);

    Task SetRoleMenusAsync(RoleMenuInput input);

    Task SetRoleStartupAsync(Guid id, RoleMenuStartupInput input);

}
