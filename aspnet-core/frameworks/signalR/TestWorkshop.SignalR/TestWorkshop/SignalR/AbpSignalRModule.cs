namespace TestWorkshop.SignalR;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSignalRModule))]
public class AbpSignalRModule : AbpModule
{
}
