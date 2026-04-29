namespace TestWorkshop.EntityFrameworkCore;

public class EntityFrameworkCoreTestWorkshopDbSchemaMigrator
    : ITestWorkshopDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreTestWorkshopDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        await _serviceProvider
            .GetRequiredService<TestWorkshopDbContext>()
            .Database
            .MigrateAsync();
    }
}
