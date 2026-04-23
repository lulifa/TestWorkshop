using TestWorkshop.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TestWorkshop.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TestWorkshopEntityFrameworkCoreModule),
    typeof(TestWorkshopApplicationContractsModule)
)]
public class TestWorkshopDbMigratorModule : AbpModule
{
}
