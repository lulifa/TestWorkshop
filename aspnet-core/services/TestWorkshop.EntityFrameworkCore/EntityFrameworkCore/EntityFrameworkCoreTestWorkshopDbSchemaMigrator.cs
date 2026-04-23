using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestWorkshop.Data;
using Volo.Abp.DependencyInjection;

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
        /* We intentionally resolving the TestWorkshopDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<TestWorkshopDbContext>()
            .Database
            .MigrateAsync();
    }
}
