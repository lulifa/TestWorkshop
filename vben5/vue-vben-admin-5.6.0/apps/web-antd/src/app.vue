<script lang="ts" setup>
import { computed, onMounted } from 'vue';

import { useAntdDesignTokens } from '@vben/hooks';
import { preferences, usePreferences } from '@vben/preferences';

import { useEventBus, useNotification, useSignalR } from '@abp/core'; // ✅ 新增 useEventBus
import { App, ConfigProvider, theme } from 'ant-design-vue';

import { antdLocale } from '#/locales';

defineOptions({ name: 'App' });

const { isDark } = usePreferences();
const { tokens } = useAntdDesignTokens();

const tokenTheme = computed(() => {
  const algorithm = isDark.value
    ? [theme.darkAlgorithm]
    : [theme.defaultAlgorithm];

  // antd 紧凑模式算法
  if (preferences.app.compact) {
    algorithm.push(theme.compactAlgorithm);
  }

  return {
    algorithm,
    token: tokens,
  };
});

const signalR = useSignalR();
const { publish } = useEventBus();
const notification = useNotification();

onMounted(async () => {
  // 1. 初始化 SignalR 配置（但不自动连接！autoStart: false）
  await signalR.init({
    serverUrl: '/signalr/notification',
    automaticReconnect: true,
    autoStart: false, // 👈 关键：不自动连接，等登录后再启动
    useAccessToken: true,
  });

  // 接收普通文本消息（私信）
  signalR.on('ReceiveTextMessageAsync', (message: any) => {
    publish('signalR:ReceiveTextMessage', message);
  });

  // 接收广播消息（通告）
  signalR.on('ReceiveBroadCastMessageAsync', (message: any) => {
    publish('signalR:ReceiveBroadCastMessage', message);
  });

  notification.register();
});
</script>

<template>
  <ConfigProvider :locale="antdLocale" :theme="tokenTheme">
    <App>
      <RouterView />
    </App>
  </ConfigProvider>
</template>
