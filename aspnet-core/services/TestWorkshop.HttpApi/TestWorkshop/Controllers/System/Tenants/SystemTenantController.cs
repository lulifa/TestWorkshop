namespace TestWorkshop;

/// <summary>
/// 租户管理
/// </summary>
[Route("api/system/tenants")]
public class SystemTenantController : TestWorkshopCrudController<
    ISystemTenantAppService,
    TenantDto,
    TenantGetListInput,
    TenantCreateDto,
    TenantUpdateDto>
{
    public SystemTenantController(ISystemTenantAppService appService) : base(appService)
    {
    }


    [HttpGet]
    [Route("search/{name}")]
    public virtual Task<FindTenantResultDto> FindTenantByNameAsync(string name)
    {
        return AppService.FindTenantByNameAsync(name);
    }

    [HttpGet]
    [Route("{id}/connection-string/{name}")]
    public virtual Task<TenantConnectionStringDto> GetConnectionStringAsync(Guid id, string name)
    {
        return AppService.GetConnectionStringAsync(id, name);
    }

    [HttpGet]
    [Route("{id}/connection-string")]
    public virtual Task<ListResultDto<TenantConnectionStringDto>> GetConnectionStringAsync(Guid id)
    {
        return AppService.GetConnectionStringAsync(id);
    }

    [HttpPut]
    [Route("{id}/connection-string")]
    public virtual Task<TenantConnectionStringDto> SetConnectionStringAsync(Guid id, TenantConnectionStringSetInput input)
    {
        return AppService.SetConnectionStringAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}/connection-string/{name}")]
    public virtual Task DeleteConnectionStringAsync(Guid id, string name)
    {
        return AppService.DeleteConnectionStringAsync(id, name);
    }
    [HttpPost]
    [Route("connection-string/check")]
    public virtual Task CheckConnectionStringAsync(TenantConnectionStringCheckInput input)
    {
        return AppService.CheckConnectionStringAsync(input);
    }

}
