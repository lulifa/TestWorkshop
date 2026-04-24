using Volo.Abp.DependencyInjection;

namespace TestWorkshop;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAbpApplicationLocalizationAppService))]
public class SystemApplicationLocalizationAppService : TestWorkshopAppService, IAbpApplicationLocalizationAppService
{
    protected readonly AbpApplicationLocalizationAppService AbpApplicationLocalizationAppService;

    public SystemApplicationLocalizationAppService(AbpApplicationLocalizationAppService abpApplicationLocalizationAppService)
    {
        AbpApplicationLocalizationAppService = abpApplicationLocalizationAppService;
    }

    public virtual async Task<ApplicationLocalizationDto> GetAsync(ApplicationLocalizationRequestDto input)
    {
        var result = await AbpApplicationLocalizationAppService.GetAsync(input);

        return result;

    }
}
