<script lang="ts" setup>
import type { WorkshopOrderDto } from '@abp/core';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useWorkshopOrderApi,
  WorkshopOrderStatus,
} from '@abp/core';
import { Tag } from 'ant-design-vue';

const { getApi } = useWorkshopOrderApi();

const order = ref<WorkshopOrderDto>();
const loading = ref(false);

const statusColorMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: 'default',
  [WorkshopOrderStatus.Testing]: 'processing',
  [WorkshopOrderStatus.Completed]: 'success',
  [WorkshopOrderStatus.Cancelled]: 'error',
};

const statusLabelMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: $t('TestWorkshop.WorkshopOrder:Draft'),
  [WorkshopOrderStatus.Testing]: $t('TestWorkshop.WorkshopOrder:Testing'),
  [WorkshopOrderStatus.Completed]: $t('TestWorkshop.WorkshopOrder:Completed'),
  [WorkshopOrderStatus.Cancelled]: $t('TestWorkshop.WorkshopOrder:Cancelled'),
};

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[680px] max-w-[96vw]',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      return;
    }

    const data = drawerApi.getData<WorkshopOrderDto>();
    order.value = undefined;
    if (!data?.id) {
      return;
    }

    loading.value = true;
    try {
      order.value = await getApi(data.id);
    } finally {
      loading.value = false;
    }
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.WorkshopOrder:Details'),
});

function formatTime(value?: string) {
  return value ? formatToDateTime(value) : '-';
}
</script>

<template>
  <Drawer>
    <div v-if="order" class="space-y-5">
      <div class="rounded-md border border-border px-5 py-4">
        <div class="flex items-center justify-between gap-4">
          <div class="min-w-0">
            <div class="truncate text-lg font-semibold">
              {{ order.orderNo }}
            </div>
            <div class="mt-1 text-sm text-gray-500">
              {{ order.deviceCode ?? '-' }}
              <span v-if="order.deviceName"> - {{ order.deviceName }}</span>
            </div>
          </div>
          <Tag :color="statusColorMap[order.status]">
            {{ statusLabelMap[order.status] ?? order.statusName }}
          </Tag>
        </div>
      </div>

      <div class="grid grid-cols-1 gap-x-8 gap-y-5 sm:grid-cols-2">
        <div>
          <div class="text-xs text-gray-400">
            {{ $t('TestWorkshop.WorkshopOrder:OrderNo') }}
          </div>
          <div class="mt-1 font-medium">{{ order.orderNo }}</div>
        </div>
        <div>
          <div class="text-xs text-gray-400">
            {{ $t('TestWorkshop.WorkshopOrder:Status') }}
          </div>
          <div class="mt-1">
            {{ statusLabelMap[order.status] ?? order.statusName }}
          </div>
        </div>
        <div>
          <div class="text-xs text-gray-400">
            {{ $t('TestWorkshop.WorkshopOrder:Device') }}
          </div>
          <div class="mt-1">
            {{ order.deviceCode ?? '-' }}
            <span v-if="order.deviceName" class="text-gray-500">
              - {{ order.deviceName }}
            </span>
          </div>
        </div>
        <div>
          <div class="text-xs text-gray-400">
            {{ $t('TestWorkshop.DisplayName:CreatedAt') }}
          </div>
          <div class="mt-1">{{ formatTime(order.creationTime) }}</div>
        </div>
        <div>
          <div class="text-xs text-gray-400">
            {{ $t('TestWorkshop.WorkshopOrder:TestStartedAt') }}
          </div>
          <div class="mt-1">{{ formatTime(order.testStartedAt) }}</div>
        </div>
        <div>
          <div class="text-xs text-gray-400">
            {{ $t('TestWorkshop.WorkshopOrder:TestEndedAt') }}
          </div>
          <div class="mt-1">{{ formatTime(order.testEndedAt) }}</div>
        </div>
      </div>

      <div v-if="loading" class="text-center text-sm text-gray-400">
        Loading...
      </div>
    </div>
  </Drawer>
</template>

<style scoped></style>
