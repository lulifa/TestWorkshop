using TestWorkshop.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TestWorkshop.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TestWorkshopController : AbpControllerBase
{
    protected TestWorkshopController()
    {
        LocalizationResource = typeof(TestWorkshopResource);
    }
}
