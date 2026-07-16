<script setup lang="ts">
import type { WorkbenchTodoItem, WorkbenchTrendItem } from '@vben/common-ui';

import type { FavoriteMenu } from '../types';

import { computed, defineAsyncComponent, onMounted, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { useAppConfig } from '@vben/hooks';
import { $t } from '@vben/locales';
import { preferences } from '@vben/preferences';
import { useUserStore } from '@vben/stores';

import { useMyFavoriteMenusApi } from '@abp/core';
// import {
//   useMyNotifilersApi,
//   useNotificationSerializer,
// } from '@abp/notifications';
import { Empty, message } from 'ant-design-vue';

import WorkbenchHeader from './WorkbenchHeader.vue';
import WorkbenchQuickNav from './WorkbenchQuickNav.vue';
import WorkbenchTodo from './WorkbenchTodo.vue';
import WorkbenchTrends from './WorkbenchTrends.vue';

defineEmits<{
  (event: 'navTo', menu: FavoriteMenu): void;
}>();

const userStore = useUserStore();
// const { getMyNotifilersApi } = useMyNotifilersApi();
const { getListApi: getFavoriteMenusApi, deleteApi: deleteFavoriteMenuApi } =
  useMyFavoriteMenusApi();
// const { deserialize } = useNotificationSerializer();
const { uiFramework } = useAppConfig(import.meta.env, import.meta.env.PROD);

const defaultMenus: FavoriteMenu[] = [
  // {
  //   id: '1',
  //   color: '#1fdaca',
  //   icon: 'ion:home-outline',
  //   displayName: $t('workbench.content.favoriteMenu.home'),
  //   path: '/',
  //   isDefault: true,
  // },
  // {
  //   id: '2',
  //   color: '#bf0c2c',
  //   icon: 'ion:grid-outline',
  //   displayName: $t('workbench.content.favoriteMenu.dashboard'),
  //   path: '/dashboard',
  //   isDefault: true,
  // },
];
const unReadNotifilerCount = ref(0);
const unReadNotifilers = ref<WorkbenchTrendItem[]>([]);
const favoriteMenus = ref<FavoriteMenu[]>([]);
const todoList = ref<WorkbenchTodoItem[]>([]);

const getFavoriteMenus = computed(() => {
  return [...defaultMenus, ...favoriteMenus.value];
});
const getWelcomeTitle = computed(() => {
  const now = new Date();
  const hour = now.getHours();
  if (hour < 12) {
    return $t('workbench.header.welcome.morning', [
      userStore.userInfo?.realName,
    ]);
  }
  if (hour < 14) {
    return $t('workbench.header.welcome.atoon', [userStore.userInfo?.realName]);
  }
  if (hour < 17) {
    return $t('workbench.header.welcome.afternoon', [
      userStore.userInfo?.realName,
    ]);
  }
  if (hour < 24) {
    return $t('workbench.header.welcome.evening', [
      userStore.userInfo?.realName,
    ]);
  }
  return '';
});

const [WorkbenchQuickNavModal, quickNavModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkbenchQuickNavModal.vue'),
  ),
});

async function onInit() {
  await Promise.all([
    onInitFavoriteMenus(),
    // onInitNotifiers(),
    onInitTodoList(),
  ]);
}
async function onInitFavoriteMenus() {
  const { items } = await getFavoriteMenusApi(uiFramework);
  favoriteMenus.value = items.map((item) => {
    return {
      ...item,
      id: item.menuId,
      isDefault: false,
    };
  });
}
// async function onInitNotifiers() {
//   const { items, totalCount } = await getMyNotifilersApi({
//     maxResultCount: 10,
//     readState: NotificationReadState.UnRead,
//   });
//   unReadNotifilers.value = items.map((item) => {
//     const notifier = deserialize(item);
//     return {
//       avatar: '',
//       date: formatToDateTime(item.creationTime),
//       title: notifier.title,
//       content: notifier.message,
//     };
//   });
//   unReadNotifilerCount.value = totalCount;
// }
async function onInitTodoList() {
  // TODO: 实现待办事项列表
  todoList.value = [];
}

function onCreatingFavoriteMenu() {
  quickNavModalApi.open();
}

async function onDeleteFavoriteMenu(menu: FavoriteMenu) {
  await deleteFavoriteMenuApi(menu.id);
  await onInitFavoriteMenus();
  message.success($t('AbpUi.DeletedSuccessfully'));
}

onMounted(onInit);
</script>

<template>
  <div class="p-5">
    <WorkbenchHeader
      :avatar="userStore.userInfo?.avatar || preferences.app.defaultAvatar"
      :text="userStore.userInfo?.realName"
      :notifier-count="unReadNotifilerCount"
    >
      <template #title>
        {{ getWelcomeTitle }}
      </template>
      <template #description> 今日晴，20℃ - 32℃！ </template>
    </WorkbenchHeader>

    <div class="mt-5 flex flex-col lg:flex-row">
      <div class="mr-4 w-full lg:w-3/5">
        <WorkbenchQuickNav
          :items="getFavoriteMenus"
          class="mt-5 lg:mt-0"
          :title="$t('workbench.content.favoriteMenu.title')"
          @add="onCreatingFavoriteMenu"
          @delete="onDeleteFavoriteMenu"
          @click="(menu: FavoriteMenu) => $emit('navTo', menu)"
        />
        <WorkbenchTodo
          :items="todoList"
          class="mt-5"
          :title="$t('workbench.content.todo.title')"
        >
          <template #empty>
            <Empty />
          </template>
        </WorkbenchTodo>
      </div>
      <div class="w-full lg:w-2/5">
        <WorkbenchTrends
          :items="unReadNotifilers"
          :title="$t('workbench.content.trends.title')"
        >
          <template #empty>
            <Empty />
          </template>
        </WorkbenchTrends>
      </div>
    </div>
    <WorkbenchQuickNavModal @change="onInitFavoriteMenus" />
  </div>
</template>

<style scoped></style>
