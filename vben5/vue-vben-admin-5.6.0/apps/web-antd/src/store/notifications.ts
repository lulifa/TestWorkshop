import type { NotificationOutput } from '@abp/core';

import type { WorkbenchTrendItem } from '@vben/common-ui';
import type { NotificationItem } from '@vben/layouts';

import { computed, ref } from 'vue';

import { preferences } from '@vben/preferences';

import {
  Events,
  formatToDateTime,
  useEventBus,
  useNotificationsApi,
} from '@abp/core';
import { defineStore } from 'pinia';

const MESSAGE_LIST_PATH = '/business/notifications/my-message';
const WORKBENCH_TREND_COUNT = 10;

export const useNotificationStore = defineStore('notification', () => {
  const { subscribe } = useEventBus();
  const { getMyNotificationListApi, setBatchReadApi, setReadApi } =
    useNotificationsApi();

  const notifications = ref<NotificationItem[]>([]);
  const unreadNotifications = ref<NotificationItem[]>([]);
  const unreadCount = ref(0);
  let refreshPromise: null | Promise<void> = null;

  const showDot = computed(() => unreadCount.value > 0);

  const unreadTrends = computed<WorkbenchTrendItem[]>(() =>
    unreadNotifications.value
      .filter((item) => !item.isRead)
      .slice(0, WORKBENCH_TREND_COUNT)
      .map((item) => ({
        avatar: 'lucide:mail-open',
        content: item.message,
        date: item.date,
        title: item.title,
      })),
  );

  function toNotificationItem(item: NotificationOutput): NotificationItem {
    return {
      avatar: preferences.app.defaultAvatar,
      date: formatToDateTime(item.creationTime),
      id: item.id,
      isRead: item.read,
      link: MESSAGE_LIST_PATH,
      message: item.content,
      title: item.title,
    };
  }

  function toSignalRItem(message: any): NotificationItem {
    return {
      avatar: preferences.app.defaultAvatar,
      date: formatToDateTime(new Date()),
      id: String(message.id),
      isRead: false,
      link: MESSAGE_LIST_PATH,
      message: message.content ?? '',
      title: message.title ?? '',
    };
  }

  function mergeItem(
    list: NotificationItem[],
    item: NotificationItem,
  ): NotificationItem[] {
    const index = list.findIndex((entry) => entry.id === item.id);
    const next = index === -1 ? [item, ...list] : [...list];
    if (index !== -1) {
      next[index] = item;
    }
    return next;
  }

  async function refresh() {
    if (refreshPromise) {
      return refreshPromise;
    }
    refreshPromise = (async () => {
      try {
        const unreadResult = await getMyNotificationListApi({
          isPaged: false,
          read: false,
        });
        notifications.value = unreadResult.items.map((item) =>
          toNotificationItem(item),
        );
        unreadNotifications.value = unreadResult.items.map((item) =>
          toNotificationItem(item),
        );
        unreadCount.value = unreadResult.totalCount;
      } catch {
        // Request errors are handled by the global request interceptor.
      } finally {
        refreshPromise = null;
      }
    })();
    return refreshPromise;
  }

  function markLocalRead(id: number | string) {
    const changed = notifications.value.some(
      (item) => item.id === id && !item.isRead,
    );
    notifications.value = notifications.value.filter((item) => item.id !== id);
    unreadNotifications.value = unreadNotifications.value.filter(
      (item) => item.id !== id,
    );
    if (changed && unreadCount.value > 0) {
      unreadCount.value--;
    }
  }

  async function markRead(id: number | string) {
    markLocalRead(id);
    try {
      await setReadApi({ id: String(id) });
    } catch {
      await refresh();
    }
  }

  async function clear() {
    const ids = notifications.value
      .filter((item) => !item.isRead)
      .map((item) => String(item.id));
    try {
      if (ids.length > 0) {
        await setBatchReadApi({ ids });
        unreadCount.value = Math.max(0, unreadCount.value - ids.length);
      }
      notifications.value = [];
      unreadNotifications.value = [];
    } catch {
      await refresh();
    }
  }

  function remove(id: number | string) {
    notifications.value = notifications.value.filter((item) => item.id !== id);
    unreadNotifications.value = unreadNotifications.value.filter(
      (item) => item.id !== id,
    );
  }

  subscribe('signalR:ReceiveTextMessage', (message: any) => {
    const item = toSignalRItem(message);
    const existed = notifications.value.find((entry) => entry.id === item.id);
    notifications.value = mergeItem(notifications.value, item);
    unreadNotifications.value = mergeItem(unreadNotifications.value, item);
    if (!existed || existed.isRead) {
      unreadCount.value++;
    }
  });

  subscribe(Events.UserLogin, () => {
    void refresh();
  });

  subscribe(Events.UserLogout, () => {
    notifications.value = [];
    unreadNotifications.value = [];
    unreadCount.value = 0;
  });

  return {
    clear,
    markRead,
    notifications,
    refresh,
    remove,
    showDot,
    unreadCount,
    unreadTrends,
  };
});
