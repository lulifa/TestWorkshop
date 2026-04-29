namespace TestWorkshop;

public class TestWorkshopPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var platform = context.AddGroup(TestWorkshopPermissions.GroupName, L("Permission:Platform"));

        var dataDictionary = platform.AddPermission(TestWorkshopPermissions.DataDictionary.Default, L("Permission:DataDictionary"));
        dataDictionary.AddChild(TestWorkshopPermissions.DataDictionary.Create, L("Permission:Create"));
        dataDictionary.AddChild(TestWorkshopPermissions.DataDictionary.Update, L("Permission:Update"));
        dataDictionary.AddChild(TestWorkshopPermissions.DataDictionary.Move, L("Permission:Move"));
        dataDictionary.AddChild(TestWorkshopPermissions.DataDictionary.Delete, L("Permission:Delete"));
        dataDictionary.AddChild(TestWorkshopPermissions.DataDictionary.ManageItems, L("Permission:ManageItems"));

        var layout = platform.AddPermission(TestWorkshopPermissions.Layout.Default, L("Permission:Layout"));
        layout.AddChild(TestWorkshopPermissions.Layout.Create, L("Permission:Create"));
        layout.AddChild(TestWorkshopPermissions.Layout.Update, L("Permission:Update"));
        layout.AddChild(TestWorkshopPermissions.Layout.Delete, L("Permission:Delete"));

        var menu = platform.AddPermission(TestWorkshopPermissions.Menu.Default, L("Permission:Menu"));
        menu.AddChild(TestWorkshopPermissions.Menu.Create, L("Permission:Create"));
        menu.AddChild(TestWorkshopPermissions.Menu.Update, L("Permission:Update"));
        menu.AddChild(TestWorkshopPermissions.Menu.Delete, L("Permission:Delete"));
        menu.AddChild(TestWorkshopPermissions.Menu.ManageRoles, L("Permission:ManageRoleMenus"));
        menu.AddChild(TestWorkshopPermissions.Menu.ManageUsers, L("Permission:ManageUserMenus"));
        menu.AddChild(TestWorkshopPermissions.Menu.ManageUserFavorites, L("Permission:ManageUserFavoriteMenus"));


        var identityGroup = context.GetGroupOrNull(IdentityPermissions.GroupName);
        if (identityGroup != null)
        {
            var userPermission = identityGroup.GetPermissionOrNull(IdentityPermissions.Users.Default);
            if (userPermission != null)
            {
                userPermission.AddChild(TestWorkshopPermissions.Users.ResetPassword, IdentityL("Permission:ResetPassword"));
                userPermission.AddChild(TestWorkshopPermissions.Users.ManageOrganizationUnits, IdentityL("Permission:ManageOrganizationUnits"));
            }

            var rolePermission = identityGroup.GetPermissionOrNull(IdentityPermissions.Roles.Default);
            if (rolePermission != null)
            {
                rolePermission.AddChild(TestWorkshopPermissions.Roles.ManageOrganizationUnits, IdentityL("Permission:ManageOrganizationUnits"));
            }

            var origanizationUnitPermission = identityGroup.AddPermission(TestWorkshopPermissions.OrganizationUnits.Default, IdentityL("Permission:OrganizationUnitManagement"));
            origanizationUnitPermission.AddChild(TestWorkshopPermissions.OrganizationUnits.Create, IdentityL("Permission:Create"));
            origanizationUnitPermission.AddChild(TestWorkshopPermissions.OrganizationUnits.Update, IdentityL("Permission:Edit"));
            origanizationUnitPermission.AddChild(TestWorkshopPermissions.OrganizationUnits.Delete, IdentityL("Permission:Delete"));
            origanizationUnitPermission.AddChild(TestWorkshopPermissions.OrganizationUnits.ManageRoles, IdentityL("Permission:ManageRoles"));
            origanizationUnitPermission.AddChild(TestWorkshopPermissions.OrganizationUnits.ManageUsers, IdentityL("Permission:ManageUsers"));
            origanizationUnitPermission.AddChild(TestWorkshopPermissions.OrganizationUnits.ManagePermissions, IdentityL("Permission:ChangePermissions"));

        }

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TestWorkshopResource>(name);
    }

    private static LocalizableString IdentityL(string name)
    {
        return LocalizableString.Create<IdentityResource>(name);
    }

}
