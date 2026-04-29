namespace TestWorkshop.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class TestWorkshopDbContext : AbpDbContext<TestWorkshopDbContext>, ITenantManagementDbContext, IIdentityDbContext
{


    public DbSet<Device> Devices { get; set; }

    #region 内置相关表


    // Platform
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<UserMenu> UserMenus { get; set; }
    public DbSet<UserFavoriteMenu> UserFavoriteMenus { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Layout> Layouts { get; set; }
    public DbSet<Data> Datas { get; set; }
    public DbSet<DataItem> DataItems { get; set; }


    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }


    #endregion


    public TestWorkshopDbContext(DbContextOptions<TestWorkshopDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.ConfigureTestWorkshop();

    }
}
