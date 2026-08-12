<script lang="ts" setup>
import type { NotificationOutput } from '@abp/core';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  NotificationMessageLevel,
  NotificationMessageType,
} from '@abp/core';
import {
  ClockCircleOutlined,
  MailOutlined,
  SoundOutlined,
  UserOutlined,
} from '@ant-design/icons-vue';
import { Tag } from 'ant-design-vue';

const row = ref<NotificationOutput>();

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[620px]',
  contentClass: 'p-0',
  onOpenChange: (isOpen: boolean) => {
    if (isOpen) {
      row.value = drawerApi.getData<NotificationOutput>();
    }
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.Notification:Detail'),
});

const isBroadcast = computed(
  () => row.value?.messageType === NotificationMessageType.BroadCast,
);

const receiverText = computed(() => {
  if (isBroadcast.value) {
    return $t('TestWorkshop.Notification:BroadcastScope');
  }
  return (
    row.value?.receiveUserName || $t('TestWorkshop.Notification:PrivateScope')
  );
});

function levelColor(level: NotificationMessageLevel) {
  switch (level) {
    case NotificationMessageLevel.Error: {
      return 'error';
    }
    case NotificationMessageLevel.Warning: {
      return 'warning';
    }
    default: {
      return 'processing';
    }
  }
}

function levelLabel(level: NotificationMessageLevel) {
  switch (level) {
    case NotificationMessageLevel.Error: {
      return $t('TestWorkshop.NotificationLevel:Error');
    }
    case NotificationMessageLevel.Warning: {
      return $t('TestWorkshop.NotificationLevel:Warning');
    }
    default: {
      return $t('TestWorkshop.NotificationLevel:Information');
    }
  }
}

function accentClass(level: NotificationMessageLevel) {
  switch (level) {
    case NotificationMessageLevel.Error: {
      return 'border-red-200 bg-red-50 text-red-600 dark:border-red-900 dark:bg-red-950/30 dark:text-red-400';
    }
    case NotificationMessageLevel.Warning: {
      return 'border-amber-200 bg-amber-50 text-amber-600 dark:border-amber-900 dark:bg-amber-950/30 dark:text-amber-400';
    }
    default: {
      return 'border-blue-200 bg-blue-50 text-blue-600 dark:border-blue-900 dark:bg-blue-950/30 dark:text-blue-400';
    }
  }
}
</script>

<template>
  <Drawer>
    <template #extra>
      <Tag v-if="row" :color="levelColor(row.messageLevel)">
        {{ levelLabel(row.messageLevel) }}
      </Tag>
    </template>

    <div v-if="row" class="flex h-full flex-col">
      <div class="border-b px-6 py-6">
        <div class="flex items-start gap-4">
          <div
            class="flex size-12 shrink-0 items-center justify-center rounded-md border"
            :class="accentClass(row.messageLevel)"
          >
            <SoundOutlined v-if="isBroadcast" class="text-xl" />
            <MailOutlined v-else class="text-xl" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex flex-wrap items-center gap-2">
              <Tag :color="isBroadcast ? 'geekblue' : 'purple'">
                {{
                  isBroadcast
                    ? $t('TestWorkshop.Notification:Broadcast')
                    : $t('TestWorkshop.Notification:Message')
                }}
              </Tag>
            </div>
            <h2 class="mt-2 break-words text-xl font-semibold leading-7">
              {{ row.title }}
            </h2>
          </div>
        </div>
      </div>

      <div class="border-b px-6 py-5">
        <div class="grid grid-cols-2 gap-x-8 gap-y-6">
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.DisplayName:From') }}
            </p>
            <p class="mt-1 flex items-center gap-1 text-sm font-medium">
              <UserOutlined />
              {{ row.senderUserName }}
            </p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Notification:Receiver') }}
            </p>
            <p class="mt-1 flex items-center gap-1 text-sm font-medium">
              <MailOutlined />
              {{ receiverText }}
            </p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.DisplayName:SendTime') }}
            </p>
            <p class="mt-1 flex items-center gap-1 text-sm font-medium">
              <ClockCircleOutlined />
              {{ formatToDateTime(row.creationTime) }}
            </p>
          </div>
          <div v-if="!isBroadcast">
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Notification:ReadState') }}
            </p>
            <Tag class="mt-1" :color="row.read ? 'success' : 'warning'">
              {{
                row.read
                  ? $t('TestWorkshop.Notification:Read')
                  : $t('TestWorkshop.Notification:Unread')
              }}
            </Tag>
            <p v-if="row.readTime" class="mt-1 text-xs text-gray-400">
              {{ formatToDateTime(row.readTime) }}
            </p>
          </div>
          <div v-else>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Notification:Type') }}
            </p>
            <Tag class="mt-1" color="geekblue">
              {{ $t('TestWorkshop.Notification:Broadcast') }}
            </Tag>
          </div>
          <div class="col-span-2">
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Notification:Id') }}
            </p>
            <p class="mt-1 break-all font-mono text-xs leading-5">
              {{ row.id }}
            </p>
          </div>
        </div>
      </div>

      <div class="flex-1 overflow-auto px-6 py-5">
        <p class="mb-3 text-sm font-medium text-gray-500">
          {{ $t('TestWorkshop.DisplayName:Content') }}
        </p>
        <p
          class="whitespace-pre-wrap border-l-2 border-gray-200 pl-5 text-base leading-7"
        >
          {{ row.content }}
        </p>
      </div>
    </div>
  </Drawer>
</template>
