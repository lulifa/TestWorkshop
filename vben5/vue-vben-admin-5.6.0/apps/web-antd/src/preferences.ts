import { defineOverridesPreferences } from '@vben/preferences';

/**
 * @description 项目配置文件
 * 只需要覆盖项目中的一部分配置，不需要的配置不用覆盖，会自动使用默认配置
 * !!! 更改配置后请清空缓存，否则可能不生效
 */
export const overridesPreferences = defineOverridesPreferences({
  // overrides
  app: {
    defaultAvatar: '/avatar/default-avatar.png',
    name: import.meta.env.VITE_APP_TITLE,
    accessMode: 'backend',
    enableRefreshToken: true,
    defaultHomePath: '/workspace',
    enablePreferences: false,
  },
  copyright: {
    companyName: 'TestWorkshop',
    date: '2026',
  },
  header: {
    height: 56,
  },
  logo: {
    source: '/logo-testworkshop.svg',
    sourceDark: '/logo-testworkshop.svg',
  },
  sidebar: {
    width: 232,
  },
  tabbar: {
    height: 40,
  },
  theme: {
    builtinType: 'deep-blue',
    colorPrimary: 'hsl(214 92% 52%)',
    mode: 'light',
    radius: '0.5',
    semiDarkSidebar: false,
  },
  widget: {
    languageToggle: false,
    notification: true,
    themeToggle: false,
    timezone: false,
    fullscreen: false,
  },
});
