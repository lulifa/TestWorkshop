namespace TestWorkshop;

/* Creates initial data that is needed to property run the application
 * and make client-to-server communication possible.
 */
public class OpenIddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public static HashSet<string> InitializeScopes = new HashSet<string>
    {
        "ruichenshuxin-abppro-application"
    };

    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictApplicationRepository _applicationRepository;

    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IOpenIddictScopeRepository _scopeRepository;

    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;

    public OpenIddictDataSeedContributor(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictApplicationRepository applicationRepository,
        IOpenIddictScopeManager scopeManager,
        IOpenIddictScopeRepository scopeRepository,
        IPermissionDataSeeder permissionDataSeeder,
        IConfiguration configuration,
        ICurrentTenant currentTenant)
    {
        _applicationManager = applicationManager;
        _applicationRepository = applicationRepository;
        _scopeManager = scopeManager;
        _scopeRepository = scopeRepository;
        _permissionDataSeeder = permissionDataSeeder;
        _configuration = configuration;
        _currentTenant = currentTenant;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context.TenantId))
        {
            await CreateScopeAsync(InitializeScopes);
            await CreateApplicationAsync(InitializeScopes);
        }
    }

    #region OpenIddict

    private async Task CreateScopeAsync(IEnumerable<string> scopes)
    {
        foreach (var scope in scopes)
        {
            if (await _scopeRepository.FindByNameAsync(scope) == null)
            {
                await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor()
                {
                    Name = scope,
                    DisplayName = scope + " access",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("zh-Hans")] = "Abp API 应用程序访问",
                        [CultureInfo.GetCultureInfo("en")] = "Abp API Application Access"
                    },
                    Resources =
                    {
                        scope
                    }
                });
            }
        }
    }

    private async Task CreateApplicationAsync(IEnumerable<string> scopes)
    {
        var configurationSection = _configuration.GetSection("OpenIddict:Applications");

        var vueClientId = configurationSection["AbpPro_App:ClientId"];
        if (!vueClientId.IsNullOrWhiteSpace())
        {
            var vueClientRootUrl = configurationSection["AbpPro_App:RootUrl"].EnsureEndsWith('/');

            if (await _applicationRepository.FindByClientIdAsync(vueClientId) == null)
            {
                var application = new AbpApplicationDescriptor
                {
                    ClientId = vueClientId,
                    ClientSecret = configurationSection["AbpPro_App:ClientSecret"],
                    ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                    ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                    DisplayName = "Abp Vue Admin Client",
                    ClientUri = vueClientRootUrl,
                    LogoUri = "/images/clients/vue.svg",
                    PostLogoutRedirectUris =
                    {
                        new Uri(vueClientRootUrl + "signout-callback"),
                        new Uri(vueClientRootUrl)
                    },
                    RedirectUris =
                    {
                        new Uri(vueClientRootUrl + "signin-callback"),
                        new Uri(vueClientRootUrl)
                    },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization,
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.Endpoints.EndSession,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.Implicit,
                        OpenIddictConstants.Permissions.GrantTypes.Password,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.GrantTypes.DeviceCode,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,

                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.None,
                        OpenIddictConstants.Permissions.ResponseTypes.Token,

                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                    }
                };

                foreach (var scope in scopes)
                {
                    application.Permissions.AddIfNotContains(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
                }

                await _applicationManager.CreateAsync(application);

                var vueClientPermissions = new string[2]
                {
                    "AbpIdentity.UserLookup","AbpIdentity.Users"
                };
                await _permissionDataSeeder.SeedAsync(ClientPermissionValueProvider.ProviderName, vueClientId, vueClientPermissions);
            }
        }

        var swaggerClientId = configurationSection["AbpPro_Swagger:ClientId"];
        if (!swaggerClientId.IsNullOrWhiteSpace())
        {
            var swaggerClientRootUrl = configurationSection["AbpPro_Swagger:RootUrl"].EnsureEndsWith('/');

            if (await _applicationRepository.FindByClientIdAsync(swaggerClientId) == null)
            {
                var application = new AbpApplicationDescriptor
                {
                    ClientId = swaggerClientId,
                    ClientSecret = null,
                    ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                    DisplayName = "Swagger Client",
                    ClientUri = swaggerClientRootUrl.EnsureEndsWith('/') + "swagger",
                    LogoUri = "/images/clients/swagger.svg",
                    PostLogoutRedirectUris = { },
                    RedirectUris = { },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization,
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.Endpoints.EndSession,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.None,
                        OpenIddictConstants.Permissions.ResponseTypes.Token,

                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                    }
                };

                foreach (var scope in scopes)
                {
                    application.Permissions.AddIfNotContains(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
                }

                application.PostLogoutRedirectUris.AddIfNotContains(new Uri(swaggerClientRootUrl.EnsureEndsWith('/')));
                application.PostLogoutRedirectUris.AddIfNotContains(new Uri(swaggerClientRootUrl.EnsureEndsWith('/') + "signout-callback"));

                application.RedirectUris.AddIfNotContains(new Uri(swaggerClientRootUrl));
                application.RedirectUris.AddIfNotContains(new Uri(swaggerClientRootUrl.EnsureEndsWith('/') + "signin-callback"));
                application.RedirectUris.AddIfNotContains(new Uri(swaggerClientRootUrl.EnsureEndsWith('/') + "swagger/oauth2-redirect.html"));

                await _applicationManager.CreateAsync(application);

                var swaggerClientPermissions = new string[2]
                {
                    "AbpIdentity.UserLookup","AbpIdentity.Users"
                };
                await _permissionDataSeeder.SeedAsync(ClientPermissionValueProvider.ProviderName, swaggerClientId, swaggerClientPermissions);
            }
        }

        var oauthClientId = configurationSection["AbpPro_OAuth:ClientId"];
        if (!oauthClientId.IsNullOrWhiteSpace())
        {
            var oauthClientRootUrl = configurationSection["AbpPro_OAuth:RootUrl"].EnsureEndsWith('/');

            if (await _applicationRepository.FindByClientIdAsync(oauthClientId) == null)
            {
                var application = new AbpApplicationDescriptor
                {
                    ClientId = oauthClientId,
                    ClientSecret = null,
                    ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                    DisplayName = "OAuth Client",
                    ClientUri = oauthClientRootUrl,
                    LogoUri = "/images/clients/aspnetcore.svg",
                    PostLogoutRedirectUris = { },
                    RedirectUris = { },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization,
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.Endpoints.EndSession,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.CodeToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.None,
                        OpenIddictConstants.Permissions.ResponseTypes.Token,

                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Address,
                        OpenIddictConstants.Permissions.Scopes.Phone,
                    }
                };

                foreach (var scope in scopes)
                {
                    application.Permissions.AddIfNotContains(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
                }

                application.PostLogoutRedirectUris.AddIfNotContains(new Uri(oauthClientRootUrl.EnsureEndsWith('/')));
                application.PostLogoutRedirectUris.AddIfNotContains(new Uri(oauthClientRootUrl.EnsureEndsWith('/') + "signout-callback"));

                application.RedirectUris.AddIfNotContains(new Uri(oauthClientRootUrl));
                application.RedirectUris.AddIfNotContains(new Uri(oauthClientRootUrl.EnsureEndsWith('/') + "signin-callback"));
                application.RedirectUris.AddIfNotContains(new Uri(oauthClientRootUrl.EnsureEndsWith('/') + "swagger/oauth2-redirect.html"));

                await _applicationManager.CreateAsync(application);

                var oauthClientPermissions = new string[2]
                {
                    "AbpIdentity.UserLookup","AbpIdentity.Users"
                };
                await _permissionDataSeeder.SeedAsync(ClientPermissionValueProvider.ProviderName, oauthClientId, oauthClientPermissions);
            }
        }
    }

    #endregion

}

