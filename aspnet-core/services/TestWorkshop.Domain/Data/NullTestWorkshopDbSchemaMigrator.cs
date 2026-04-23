using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TestWorkshop.Data;

/* This is used if database provider does't define
 * ITestWorkshopDbSchemaMigrator implementation.
 */
public class NullTestWorkshopDbSchemaMigrator : ITestWorkshopDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
