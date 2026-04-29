namespace TestWorkshop;

public static class TestWorkshopPermissions
{
    public const string GroupName = "TestWorkshop";

    public static class DataDictionary
    {
        public const string Default = GroupName + ".DataDictionary";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Move = Default + ".Move";

        public const string Delete = Default + ".Delete";

        public const string ManageItems = Default + ".ManageItems";
    }

    public static class Layout
    {
        public const string Default = GroupName + ".Layout";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";
    }

    public static class Menu
    {
        public const string Default = GroupName + ".Menu";

        public const string Create = Default + ".Create";

        public const string Update = Default + ".Update";

        public const string Delete = Default + ".Delete";

        public const string ManageRoles = Default + ".ManageRoles";

        public const string ManageUsers = Default + ".ManageUsers";

        public const string ManageUserFavorites = Default + ".ManageUserFavorites";
    }


    public static class Users
    {
        public const string ResetPassword = IdentityPermissions.Users.Default + ".ResetPassword";
        public const string ManageOrganizationUnits = IdentityPermissions.Users.Default + ".ManageOrganizationUnits";
    }

    public static class Roles
    {
        public const string ManageOrganizationUnits = IdentityPermissions.Roles.Default + ".ManageOrganizationUnits";
    }

    public static class OrganizationUnits
    {
        public const string Default = IdentityPermissions.GroupName + ".OrganizationUnits";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManageUsers = Default + ".ManageUsers";
        public const string ManageRoles = Default + ".ManageRoles";
        public const string ManagePermissions = Default + ".ManagePermissions";
    }


    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(TestWorkshopPermissions));
    }
}
