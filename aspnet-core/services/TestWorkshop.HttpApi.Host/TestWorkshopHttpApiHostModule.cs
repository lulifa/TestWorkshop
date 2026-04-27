using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestWorkshop.AspNetCore.Mvc.Wrapper;
using TestWorkshop.Authorization.OrganizationUnits;
using TestWorkshop.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

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

        ConfigureSecurity(configuration);

        ConfigureWrapper();

        ConfigureAuthentication(context);

        ConfigureUrls(configuration);

        ConfigureBundles();

        ConfigureHealthChecks(context);

        ConfigureCors(services, configuration);

        ConfigureCors(services, configuration);

        ConfigureLocalization(configuration);

        ConfigureCache(services, configuration, hostingEnvironment);

        ConfigureSwagger(services, configuration);

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
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes(configuration["AuthServer:Scopes"]);
        });

        app.UseAuditing();

        app.UseAbpSerilogEnrichers();

        app.UseConfiguredEndpoints();

    }
}
