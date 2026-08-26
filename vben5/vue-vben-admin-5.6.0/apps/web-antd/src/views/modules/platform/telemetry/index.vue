<!-- 遥测任务管理 -->
<script lang="ts" setup>
import type {
  WorkshopTelemetryStatisticsDto,
  WorkshopTelemetryTaskDto,
} from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h, onMounted, ref } from 'vue';

import { Page, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useWorkshopTelemetryApi,
  WorkshopTelemetryStatus,
} from '@abp/core';
import {
  DeleteOutlined,
  DownloadOutlined,
  RedoOutlined,
  UploadOutlined,
} from '@ant-design/icons-vue';
import { Button, message, Modal, Space, Tag, Tooltip } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useFileUpload } from '#/components/file-upload';

defineOptions({
  name: 'WorkshopTelemetryManagement',
});

const { deleteApi, getListApi, getStatisticsApi, retryApi, uploadApi } =
  useWorkshopTelemetryApi();

const statistics = ref<WorkshopTelemetryStatisticsDto>();

const statisticItems = computed(() => [
  {
    label: $t('TestWorkshop.Telemetry:TotalFiles'),
    value: statistics.value?.totalFiles ?? 0,
  },
  {
    label: $t('TestWorkshop.Telemetry:TotalSize'),
    value: formatFileSize(statistics.value?.totalSize ?? 0),
  },
  {
    color: 'orange',
    label: $t('TestWorkshop.Telemetry:Pending'),
    value: statistics.value?.pendingCount ?? 0,
  },
  {
    color: 'blue',
    label: $t('TestWorkshop.Telemetry:Processing'),
    value: statistics.value?.processingCount ?? 0,
  },
  {
    color: 'green',
    label: $t('TestWorkshop.Telemetry:Success'),
    value: statistics.value?.successCount ?? 0,
  },
  {
    color: 'red',
    label: $t('TestWorkshop.Telemetry:Failed'),
    value: statistics.value?.failedCount ?? 0,
  },
  {
    label: $t('TestWorkshop.Telemetry:TotalRecords'),
    value: statistics.value?.totalRecords ?? 0,
  },
]);

const statusOptions = [
  {
    label: $t('TestWorkshop.Telemetry:Pending'),
    value: WorkshopTelemetryStatus.Pending,
  },
  {
    label: $t('TestWorkshop.Telemetry:Processing'),
    value: WorkshopTelemetryStatus.Processing,
  },
  {
    label: $t('TestWorkshop.Telemetry:Success'),
    value: WorkshopTelemetryStatus.Success,
  },
  {
    label: $t('TestWorkshop.Telemetry:Failed'),
    value: WorkshopTelemetryStatus.Failed,
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

const formOptions: VbenFormProps = {
  collapsed: false,
  commonConfig: {
    colon: true,
    componentProps: {
      class: 'w-full',
    },
  },
  schema: [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'fileName',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.Telemetry:FileName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: statusOptions,
      },
      fieldName: 'status',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.Telemetry:Status'),
    },
  ],
  showCollapseButton: false,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<WorkshopTelemetryTaskDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      field: 'fileName',
      minWidth: 220,
      slots: { default: 'fileName' },
      title: $t('TestWorkshop.Telemetry:FileName'),
    },
    {
      align: 'right',
      field: 'fileSize',
      formatter: ({ cellValue }) => formatFileSize(cellValue),
      minWidth: 100,
      title: $t('TestWorkshop.Telemetry:FileSize'),
    },
    {
      align: 'center',
      field: 'status',
      slots: { default: 'status' },
      title: $t('TestWorkshop.Telemetry:Status'),
      width: 100,
    },
    {
      align: 'right',
      field: 'recordCount',
      minWidth: 100,
      title: $t('TestWorkshop.Telemetry:RecordCount'),
    },
    {
      align: 'center',
      field: 'retryCount',
      minWidth: 90,
      title: $t('TestWorkshop.Telemetry:RetryCount'),
    },
    {
      field: 'nextRetryTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '';
      },
      minWidth: 150,
      title: $t('TestWorkshop.Telemetry:NextRetryTime'),
    },
    {
      field: 'error',
      minWidth: 180,
      slots: { default: 'error' },
      title: $t('TestWorkshop.Telemetry:Error'),
    },
    {
      field: 'createdAt',
      formatter: ({ cellValue }) => formatToDateTime(cellValue),
      minWidth: 150,
      title: $t('TestWorkshop.DisplayName:CreatedAt'),
    },
    {
      field: 'processedAt',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '';
      },
      minWidth: 150,
      title: $t('TestWorkshop.Telemetry:ProcessedAt'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      width: 180,
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getListApi({
          fileName: (formValues as Record<string, any>)?.fileName,
          maxResultCount: page.pageSize,
          skipCount: (page.currentPage - 1) * page.pageSize,
          sorting,
          status: (formValues as Record<string, any>)?.status,
        });
      },
      queryAll: async (params) => {
        const { sort } = params;
        const formValues = await gridApi.formApi.getValues();
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getListApi({
          fileName: (formValues as Record<string, any>)?.fileName,
          isPaged: false,
          sorting,
          status: (formValues as Record<string, any>)?.status,
        });
      },
    },
    response: {
      total: 'totalCount',
      list: 'items',
    },
  },
  toolbarConfig: {
    custom: true,
    export: true,
    refresh: true,
    refreshOptions: {
      code: 'query',
    },
    zoom: true,
  },
};

const gridEvents: VxeGridListeners<WorkshopTelemetryTaskDto> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const [TelemetryUploadModal, telemetryUploadModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./TelemetryUploadModal.vue'),
  ),
});

const { UploadModal: CsvUploadModal, openFileUpload: openCsvUpload } =
  useFileUpload();

async function loadStatistics() {
  statistics.value = await getStatisticsApi();
}

function onSimulateUpload() {
  telemetryUploadModalApi.open();
}

function onDownloadTemplate() {
  const template = [
    'DeviceCode,MetricType,Value,Timestamp,TestedDeviceCode,TestedDeviceName',
    'FIVA-001,2,65.50,2026-08-26T10:00:00Z,DUT-A1,水泵A',
  ].join('\n');
  const blob = new Blob([template], {
    type: 'text/csv;charset=utf-8',
  });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = 'telemetry-template.csv';
  link.click();
  URL.revokeObjectURL(url);
}

function onUploadCsv() {
  openCsvUpload({
    accept: '.csv',
    multiple: false,
    title: $t('TestWorkshop.Telemetry:UploadCsv'),
    upload: async (files) => {
      const [file] = files;
      if (file) {
        await uploadApi(file);
      }
    },
  });
}

async function onUploadChange() {
  await Promise.all([gridApi.query(), loadStatistics()]);
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

function onRetry(row: WorkshopTelemetryTaskDto) {
  Modal.confirm({
    centered: true,
    content: $t('AbpUi.AreYouSure'),
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await retryApi(row.id);
        message.success($t('AbpUi.SavedSuccessfully'));
        await Promise.all([gridApi.query(), loadStatistics()]);
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('TestWorkshop.Telemetry:Retry'),
  });
}

function onDelete(row: WorkshopTelemetryTaskDto) {
  Modal.confirm({
    centered: true,
    content: $t('AbpUi.ItemWillBeDeletedMessage'),
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await deleteApi(row.id);
        message.success($t('AbpUi.DeletedSuccessfully'));
        await Promise.all([gridApi.query(), loadStatistics()]);
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('AbpUi.AreYouSure'),
  });
}

onMounted(async () => {
  await loadStatistics();
  await gridApi.query();
});
</script>

<template>
  <Page auto-content-height>
    <Grid>
      <template #table-title>
        <div class="flex min-w-0 items-center gap-2">
          <span class="mr-1 shrink-0 text-[1rem]">
            {{ $t('TestWorkshop.Telemetry:Title') }}
          </span>
          <Space :size="4" wrap>
            <Tag
              v-for="item in statisticItems"
              :key="item.label"
              :color="item.color"
            >
              {{ item.label }}: {{ item.value }}
            </Tag>
          </Space>
        </div>
      </template>
      <template #toolbar-tools>
        <Space :size="8">
          <Button :icon="h(UploadOutlined)" type="primary" @click="onUploadCsv">
            {{ $t('TestWorkshop.Telemetry:UploadCsv') }}
          </Button>
          <Button :icon="h(DownloadOutlined)" @click="onDownloadTemplate">
            {{ $t('TestWorkshop.Telemetry:DownloadTemplate') }}
          </Button>
          <Button
            :icon="h(UploadOutlined)"
            type="primary"
            @click="onSimulateUpload"
          >
            {{ $t('TestWorkshop.Telemetry:SimulateUpload') }}
          </Button>
        </Space>
      </template>
      <template #fileName="{ row }">
        <span class="block max-w-[260px] truncate" :title="row.fileName">
          {{ row.fileName }}
        </span>
      </template>
      <template #status="{ row }">
        <Tag :color="statusColorMap[row.status]">
          {{ statusLabelMap[row.status] ?? row.statusName }}
        </Tag>
      </template>
      <template #error="{ row }">
        <Tooltip v-if="row.error" :title="row.error">
          <span class="block max-w-[180px] truncate text-red-500">
            {{ row.error }}
          </span>
        </Tooltip>
        <span v-else>-</span>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row justify-center">
          <Space>
            <Button
              v-if="row.status === WorkshopTelemetryStatus.Failed"
              :icon="h(RedoOutlined)"
              type="link"
              @click="onRetry(row)"
            >
              {{ $t('TestWorkshop.Telemetry:Retry') }}
            </Button>
            <Button
              :icon="h(DeleteOutlined)"
              danger
              type="link"
              @click="onDelete(row)"
            >
              {{ $t('AbpUi.Delete') }}
            </Button>
          </Space>
        </div>
      </template>
    </Grid>
    <CsvUploadModal @change="onUploadChange" />
    <TelemetryUploadModal @change="onUploadChange" />
  </Page>
</template>

<style scoped></style>
