using TestWorkshop.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace TestWorkshop.Permissions;

public class TestWorkshopPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TestWorkshopPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(TestWorkshopPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TestWorkshopResource>(name);
    }
}
