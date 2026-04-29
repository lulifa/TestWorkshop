namespace TestWorkshop.EntityFrameworkCore;

[DependsOn(
    typeof(TestWorkshopDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class TestWorkshopEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        TestWorkshopEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TestWorkshopDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);

            options.AddRepository<Menu, EfCoreMenuRepository>();
            options.AddRepository<UserMenu, EfCoreUserMenuRepository>();
            options.AddRepository<RoleMenu, EfCoreRoleMenuRepository>();
            options.AddRepository<UserFavoriteMenu, EfCoreUserFavoriteMenuRepository>();
            options.AddRepository<Layout, EfCoreLayoutRepository>();
            options.AddRepository<Data, EfCoreDataRepository>();

            options.AddRepository<IdentityRole, EfCoreIdentityRoleRepository>();
            options.AddRepository<IdentityUser, EfCoreIdentityUserRepository>();
            options.AddRepository<OrganizationUnit, EfCoreOrganizationUnitRepository>();


        });

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also TestWorkshopDbContextFactory for EF Core tooling. */

            options.UseNpgsql();

        });



        context.Services.AddSingleton<TelemetryChannel>();
        context.Services.AddHostedService<TelemetryBackgroundService>();


    }
}
