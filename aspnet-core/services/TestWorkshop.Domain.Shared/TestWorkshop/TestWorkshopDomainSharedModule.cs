namespace TestWorkshop;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule)
    )]
public class TestWorkshopDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<TestWorkshopDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<TestWorkshopResource>()
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/TestWorkshop/Localization/Resources");

            options.Resources
                  .Get<IdentityResource>()
                  .AddVirtualJson("/TestWorkshop/Localization/Identity");

            options.Resources
                   .Add<AbpSaasResource>()
                   .AddVirtualJson("/TestWorkshop/Localization/Saas");

            options.Resources
                   .Get<AbpOpenIddictResource>()
                   .AddVirtualJson("/TestWorkshop/Localization/OpenIddict");

            options.DefaultResourceType = typeof(TestWorkshopResource);

        });
        
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("TestWorkshop", typeof(TestWorkshopResource));
        });
    }
}
