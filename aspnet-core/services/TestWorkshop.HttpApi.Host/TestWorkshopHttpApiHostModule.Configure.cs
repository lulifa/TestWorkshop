using Autofac.Core;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using TestWorkshop.HealthChecks;
using TestWorkshop.Wrapper;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;

namespace TestWorkshop
{
    public partial class TestWorkshopHttpApiHostModule
    {

        private void PreConfigureOpenIddict(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var authority = configuration["AuthServer:Authority"];
            var scopes = configuration.GetSection("AuthServer:Scopes").Get<string[]>();
            var certificatePassPhrase = configuration["AuthServer:CertificatePassPhrase"];

            PreConfigure<OpenIddictBuilder>(builder =>
            {
                builder.AddValidation(options =>
                {
                    options.AddAudiences(scopes);
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
            });

            if (!environment.IsDevelopment())
            {
                PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
                {
                    options.AddDevelopmentEncryptionAndSigningCertificate = false;
                });

                PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
                {
                    serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", certificatePassPhrase);
                    serverBuilder.SetIssuer(new Uri(authority));
                });
            }
        }

        private void ConfigureSecurity(IConfiguration configuration)
        {
            if (!configuration.GetValue<bool>("App:DisablePII"))
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
            }

            if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
            {
                Configure<OpenIddictServerAspNetCoreOptions>(options =>
                {
                    options.DisableTransportSecurityRequirement = true;
                });

                Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                });
            }
        }

        private void ConfigureWrapper()
        {
            Configure<AbpWrapperOptions>(options =>
            {
                options.IsEnabled = true;
            });
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context)
        {
            context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
            {
                options.IsDynamicClaimsEnabled = true;
            });

            // CSRF/XSRF https://abp.io/docs/latest/framework/infrastructure/csrf-anti-forgery
            context.Services.Configure<AbpAntiForgeryOptions>(options =>
            {
                options.AutoValidate = true;
            });

            context.Services.AddSameSiteCookiePolicy();

        }

        private void ConfigureUrls(IConfiguration configuration)
        {
            Configure<AppUrlOptions>(options =>
            {
                options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
                options.Applications["Vue"].RootUrl = configuration["App:VueUrl"];
                options.Applications["Vue"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
                options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
            });
        }

        private void ConfigureBundles()
        {
            Configure<AbpBundlingOptions>(options =>
            {
                options.StyleBundles.Configure(
                    LeptonXLiteThemeBundles.Styles.Global,
                    bundle =>
                    {
                        bundle.AddFiles("/global-styles.css");
                    }
                );

                options.ScriptBundles.Configure(
                    LeptonXLiteThemeBundles.Scripts.Global,
                    bundle =>
                    {
                        bundle.AddFiles("/global-scripts.js");
                    }
                );
            });
        }

        private void ConfigureHealthChecks(ServiceConfigurationContext context)
        {
            context.Services.AddTestWorkshopHealthChecks();
        }

        private void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]?
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.Trim().RemovePostFix("/"))
                                .ToArray() ?? Array.Empty<string>()
                        )
                        .WithAbpExposedHeaders()
                        .WithAbpWrapExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }


        private void ConfigureLocalization(IConfiguration configuration)
        {

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("en", "en", "English"));
                options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));

            });

        }

        private void ConfigureCache(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {

            var isRedisEnabled = configuration.GetValue<bool>("Redis:IsEnabled");
            var redisConfiguration = configuration.GetValue<string>("Redis:Configuration");
            var redisInstanceName = configuration.GetValue<string>("Redis:InstanceName");

            Configure<AbpDistributedCacheOptions>(options =>
            {
                options.KeyPrefix = "TestWorkshop:";
            });

            var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("TestWorkshop");

            if (isRedisEnabled)
            {
                Configure<RedisCacheOptions>(options =>
                {
                    options.Configuration = redisConfiguration;
                    options.InstanceName = redisInstanceName;
                });

                var redis = ConnectionMultiplexer.Connect(redisConfiguration);

                services.AddSingleton<IDistributedLockProvider>(sp =>
                {
                    return new RedisDistributedSynchronizationProvider(redis.GetDatabase());
                });

                if (!environment.IsDevelopment())
                {
                    dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, $"TestWorkshop-Protection-Keys");
                }
            }

        }

        private void ConfigureSwagger(IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration["AuthServer:Authority"];
            var scopes = configuration.GetSection("AuthServer:Scopes").Get<string[]>();

            services.AddAbpSwaggerGenWithOidc(
                authority,
                scopes,
                [AbpSwaggerOidcFlows.AuthorizationCode],
                null,
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "TestWorkshop API",
                        Version = "v1"
                    });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);

                    options.DocumentFilter<AbpHideDefaultApiFilter>();
                    options.OperationFilter<AbpOperationFilter>();

                    // 自动扫描所有 XML 注释
                    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
                    foreach (var xmlPath in xmlFiles)
                    {
                        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                    }

                });
        }

    }
}
