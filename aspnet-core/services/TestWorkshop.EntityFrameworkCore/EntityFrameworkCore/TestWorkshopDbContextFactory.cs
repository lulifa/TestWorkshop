namespace TestWorkshop.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class TestWorkshopDbContextFactory : IDesignTimeDbContextFactory<TestWorkshopDbContext>
{
    public TestWorkshopDbContext CreateDbContext(string[] args)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        var configuration = BuildConfiguration();
        
        TestWorkshopEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<TestWorkshopDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));
        
        return new TestWorkshopDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TestWorkshop.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
