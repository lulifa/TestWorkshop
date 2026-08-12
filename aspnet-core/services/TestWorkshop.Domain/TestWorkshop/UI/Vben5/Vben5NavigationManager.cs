using System;

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
              name: "Vben5BusinessWorkDevices",
              displayName: "车间设备管理",
              url: "/business/workshopdevices",
              component: "/business/workshopdevices/index",
              icon: "arcticons:tenantcloud-pro",
              description: "车间设备管理")
            .SetProperty("title", "page.business.workshopdevices"));
        business.AddItem(
          new ApplicationMenu(
              name: "Vben5BusinessMyBroadcast",
              displayName: "我的通告",
              url: "/business/notifications/my-broadcast",
              component: "/business/notifications/my-broadcast/index",
              icon: "lucide:megaphone",
              description: "我的通告")
            .SetProperty("title", "page.business.notifications.broadcast"));
        business.AddItem(
          new ApplicationMenu(
              name: "Vben5BusinessMyMessage",
              displayName: "我的消息",
              url: "/business/notifications/my-message",
              component: "/business/notifications/my-message/index",
              icon: "lucide:mail-open",
              description: "我的消息")
            .SetProperty("title", "page.business.notifications.message"));

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
        modules.AddItem(
          new ApplicationMenu(
              name: "Vben5ModulesPlatformFiles",
              displayName: "文件管理",
              url: "/modules/platform/files",
              component: "/modules/platform/files/index",
              icon: "mdi-light:file",
              description: "文件管理")
            .SetProperty("title", "abp.modules.platform.files"));
        modules.AddItem(
          new ApplicationMenu(
              name: "Vben5ModulesPlatformBroadcast",
              displayName: "通告管理",
              url: "/modules/platform/notifications/broadcast",
              component: "/modules/platform/notifications/broadcast/index",
              icon: "lucide:megaphone",
              description: "通告管理")
            .SetProperty("title", "abp.modules.platform.notifications.broadcast"));
        modules.AddItem(
          new ApplicationMenu(
              name: "Vben5ModulesPlatformMessage",
              displayName: "消息管理",
              url: "/modules/platform/notifications/message",
              component: "/modules/platform/notifications/message/index",
              icon: "lucide:mail-open",
              description: "消息管理")
            .SetProperty("title", "abp.modules.platform.notifications.message"));

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
              name: "Vben5SystemSecurityLogs",
              displayName: "安全日志",
              url: "/system/identity/securitylogs",
              component: "/system/identity/securitylogs/index",
              icon: "carbon:security",
              description: "安全日志",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.system.securitylogs"));

        system.AddItem(new ApplicationMenu(
               name: "Vben5SystemAuditLogs",
               displayName: "审计日志",
               url: "/system/identity/auditlogs",
               component: "/system/identity/auditlogs/index",
               icon: "fluent-mdl2:compliance-audit",
               description: "审计日志")
            .SetProperty("title", "abp.system.auditlogs"));

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

        return
        [
            new NavigationDefinition(system),
        ];
    }


}
