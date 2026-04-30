using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;

namespace TestWorkshop;

[DependsOn(
    typeof(TestWorkshopHttpApiModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(TestWorkshopApplicationModule),
    typeof(TestWorkshopEntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpSwashbuckleModule),

    typeof(AbpBlobStoringFileSystemModule),
    typeof(AbpAuthorizationOrganizationUnitsModule),
    typeof(AbpAspNetCoreMvcWrapperModule),


    typeof(AbpAspNetCoreSerilogModule)
    )]
public partial class TestWorkshopHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigureOpenIddict(configuration, hostingEnvironment);

    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureWrapper();

        ConfigureSecurity(configuration);

        ConfigureAuthentication(context);

        ConfigureUrls(configuration);

        ConfigureBundles();

        ConfigureHealthChecks(context);

        ConfigureCors(services, configuration);

        ConfigureLocalization(configuration);

        ConfigureCache(services, configuration, hostingEnvironment);

        ConfigureSwagger(services, configuration);

        ConfigureBlob(configuration);

    }



    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        var configuration = context.GetConfiguration();

        app.UseForwardedHeaders();

        app.UseAbpSecurityHeaders();

        app.UseCookiePolicy();

        app.UseTestWorkshopRequestLocalization();

        app.UseCorrelationId();

        app.MapAbpStaticAssets();

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();

        app.UseAbpOpenIddictValidation();

        app.UseDynamicClaims();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();

        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TestWorkshop API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var scopes = configuration.GetSection("AuthServer:Scopes").Get<string[]>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes(scopes);

        });

        app.UseAuditing();

        app.UseAbpSerilogEnrichers();

        app.UseConfiguredEndpoints();

    }
}
