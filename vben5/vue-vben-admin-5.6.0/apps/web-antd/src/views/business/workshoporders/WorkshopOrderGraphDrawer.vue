<script lang="ts" setup>
import type {
  WorkshopOrderDto,
  WorkshopOrderTelemetryGraphDto,
} from '@abp/core';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { formatToDateTime, useWorkshopOrderApi } from '@abp/core';
import { Empty, Spin } from 'ant-design-vue';

import WorkshopOrderGraphChart from './WorkshopOrderGraphChart.vue';

const { getTelemetryGraphApi } = useWorkshopOrderApi();

const order = ref<WorkshopOrderDto>();
const result = ref<WorkshopOrderTelemetryGraphDto>();
const loading = ref(false);
const loadFailed = ref(false);

const graphs = computed(() => result.value?.graphs ?? []);

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-full max-w-full',
  contentClass: 'p-4',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      return;
    }

    const data = drawerApi.getData<WorkshopOrderDto>();
    order.value = data;
    result.value = undefined;
    loadFailed.value = false;
    if (!data?.id) {
      return;
    }

    loading.value = true;
    try {
      result.value = await getTelemetryGraphApi(data.id);
    } catch {
      loadFailed.value = true;
    } finally {
      loading.value = false;
    }
  },
  showConfirmButton: false,
  title: $t('page.business.orderGraph'),
});

function getGroupTitle(timestamp: string) {
  return timestamp ? formatToDateTime(timestamp) : '-';
}
</script>

<template>
  <Drawer>
    <div class="flex min-h-full flex-col gap-4">
      <div class="rounded-md border border-border px-5 py-4">
        <div class="text-base font-semibold">
          {{ result?.orderNo ?? order?.orderNo ?? '-' }}
        </div>
        <div class="mt-2 flex flex-wrap gap-x-6 gap-y-1 text-sm text-gray-500">
          <span>
            {{ $t('TestWorkshop.WorkshopOrder:Device') }}:
            <span class="text-foreground">
              {{ result?.deviceCode ?? order?.deviceCode ?? '-' }}
              <span v-if="result?.deviceName ?? order?.deviceName">
                · {{ result?.deviceName ?? order?.deviceName }}
              </span>
            </span>
          </span>
          <span>
            {{ $t('TestWorkshop.Telemetry:TestedDeviceCode') }}:
            <span class="text-foreground">
              {{ result?.testedDeviceCode || '-' }}
              <span v-if="result?.testedDeviceName">
                · {{ result.testedDeviceName }}
              </span>
            </span>
          </span>
        </div>
      </div>

      <div v-if="loading" class="flex flex-1 items-center justify-center py-20">
        <Spin />
      </div>

      <Empty
        v-else-if="loadFailed"
        class="py-20"
        :description="$t('ui.actionMessage.operationFailed')"
      />

      <Empty
        v-else-if="graphs.length === 0"
        class="py-20"
        :description="$t('common.noData')"
      />

      <div v-else class="flex flex-col gap-4">
        <div
          v-for="group in graphs"
          :key="group.timestamp"
          class="rounded-md border border-border px-3 py-3"
        >
          <div class="mb-1 flex items-center justify-between px-1">
            <span class="text-sm font-medium">
              {{ getGroupTitle(group.timestamp) }}
            </span>
            <span class="text-xs text-gray-400">
              {{ group.channels.length }}
            </span>
          </div>
          <WorkshopOrderGraphChart :channels="group.channels" height="46vh" />
        </div>
      </div>
    </div>
  </Drawer>
</template>

<style scoped></style>
