<script lang="ts" setup>
import { computed } from 'vue';

import { AuthPageLayout } from '@vben/layouts';
import { preferences } from '@vben/preferences';

const appName = computed(() => preferences.app.name);
const logo = computed(() => preferences.logo.source);
const logoDark = computed(() => preferences.logo.sourceDark);
const pageDescription = computed(() =>
  preferences.app.locale === 'en-US'
    ? 'Unified operations for workshop orders, devices and telemetry'
    : '订单、设备与遥测数据的一体化车间运营平台',
);
const pageTitle = computed(() =>
  preferences.app.locale === 'en-US'
    ? 'TestWorkshop Operations Center'
    : 'TestWorkshop 测试车间运营中心',
);
</script>

<template>
  <AuthPageLayout
    :app-name="appName"
    :logo="logo"
    :logo-dark="logoDark"
    :page-description="pageDescription"
    :page-title="pageTitle"
  >
    <!-- 自定义工具栏 -->
    <!-- <template #toolbar></template> -->
  </AuthPageLayout>
</template>
