using System.Threading.Tasks;

namespace TestWorkshop.Data;

public interface ITestWorkshopDbSchemaMigrator
{
    Task MigrateAsync();
}
