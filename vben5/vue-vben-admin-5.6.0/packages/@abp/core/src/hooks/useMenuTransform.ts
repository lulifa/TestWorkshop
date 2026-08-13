import type { RouteRecordStringComponent } from '@vben/types';

import type { MenuDto } from '../types';

import { useUserStore } from '@vben/stores';

import { listToTree } from '@abp/core';

export function useMenuTransform() {
  const userStore = useUserStore();
  function mapMetaString(meta: Record<string, any>, key: string) {
    if (!meta[key]) {
      return undefined;
    }
    return typeof meta[key] === 'string' ? meta[key] : String(meta[key]);
  }
  function mapMetaNumber(meta: Record<string, any>, key: string) {
    if (!meta[key]) {
      return undefined;
    }
    return typeof meta[key] === 'number' ? meta[key] : Number(meta[key]);
  }
  function mapMetaBoolean(meta: Record<string, any>, key: string) {
    if (!meta[key]) {
      return undefined;
    }
    return typeof meta[key] === 'boolean' ? meta[key] : meta[key] === 'true';
  }
  function mapMetaArray(meta: Record<string, any>, key: string) {
    if (!meta[key]) {
      return undefined;
    }
    return Array.isArray(meta[key]) ? meta[key] : String(meta[key]).split(',');
  }
  function transformRoutes(menus: MenuDto[]): RouteRecordStringComponent[] {
    const startupMenus = menus.filter((x) => x.startup);
    if (startupMenus.length > 0) {
      userStore.$patch((state) => {
        state.userInfo && (state.userInfo.homePath = startupMenus[0]?.path);
      });
    } else {
      userStore.$patch((state) => {
        state.userInfo && (state.userInfo.homePath = undefined);
      });
    }
    const combMenus = menus.map((item) => {
      return {
        component: item.component.includes('BasicLayout')
          ? undefined
          : item.component,
        id: item.id,
        meta: {
          title: item.meta.title ?? item.displayName,
          icon: mapMetaString(item.meta, 'icon'),
          activeIcon: mapMetaString(item.meta, 'activeIcon'),
          keepAlive: mapMetaBoolean(item.meta, 'keepAlive'),
          hideInMenu: mapMetaBoolean(item.meta, 'hideInMenu'),
          hideInTab: mapMetaBoolean(item.meta, 'hideInTab'),
          hideInBreadcrumb: mapMetaBoolean(item.meta, 'hideInBreadcrumb'),
          hideChildrenInMenu: mapMetaBoolean(item.meta, 'hideChildrenInMenu'),
          authority: mapMetaArray(item.meta, 'authority'),
          badge: mapMetaString(item.meta, 'badge'),
          badgeType: mapMetaString(item.meta, 'badgeType'),
          badgeVariants: mapMetaString(item.meta, 'badgeVariants'),
          activePath: mapMetaString(item.meta, 'activePath'),
          affixTab: mapMetaBoolean(item.meta, 'affixTab'),
          affixTabOrder: mapMetaNumber(item.meta, 'affixTabOrder'),
          iframeSrc: mapMetaString(item.meta, 'iframeSrc'),
          ignoreAccess: mapMetaBoolean(item.meta, 'ignoreAccess'),
          link: mapMetaString(item.meta, 'link'),
          maxNumOfOpenTab: mapMetaNumber(item.meta, 'maxNumOfOpenTab'),
          menuVisibleWithForbidden: mapMetaBoolean(
            item.meta,
            'menuVisibleWithForbidden',
          ),
          openInNewWindow: mapMetaBoolean(item.meta, 'openInNewWindow'),
          order: mapMetaNumber(item.meta, 'order'),
          noBasicLayout: mapMetaBoolean(item.meta, 'noBasicLayout'),
        },
        name: item.name,
        parentId: item.parentId,
        path: item.path,
        redirect: item.redirect,
      };
    });
    const routes = listToTree(combMenus, {
      id: 'id',
      pid: 'parentId',
      children: 'children',
    });

    return routes;
  }

  return {
    transformRoutes,
  };
}
