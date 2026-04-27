namespace TestWorkshop;

/// <summary>
/// 菜单管理
/// </summary>
[Route("api/platform/menus")]
public class MenuController : TestWorkshopCrudController<
    IMenuAppService,
    MenuDto,
    MenuGetListInput,
    MenuCreateDto,
    MenuUpdateDto>
{
    protected IUserRoleFinder UserRoleFinder { get; }
    public MenuController(IMenuAppService appService, IUserRoleFinder userRoleFinder) : base(appService)
    {
        UserRoleFinder = userRoleFinder;
    }

    [HttpGet]
    [Route("by-current-user")]
    public async virtual Task<ListResultDto<MenuDto>> GetCurrentUserMenuListAsync(GetMenuInput input)
    {
        return await AppService.GetCurrentUserMenuListAsync(input);
    }

    [HttpGet]
    [Route("all")]
    public async virtual Task<ListResultDto<MenuDto>> GetAllAsync(MenuGetAllInput input)
    {
        return await AppService.GetAllAsync(input);
    }

    [HttpGet]
    [Route("by-user")]
    public async virtual Task<ListResultDto<MenuDto>> GetUserMenuListAsync(MenuGetByUserInput input)
    {
        return await AppService.GetUserMenuListAsync(input);
    }

    [HttpPut]
    [Route("by-user")]
    public async virtual Task SetUserMenusAsync(UserMenuInput input)
    {
        await AppService.SetUserMenusAsync(input);
    }

    [HttpPut]
    [Route("startup/{id}/by-user")]
    public async virtual Task SetUserStartupAsync(Guid id, UserMenuStartupInput input)
    {
        await AppService.SetUserStartupAsync(id, input);
    }

    [HttpPut]
    [Route("by-role")]
    public async virtual Task SetRoleMenusAsync(RoleMenuInput input)
    {
        await AppService.SetRoleMenusAsync(input);
    }

    [HttpPut]
    [Route("startup/{id}/by-role")]
    public async virtual Task SetRoleStartupAsync(Guid id, RoleMenuStartupInput input)
    {
        await AppService.SetRoleStartupAsync(id, input);
    }

    [HttpGet]
    [Route("by-role")]
    public async virtual Task<ListResultDto<MenuDto>> GetRoleMenuListAsync(MenuGetByRoleInput input)
    {
        return await AppService.GetRoleMenuListAsync(input);
    }

}
