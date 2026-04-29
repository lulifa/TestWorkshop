namespace TestWorkshop;

[Dependency(ReplaceServices = true)]
public class TestWorkshopBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<TestWorkshopResource> _localizer;

    public TestWorkshopBrandingProvider(IStringLocalizer<TestWorkshopResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
