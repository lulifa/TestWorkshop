using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;

namespace TestWorkshop;

[DependsOn(
    typeof(TestWorkshopDomainSharedModule),
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpCachingModule),
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpTenantManagementDomainModule),
    typeof(BlobStoringDatabaseDomainModule)
    )]
public class TestWorkshopDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        context.Services.AddAutoMapperObjectMapper<TestWorkshopDomainModule>();

        Configure<DataItemMappingOptions>(options =>
        {
            options.SetDefaultMapping();
        });

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddProfile<TestWorkshopDomainMappingProfile>(validate: true);
        });

        Configure<AbpDistributedEntityEventOptions>(options =>
        {
            options.EtoMappings.Add<Layout, LayoutEto>(typeof(TestWorkshopDomainModule));
            options.EtoMappings.Add<Menu, MenuEto>(typeof(TestWorkshopDomainModule));
            options.EtoMappings.Add<UserMenu, UserMenuEto>(typeof(TestWorkshopDomainModule));
            options.EtoMappings.Add<RoleMenu, RoleMenuEto>(typeof(TestWorkshopDomainModule));
        });

        Configure<PermissionManagementOptions>(options =>
        {
            options.ManagementProviders.Add<OrganizationUnitPermissionManagementProvider>();

            options.ProviderPolicies[OrganizationUnitPermissionValueProvider.ProviderName] = "AbpIdentity.OrganizationUnits.ManagePermissions";
        });


    }
}
