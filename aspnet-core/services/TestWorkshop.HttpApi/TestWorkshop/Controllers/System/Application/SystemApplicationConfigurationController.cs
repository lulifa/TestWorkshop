namespace TestWorkshop;

/// <summary>
/// 系统应用配置
/// </summary>
[Route("api/system/application-configuration")]
public class SystemApplicationConfigurationController : TestWorkshopController, IAbpApplicationConfigurationAppService
{
    protected readonly IAbpApplicationConfigurationAppService SystemApplicationConfigurationAppService;
    protected readonly IAbpAntiForgeryManager AntiForgeryManager;

    public SystemApplicationConfigurationController(
        IAbpApplicationConfigurationAppService systemApplicationConfigurationAppService,
        IAbpAntiForgeryManager antiForgeryManager)
    {
        SystemApplicationConfigurationAppService = systemApplicationConfigurationAppService;
        AntiForgeryManager = antiForgeryManager;
    }


    [HttpGet]
    public virtual async Task<ApplicationConfigurationDto> GetAsync(ApplicationConfigurationRequestOptions options)
    {
        AntiForgeryManager.SetCookie();
        return await SystemApplicationConfigurationAppService.GetAsync(options);
    }
}
