<script lang="ts" setup>
import type { NotificationItem } from './types';

import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Bell, CircleX, MailOpen } from '@vben/icons';
import { $t } from '@vben/locales';

import {
  VbenButton,
  VbenIconButton,
  VbenPopover,
  VbenScrollbar,
} from '@vben-core/shadcn-ui';

import { useToggle } from '@vueuse/core';

interface Props {
  /**
   * 显示圆点
   */
  dot?: boolean;
  /**
   * 消息列表
   */
  notifications?: NotificationItem[];
  /**
   * 通告列表
   */
  broadcasts?: NotificationItem[];
  /**
   * 是否允许删除已读消息
   */
  removable?: boolean;
}

defineOptions({ name: 'NotificationPopup' });

const props = withDefaults(defineProps<Props>(), {
  dot: false,
  broadcasts: () => [],
  notifications: () => [],
  removable: true,
});

const emit = defineEmits<{
  clear: [];
  clearBroadcasts: [];
  read: [NotificationItem];
  readBroadcast: [NotificationItem];
  remove: [NotificationItem];
  removeBroadcast: [NotificationItem];
  viewAll: [];
  viewBroadcastAll: [];
}>();

const router = useRouter();
const activeTab = ref<'broadcast' | 'message'>('message');
const [open, toggle] = useToggle();
const visibleBroadcasts = computed(() =>
  props.broadcasts.filter((item) => !item.isRead),
);

function close() {
  open.value = false;
}

function handleViewAll() {
  emit('viewAll');
  close();
}

function handleViewBroadcastAll() {
  emit('viewBroadcastAll');
  close();
}

function handleClear() {
  emit('clear');
}

function handleClearBroadcasts() {
  emit('clearBroadcasts');
}

function handleBroadcastRead(item: NotificationItem) {
  emit('readBroadcast', item);
}

function handleBroadcastRemove(item: NotificationItem) {
  emit('removeBroadcast', item);
}

function unreadBroadcastCount() {
  return visibleBroadcasts.value.length;
}

function footerClearDisabled() {
  return activeTab.value === 'message'
    ? props.notifications.length <= 0
    : unreadBroadcastCount() <= 0;
}

function footerViewAllText() {
  return activeTab.value === 'message'
    ? $t('ui.widgets.viewAll')
    : $t('ui.widgets.viewAllBroadcasts');
}

function handleFooterClear() {
  if (activeTab.value === 'message') {
    handleClear();
  } else {
    handleClearBroadcasts();
  }
}

function handleFooterViewAll() {
  if (activeTab.value === 'message') {
    handleViewAll();
  } else {
    handleViewBroadcastAll();
  }
}

function handleClick(item: NotificationItem) {
  // 如果通知项有链接，点击时跳转
  if (item.link) {
    navigateTo(item.link, item.query, item.state);
  }
}

function navigateTo(
  link: string,
  query?: Record<string, any>,
  state?: Record<string, any>,
) {
  if (link.startsWith('http://') || link.startsWith('https://')) {
    // 外部链接，在新标签页打开
    window.open(link, '_blank');
  } else {
    // 内部路由链接，支持 query 参数和 state
    router.push({
      path: link,
      query: query || {},
      state,
    });
  }
}
</script>
<template>
  <VbenPopover
    v-model:open="open"
    content-class="relative right-2 w-[360px] p-0"
  >
    <template #trigger>
      <div class="flex-center mr-2 h-full" @click.stop="toggle()">
        <VbenIconButton class="bell-button text-foreground relative">
          <span
            v-if="dot"
            class="bg-primary absolute right-0.5 top-0.5 h-2 w-2 rounded"
          ></span>
          <Bell class="size-4" />
        </VbenIconButton>
      </div>
    </template>

    <div class="relative">
      <div
        class="border-border flex items-center justify-between gap-2 border-b px-4 py-3"
      >
        <div class="flex items-center gap-1">
          <button
            type="button"
            class="rounded-md px-3 py-1.5 text-sm transition"
            :class="
              activeTab === 'message'
                ? 'bg-primary/10 text-primary font-medium'
                : 'text-muted-foreground hover:bg-accent'
            "
            @click="activeTab = 'message'"
          >
            消息
            <span v-if="notifications.length > 0" class="ml-1 text-xs">
              {{ notifications.length }}
            </span>
          </button>
          <button
            type="button"
            class="rounded-md px-3 py-1.5 text-sm transition"
            :class="
              activeTab === 'broadcast'
                ? 'bg-primary/10 text-primary font-medium'
                : 'text-muted-foreground hover:bg-accent'
            "
            @click="activeTab = 'broadcast'"
          >
            通告
            <span v-if="unreadBroadcastCount() > 0" class="ml-1 text-xs">
              {{ unreadBroadcastCount() }}
            </span>
          </button>
        </div>
      </div>
      <VbenScrollbar v-if="activeTab === 'message' && notifications.length > 0">
        <ul class="!flex max-h-[360px] w-full flex-col">
          <template v-for="item in notifications" :key="item.id ?? item.title">
            <li
              class="hover:bg-accent border-border relative flex w-full cursor-pointer items-start gap-5 border-t px-3 py-3"
              @click="handleClick(item)"
            >
              <span
                v-if="!item.isRead"
                class="bg-primary absolute right-2 top-2 h-2 w-2 rounded"
              ></span>

              <span
                class="relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full"
              >
                <img
                  :src="item.avatar"
                  class="aspect-square h-full w-full object-cover"
                />
              </span>
              <div
                class="flex min-w-0 flex-1 flex-col gap-1 pr-14 leading-none"
              >
                <p class="font-semibold" :title="item.title">
                  {{ item.title }}
                </p>
                <p
                  class="text-muted-foreground my-1 line-clamp-2 break-words text-xs"
                  :title="item.message"
                >
                  {{ item.message }}
                </p>
                <p
                  class="text-muted-foreground line-clamp-2 break-words text-xs"
                  :title="item.date"
                >
                  {{ item.date }}
                </p>
              </div>
              <div
                class="absolute right-3 top-1/2 flex -translate-y-1/2 flex-col gap-2"
              >
                <VbenIconButton
                  v-if="!item.isRead"
                  size="xs"
                  variant="ghost"
                  class="h-6 px-2"
                  :tooltip="$t('ui.widgets.markAsRead')"
                  @click.stop="emit('read', item)"
                >
                  <MailOpen class="size-4" />
                </VbenIconButton>
                <VbenIconButton
                  v-if="removable && item.isRead"
                  size="xs"
                  variant="ghost"
                  class="text-destructive h-6 px-2"
                  :tooltip="$t('common.delete')"
                  @click.stop="emit('remove', item)"
                >
                  <CircleX class="size-4" />
                </VbenIconButton>
              </div>
            </li>
          </template>
        </ul>
      </VbenScrollbar>

      <VbenScrollbar
        v-else-if="activeTab === 'broadcast' && visibleBroadcasts.length > 0"
      >
        <ul class="!flex max-h-[360px] w-full flex-col">
          <li
            v-for="item in visibleBroadcasts"
            :key="item.id ?? item.title"
            class="hover:bg-accent border-border relative flex w-full cursor-pointer items-start gap-5 border-t px-3 py-3"
            @click="handleClick(item)"
          >
            <span
              v-if="!item.isRead"
              class="bg-primary absolute right-2 top-2 h-2 w-2 rounded"
            ></span>

            <span
              class="relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full"
            >
              <img
                :src="item.avatar"
                class="aspect-square h-full w-full object-cover"
              />
            </span>
            <div class="flex min-w-0 flex-1 flex-col gap-1 pr-14 leading-none">
              <p class="font-semibold" :title="item.title">
                {{ item.title }}
              </p>
              <p
                class="text-muted-foreground my-1 line-clamp-2 break-words text-xs"
                :title="item.message"
              >
                {{ item.message }}
              </p>
              <p
                class="text-muted-foreground line-clamp-2 break-words text-xs"
                :title="item.date"
              >
                {{ item.date }}
              </p>
            </div>
            <div
              class="absolute right-3 top-1/2 flex -translate-y-1/2 flex-col gap-2"
            >
              <VbenIconButton
                v-if="!item.isRead"
                size="xs"
                variant="ghost"
                class="h-6 px-2"
                :tooltip="$t('ui.widgets.markAsRead')"
                @click.stop="handleBroadcastRead(item)"
              >
                <MailOpen class="size-4" />
              </VbenIconButton>
              <VbenIconButton
                v-if="removable && item.isRead"
                size="xs"
                variant="ghost"
                class="text-destructive h-6 px-2"
                :tooltip="$t('common.delete')"
                @click.stop="handleBroadcastRemove(item)"
              >
                <CircleX class="size-4" />
              </VbenIconButton>
            </div>
          </li>
        </ul>
      </VbenScrollbar>

      <div
        v-else
        class="flex-center text-muted-foreground min-h-[150px] w-full"
      >
        {{ $t('common.noData') }}
      </div>

      <div class="border-border flex items-center gap-2 border-t px-4 py-3">
        <VbenButton
          :disabled="footerClearDisabled()"
          size="sm"
          variant="ghost"
          @click="handleFooterClear"
        >
          {{ $t('ui.widgets.markAllAsRead') }}
        </VbenButton>
        <div class="ml-auto">
          <VbenButton size="sm" @click="handleFooterViewAll">
            {{ footerViewAllText() }}
          </VbenButton>
        </div>
      </div>
    </div>
  </VbenPopover>
</template>

<style scoped>
:deep(.bell-button) {
  &:hover {
    svg {
      animation: bell-ring 1s both;
    }
  }
}

@keyframes bell-ring {
  0%,
  100% {
    transform-origin: top;
  }

  15% {
    transform: rotateZ(10deg);
  }

  30% {
    transform: rotateZ(-10deg);
  }

  45% {
    transform: rotateZ(5deg);
  }

  60% {
    transform: rotateZ(-5deg);
  }

  75% {
    transform: rotateZ(2deg);
  }
}
</style>
