namespace TestWorkshop.Authorization.OrganizationUnits;

[DependsOn(typeof(AbpAuthorizationModule))]
public class AbpAuthorizationOrganizationUnitsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.ValueProviders.Add<OrganizationUnitPermissionValueProvider>();
        });
    }

}
