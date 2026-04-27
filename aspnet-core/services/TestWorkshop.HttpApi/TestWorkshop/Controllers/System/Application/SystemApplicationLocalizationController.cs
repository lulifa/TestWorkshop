namespace TestWorkshop;

/// <summary>
/// 系统应用多语言
/// </summary>
[Route("api/system/application-localization")]
public class SystemApplicationLocalizationController : TestWorkshopController, IAbpApplicationLocalizationAppService
{
    protected readonly IAbpApplicationLocalizationAppService SystemApplicationLocalizationAppService;

    public SystemApplicationLocalizationController(IAbpApplicationLocalizationAppService systemApplicationLocalizationAppService)
    {
        SystemApplicationLocalizationAppService = systemApplicationLocalizationAppService;
    }


    [HttpGet]
    public virtual async Task<ApplicationLocalizationDto> GetAsync(ApplicationLocalizationRequestDto input)
    {
        return await SystemApplicationLocalizationAppService.GetAsync(input);
    }
}
