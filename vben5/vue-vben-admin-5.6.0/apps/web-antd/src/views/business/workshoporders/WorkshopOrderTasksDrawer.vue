<script lang="ts" setup>
import type { WorkshopOrderDto, WorkshopTelemetryTaskDto } from '@abp/core';

import { reactive, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useWorkshopOrderApi,
  WorkshopTelemetryStatus,
} from '@abp/core';
import { Table, Tag } from 'ant-design-vue';

const { getTelemetryTasksApi } = useWorkshopOrderApi();

const order = ref<WorkshopOrderDto>();
const loading = ref(false);
const tasks = ref<WorkshopTelemetryTaskDto[]>([]);
const pagination = reactive({
  current: 1,
  onChange: (page: number, pageSize: number) => {
    void loadTasks(page, pageSize);
  },
  pageSize: 10,
  showSizeChanger: true,
  showTotal: (total: number) => `${total}`,
  total: 0,
});

const columns = [
  {
    dataIndex: 'fileName',
    ellipsis: true,
    key: 'fileName',
    title: $t('TestWorkshop.Telemetry:FileName'),
  },
  {
    dataIndex: 'status',
    key: 'status',
    title: $t('TestWorkshop.Telemetry:Status'),
    width: 110,
  },
  {
    dataIndex: 'fileSize',
    key: 'fileSize',
    title: $t('TestWorkshop.Telemetry:FileSize'),
    width: 120,
  },
  {
    dataIndex: 'recordCount',
    key: 'recordCount',
    title: $t('TestWorkshop.Telemetry:SampleCount'),
    width: 110,
  },
  {
    dataIndex: 'createdAt',
    key: 'createdAt',
    title: $t('TestWorkshop.DisplayName:CreatedAt'),
    width: 180,
  },
  {
    dataIndex: 'processedAt',
    key: 'processedAt',
    title: $t('TestWorkshop.Telemetry:ProcessedAt'),
    width: 180,
  },
];

const statusColorMap: Record<WorkshopTelemetryStatus, string> = {
  [WorkshopTelemetryStatus.Pending]: 'warning',
  [WorkshopTelemetryStatus.Processing]: 'processing',
  [WorkshopTelemetryStatus.Success]: 'success',
  [WorkshopTelemetryStatus.Failed]: 'error',
};

const statusLabelMap: Record<WorkshopTelemetryStatus, string> = {
  [WorkshopTelemetryStatus.Pending]: $t('TestWorkshop.Telemetry:Pending'),
  [WorkshopTelemetryStatus.Processing]: $t('TestWorkshop.Telemetry:Processing'),
  [WorkshopTelemetryStatus.Success]: $t('TestWorkshop.Telemetry:Success'),
  [WorkshopTelemetryStatus.Failed]: $t('TestWorkshop.Telemetry:Failed'),
};

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[1080px] max-w-[96vw]',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      return;
    }

    order.value = drawerApi.getData<WorkshopOrderDto>();
    pagination.current = 1;
    await loadTasks(1, pagination.pageSize);
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.WorkshopOrder:TelemetryTasks'),
});

async function loadTasks(page: number, pageSize: number) {
  if (!order.value?.id) {
    return;
  }

  loading.value = true;
  try {
    const result = await getTelemetryTasksApi(order.value.id, {
      maxResultCount: pageSize,
      skipCount: (page - 1) * pageSize,
    });
    tasks.value = result.items;
    pagination.current = page;
    pagination.pageSize = pageSize;
    pagination.total = result.totalCount;
  } finally {
    loading.value = false;
  }
}

function formatFileSize(size: number) {
  if (!size) {
    return '0 B';
  }

  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  let value = size;
  let index = 0;
  while (value >= 1024 && index < units.length - 1) {
    value /= 1024;
    index += 1;
  }
  return `${value.toFixed(index > 0 ? 2 : 0)} ${units[index]}`;
}

function getStatusColor(status: WorkshopTelemetryStatus) {
  return statusColorMap[status] ?? 'default';
}

function getStatusLabel(status: WorkshopTelemetryStatus, statusName?: string) {
  return statusLabelMap[status] ?? statusName ?? String(status);
}
</script>

<template>
  <Drawer>
    <div class="mb-4 rounded-md border border-border px-4 py-3">
      <div class="text-sm font-medium">{{ order?.orderNo }}</div>
      <div class="mt-1 text-xs text-gray-500">
        {{ order?.deviceCode }} - {{ order?.deviceName }}
      </div>
    </div>

    <Table
      :columns="columns"
      :data-source="tasks"
      :loading="loading"
      :pagination="pagination"
      :row-key="(record: WorkshopTelemetryTaskDto) => String(record.id)"
      :scroll="{ x: 920 }"
      size="small"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'fileName'">
          <span class="block max-w-[360px] truncate" :title="record.fileName">
            {{ record.fileName }}
          </span>
        </template>
        <template v-else-if="column.key === 'status'">
          <Tag :color="getStatusColor(record.status)">
            {{ getStatusLabel(record.status, record.statusName) }}
          </Tag>
        </template>
        <template v-else-if="column.key === 'fileSize'">
          {{ formatFileSize(record.fileSize) }}
        </template>
        <template v-else-if="column.key === 'createdAt'">
          {{ record.createdAt ? formatToDateTime(record.createdAt) : '-' }}
        </template>
        <template v-else-if="column.key === 'processedAt'">
          {{ record.processedAt ? formatToDateTime(record.processedAt) : '-' }}
        </template>
      </template>
    </Table>
  </Drawer>
</template>

<style scoped></style>
