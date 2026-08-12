import type {
  NotificationOutput,
  NotificationSubscriptionOutput,
} from '@abp/core';

import type { WorkbenchTrendItem } from '@vben/common-ui';
import type { NotificationItem } from '@vben/layouts';

import { computed, ref } from 'vue';

import {
  Events,
  formatToDateTime,
  useEventBus,
  useNotificationsApi,
} from '@abp/core';
import { defineStore } from 'pinia';

const MESSAGE_LIST_PATH = '/business/notifications/my-message';
const BROADCAST_LIST_PATH = '/business/notifications/my-broadcast';
const WORKBENCH_TREND_COUNT = 10;

function defaultMessageIcon() {
  return `${window.location.origin}${import.meta.env.BASE_URL}icon/message.svg`;
}

export const useNotificationStore = defineStore('notification', () => {
  const { subscribe } = useEventBus();
  const {
    deleteSubscriptionApi,
    getMyNotificationListApi,
    getSubscriptionListApi,
    setBatchReadApi,
    setReadApi,
    setSubscriptionBatchReadApi,
    setSubscriptionReadApi,
  } = useNotificationsApi();

  const notifications = ref<NotificationItem[]>([]);
  const unreadNotifications = ref<NotificationItem[]>([]);
  const broadcasts = ref<NotificationItem[]>([]);
  const unreadCount = ref(0);
  let refreshPromise: null | Promise<void> = null;

  const unreadBroadcastCount = computed(
    () => broadcasts.value.filter((item) => !item.isRead).length,
  );

  const showDot = computed(
    () => unreadCount.value > 0 || unreadBroadcastCount.value > 0,
  );

  const unreadTrends = computed<WorkbenchTrendItem[]>(() =>
    unreadNotifications.value
      .filter((item) => !item.isRead)
      .slice(0, WORKBENCH_TREND_COUNT)
      .map((item) => ({
        avatar: defaultMessageIcon(),
        content: item.message,
        date: item.date,
        title: item.title,
      })),
  );

  const broadcastTrends = computed<WorkbenchTrendItem[]>(() =>
    broadcasts.value.slice(0, WORKBENCH_TREND_COUNT).map((item) => ({
      avatar: defaultMessageIcon(),
      content: item.message,
      date: item.date,
      title: item.title,
    })),
  );

  function toNotificationItem(item: NotificationOutput): NotificationItem {
    return {
      avatar: defaultMessageIcon(),
      date: formatToDateTime(item.creationTime),
      id: item.id,
      isRead: item.read,
      link: MESSAGE_LIST_PATH,
      message: item.content,
      query: { read: 'false' },
      title: item.title,
    };
  }

  function toSignalRItem(
    message: any,
    link = MESSAGE_LIST_PATH,
  ): NotificationItem {
    return {
      avatar: defaultMessageIcon(),
      date: formatToDateTime(new Date()),
      id: String(message.id),
      isRead: false,
      link,
      message: message.content ?? '',
      query: { read: 'false' },
      title: message.title ?? '',
    };
  }

  function toBroadcastItem(
    item: NotificationSubscriptionOutput,
  ): NotificationItem {
    return {
      avatar: defaultMessageIcon(),
      date: formatToDateTime(item.creationTime),
      id: item.id,
      isRead: item.read,
      link: BROADCAST_LIST_PATH,
      message: item.content,
      query: { read: 'false' },
      title: item.title,
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
        const broadcastResult = await getSubscriptionListApi({
          maxResultCount: 20,
          read: false,
        });
        broadcasts.value = broadcastResult.items.map((item) =>
          toBroadcastItem(item),
        );
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

  async function markBroadcastRead(id: number | string) {
    broadcasts.value = broadcasts.value.filter((entry) => entry.id !== id);
    try {
      await setSubscriptionReadApi({ id: String(id) });
    } catch {
      await refresh();
    }
  }

  async function clearBroadcasts() {
    const ids = broadcasts.value
      .filter((item) => !item.isRead)
      .map((item) => String(item.id));
    try {
      if (ids.length > 0) {
        await setSubscriptionBatchReadApi({ ids });
      }
      broadcasts.value = [];
    } catch {
      await refresh();
    }
  }

  async function removeBroadcast(id: number | string) {
    broadcasts.value = broadcasts.value.filter((entry) => entry.id !== id);
    try {
      await deleteSubscriptionApi({ id: String(id) });
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

  subscribe('signalR:ReceiveBroadCastMessage', () => {
    void refresh();
  });

  subscribe(Events.UserLogin, () => {
    void refresh();
  });

  subscribe(Events.UserLogout, () => {
    notifications.value = [];
    unreadNotifications.value = [];
    broadcasts.value = [];
    unreadCount.value = 0;
  });

  return {
    broadcasts,
    broadcastTrends,
    clearBroadcasts,
    clear,
    markBroadcastRead,
    markRead,
    notifications,
    refresh,
    remove,
    removeBroadcast,
    showDot,
    unreadCount,
    unreadBroadcastCount,
    unreadTrends,
  };
});
