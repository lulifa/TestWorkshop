namespace TestWorkshop;

/* Inherit your application services from this class.
 */
public abstract class TestWorkshopAppService : ApplicationService
{
    protected TestWorkshopAppService()
    {
        LocalizationResource = typeof(TestWorkshopResource);
    }
}
