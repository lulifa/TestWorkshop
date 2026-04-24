namespace TestWorkshop;

public class Vben5NavigationManager : IVben5NavigationManager, ISingletonDependency
{
    public Vben5NavigationManager()
    {

    }

    public virtual Task<IReadOnlyCollection<ApplicationMenu>> GetAll()
    {
        var navigations = new List<ApplicationMenu>();

        var navigationDefineitions = new List<NavigationDefinition>();

        navigationDefineitions.AddRange(GetVbenBusiness());
        navigationDefineitions.AddRange(GetDashboard());
        navigationDefineitions.AddRange(GetModules());
        navigationDefineitions.AddRange(GetSystem());

        foreach (var navigationDefineition in navigationDefineitions)
        {
            navigations.Add(navigationDefineition.Menu);
        }

        IReadOnlyCollection<ApplicationMenu> menus = navigations.OrderBy(item => item.Order).ToImmutableList();

        return Task.FromResult(menus);
    }

    private static NavigationDefinition[] GetVbenBusiness()
    {
        var business = new ApplicationMenu(
            name: "Vben5Business",
            displayName: "业务管理",
            url: "/business",
            component: "",
            description: "业务管理",
            icon: "arcticons:activity-manager",
            order: 1)
            .SetProperty("title", "page.business.title");
        business.AddItem(
          new ApplicationMenu(
              name: "Vben5BusinessBooks",
              displayName: "书籍管理",
              url: "/business/books",
              component: "/business/books/index",
              icon: "arcticons:tenantcloud-pro",
              description: "书籍管理")
            .SetProperty("title", "page.business.books"));

        return
        [
            new NavigationDefinition(business),
        ];
    }

    private static NavigationDefinition[] GetDashboard()
    {
        var dashboard = new ApplicationMenu(
            name: "Vben5Dashboard",
            displayName: "仪表盘",
            url: "/dashboard",
            component: "",
            description: "仪表盘",
            icon: "lucide:layout-dashboard",
            order: -1)
            .SetProperty("title", "page.dashboard.title");

        dashboard.AddItem(
           new ApplicationMenu(
               name: "Vben5Workbench",
               displayName: "工作台",
               url: "/workspace",
               component: "/dashboard/workspace/index",
               icon: "carbon:workspace",
               description: "工作台")
           .SetProperty("affixTab", "true")
           .SetProperty("title", "page.dashboard.workspace")
        );

        dashboard.AddItem(
            new ApplicationMenu(
                name: "Vben5Analysis",
                displayName: "分析页",
                url: "/analytics",
                component: "/dashboard/analytics/index",
                icon: "lucide:area-chart",
                description: "分析页")
            .SetProperty("title", "page.dashboard.analytics")
         );

        var about = new ApplicationMenu(
            name: "VbenAbout",
            displayName: "关于",
            url: "/vben-admin/about",
            component: "/_core/about/index",
            description: "关于",
            order: 9999,
            icon: "lucide:copyright")
            .SetProperty("title", "demos.vben.about");

        return
        [
            new NavigationDefinition(dashboard),
            new NavigationDefinition(about),
        ];
    }

    private static NavigationDefinition[] GetModules()
    {
        var modules = new ApplicationMenu(
            name: "Vben5Modules",
            displayName: "平台管理",
            url: "/modules",
            component: "",
            description: "平台管理",
            icon: "ep:platform",
            order: 2)
            .SetProperty("title", "abp.modules.title");
        modules.AddItem(
          new ApplicationMenu(
              name: "Vben5ModulesPlatformMenus",
              displayName: "菜单管理",
              url: "/modules/platform/menus",
              component: "/modules/platform/menus/index",
              icon: "material-symbols-light:menu",
              description: "菜单管理")
            .SetProperty("title", "abp.modules.platform.menus"));
        modules.AddItem(
          new ApplicationMenu(
              name: "Vben5ModulesPlatformLayouts",
              displayName: "布局管理",
              url: "/modules/platform/layouts",
              component: "/modules/platform/layouts/index",
              icon: "material-symbols-light:responsive-layout",
              description: "布局管理")
            .SetProperty("title", "abp.modules.platform.layouts"));
        modules.AddItem(
          new ApplicationMenu(
              name: "Vben5ModulesPlatformDataDictionaries",
              displayName: "数据字典",
              url: "/modules/platform/data-dictionaries",
              component: "/modules/platform/data-dictionaries/index",
              icon: "material-symbols:dictionary-outline",
              description: "数据字典")
            .SetProperty("title", "abp.modules.platform.dataDictionaries"));

        return
        [
            new NavigationDefinition(modules),
        ];
    }

    private static NavigationDefinition[] GetSystem()
    {
        var system = new ApplicationMenu(
            name: "Vben5System",
            displayName: "系统管理",
            url: "/system",
            component: "",
            description: "系统管理",
            icon: "arcticons:activity-manager",
            order: 3,
            multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.system.title");
        system.AddItem(
          new ApplicationMenu(
              name: "Vben5SystemTenants",
              displayName: "租户管理",
              url: "/system/tenants",
              component: "/system/tenants/index",
              icon: "arcticons:tenantcloud-pro",
              description: "租户管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.system.tenants"));
        system.AddItem(
          new ApplicationMenu(
              name: "Vben5SystemUsers",
              displayName: "用户管理",
              url: "/system/identity/users",
              component: "/system/identity/users/index",
              icon: "mdi:user-outline",
              description: "用户管理")
          .SetProperty("title", "abp.system.identity.users"));
        system.AddItem(
          new ApplicationMenu(
              name: "Vben5SystemRoles",
              displayName: "角色管理",
              url: "/system/identity/roles",
              component: "/system/identity/roles/index",
              icon: "carbon:user-role",
              description: "角色管理")
          .SetProperty("title", "abp.system.identity.roles"));
        system.AddItem(
          new ApplicationMenu(
              name: "Vben5SystemOrganizationUnits",
              displayName: "组织机构",
              url: "/system/identity/organization-units",
              component: "/system/identity/organization-units/index",
              icon: "clarity:organization-line",
              description: "组织机构")
          .SetProperty("title", "abp.system.identity.organizationUnits"));

        var dataProtection = system.AddItem(new ApplicationMenu(
              name: "Vben5SystemDataProtection",
              displayName: "数据保护",
              url: "/system/data-protection",
              component: "",
              description: "数据保护",
              icon: "icon-park-outline:protect",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.system.dataProtection.title"));
        dataProtection.AddItem(new ApplicationMenu(
               name: "Vben5SystemDataProtectionEntityTypeInfos",
               displayName: "实体管理",
               url: "/system/data-protection/entity-type-infos",
               component: "/system/data-protection/entity-type-infos/index",
               icon: "iconamoon:type",
               description: "实体管理",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.system.dataProtection.entityTypeInfos"));

        return
        [
            new NavigationDefinition(system),
        ];
    }


}
