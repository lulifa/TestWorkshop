<script lang="ts" setup>
import { computed, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';

import { AuthenticationLoginExpiredModal } from '@vben/common-ui';
import { useWatermark } from '@vben/hooks';
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';

import { $t } from '#/locales';
import { useAuthStore, useNotificationStore } from '#/store';
import LoginForm from '#/views/_core/authentication/login.vue';

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const accessStore = useAccessStore();
const notificationStore = useNotificationStore();
const { destroyWatermark, updateWatermark } = useWatermark();

const menus = computed(() => [
  {
    handler: () => {
      router.push({ name: 'Profile' });
    },
    icon: 'lucide:user',
    text: $t('page.auth.profile'),
  },
]);

const avatar = computed(() => {
  return userStore.userInfo?.avatar ?? preferences.app.defaultAvatar;
});

const description = computed(() => userStore.userInfo?.email ?? '');

async function handleLogout() {
  await authStore.logout(false);
}

function handleNoticeClear() {
  void notificationStore.clear();
}

function handleViewAll() {
  router.push({
    path: '/business/notifications/my-message',
    query: { read: 'false' },
  });
}

function handleViewBroadcastAll() {
  router.push({
    path: '/business/notifications/my-broadcast',
    query: { read: 'false' },
  });
}

onMounted(() => {
  void notificationStore.refresh();
});

watch(
  () => ({
    enable: preferences.app.watermark,
    content: preferences.app.watermarkContent,
  }),
  async ({ enable, content }) => {
    if (enable) {
      await updateWatermark({
        content:
          content ||
          `${userStore.userInfo?.username} - ${userStore.userInfo?.realName}`,
      });
    } else {
      destroyWatermark();
    }
  },
  {
    immediate: true,
  },
);
</script>

<template>
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userStore.userInfo?.realName"
        :description
        tag-text="Pro"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        :broadcasts="notificationStore.broadcasts"
        :dot="notificationStore.showDot"
        :notifications="notificationStore.notifications"
        :removable="false"
        @clear="handleNoticeClear"
        @clear-broadcasts="notificationStore.clearBroadcasts"
        @read="(item) => item.id && notificationStore.markRead(item.id)"
        @read-broadcast="
          (item) => item.id && notificationStore.markBroadcastRead(item.id)
        "
        @remove-broadcast="
          (item) => item.id && notificationStore.removeBroadcast(item.id)
        "
        @view-broadcast-all="handleViewBroadcastAll"
        @view-all="handleViewAll"
      />
    </template>
    <template #extra>
      <AuthenticationLoginExpiredModal
        v-model:open="accessStore.loginExpired"
        :avatar
      >
        <LoginForm />
      </AuthenticationLoginExpiredModal>
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
</template>
