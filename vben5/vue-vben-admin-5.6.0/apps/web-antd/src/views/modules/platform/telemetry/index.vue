<!-- 遥测任务管理 -->
<script lang="ts" setup>
import type {
  WorkshopTelemetryStatisticsDto,
  WorkshopTelemetryTaskDto,
} from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h, onMounted, ref } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { downloadFileFromBlob } from '@vben/utils';

import {
  formatToDateTime,
  useFileApi,
  useWorkshopTelemetryApi,
  WorkshopTelemetryStatus,
} from '@abp/core';
import {
  DeleteOutlined,
  DownloadOutlined,
  EyeOutlined,
  InfoCircleOutlined,
  RedoOutlined,
  UploadOutlined,
} from '@ant-design/icons-vue';
import { Button, message, Modal, Space, Tag, Tooltip } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useFilePreview } from '#/components/file-preview';

defineOptions({
  name: 'WorkshopTelemetryManagement',
});

const { deleteApi, deleteManyApi, getListApi, getStatisticsApi, retryApi } =
  useWorkshopTelemetryApi();
const { getApi: getFileApi } = useFileApi();

const statistics = ref<WorkshopTelemetryStatisticsDto>();
const selectedRows = ref<WorkshopTelemetryTaskDto[]>([]);

// 界面上“状态”列的虚拟状态：已替换（被后一次上传顶掉的历史任务，底层状态其实还是成功）
const ReplacedStatusValue = 4;

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
    color: 'default',
    label: $t('TestWorkshop.Telemetry:Replaced'),
    value: statistics.value?.replacedCount ?? 0,
  },
  {
    color: 'volcano',
    label: $t('page.business.unmatchedOrder'),
    value: statistics.value?.unmatchedOrderCount ?? 0,
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
  {
    label: $t('TestWorkshop.Telemetry:Replaced'),
    value: ReplacedStatusValue,
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
  fieldMappingTime: [
    [
      'createdAtRange',
      ['startTime', 'endTime'],
      (value) => formatToDateTime(value),
    ],
  ],
  schema: [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'orderNo',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.WorkshopOrder:OrderNo'),
    },
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
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'testedDeviceCode',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('page.business.testedDevice'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          {
            label: $t('page.business.hasOrderYes'),
            value: true,
          },
          {
            label: $t('page.business.hasOrderNo'),
            value: false,
          },
        ],
      },
      fieldName: 'hasOrder',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('page.business.hasOrder'),
    },
    {
      component: 'RangePicker',
      componentProps: {
        showTime: true,
      },
      fieldName: 'createdAtRange',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('TestWorkshop.Telemetry:TimeRange'),
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
      type: 'checkbox',
      width: 50,
    },
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      align: 'center',
      field: 'orderNo',
      minWidth: 170,
      slots: { default: 'orderNo' },
      title: $t('TestWorkshop.WorkshopOrder:OrderNo'),
    },
    {
      align: 'center',
      field: 'status',
      slots: { default: 'status' },
      title: $t('TestWorkshop.Telemetry:Status'),
      width: 110,
    },
    {
      align: 'center',
      field: 'id',
      minWidth: 100,
      slots: { default: 'taskId' },
      title: $t('page.business.taskIdCurrent'),
    },
    {
      align: 'left',
      field: 'fileName',
      minWidth: 320,
      slots: { default: 'fileName' },
      title: $t('TestWorkshop.Telemetry:FileName'),
    },
    {
      field: 'deviceCode',
      minWidth: 140,
      slots: { default: 'deviceCode' },
      title: $t('TestWorkshop.Telemetry:DeviceCode'),
    },
    {
      align: 'left',
      field: 'testedDeviceCode',
      minWidth: 180,
      slots: { default: 'testedDevice' },
      title: $t('page.business.testedDevice'),
    },
    {
      align: 'center',
      field: 'testBy',
      minWidth: 110,
      slots: { default: 'testBy' },
      title: $t('TestWorkshop.Telemetry:TestBy'),
    },
    {
      field: 'shipName',
      minWidth: 150,
      slots: { default: 'shipName' },
      title: $t('TestWorkshop.Telemetry:ShipName'),
    },
    {
      align: 'center',
      field: 'channelType',
      minWidth: 140,
      slots: { default: 'channelType' },
      title: $t('TestWorkshop.Telemetry:ChannelType'),
    },
    {
      align: 'right',
      field: 'fileSize',
      formatter: ({ cellValue }) => formatFileSize(cellValue),
      minWidth: 100,
      title: $t('TestWorkshop.Telemetry:FileSize'),
    },
    {
      field: 'fileTime',
      minWidth: 170,
      slots: { default: 'fileTime' },
      title: $t('TestWorkshop.Telemetry:FileTime'),
    },
    {
      field: 'createdAt',
      formatter: ({ cellValue }) => formatToDateTime(cellValue),
      minWidth: 150,
      title: $t('TestWorkshop.DisplayName:CreatedAt'),
    },
    {
      align: 'right',
      field: 'recordCount',
      minWidth: 100,
      title: $t('TestWorkshop.Telemetry:SampleCount'),
    },
    {
      field: 'error',
      minWidth: 180,
      slots: { default: 'error' },
      title: $t('TestWorkshop.Telemetry:Error'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      width: 340,
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  rowClassName: ({ row }) => (row.orderId ? '' : 'unmatched-order-row'),
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        const values = formValues as Record<string, any>;
        return await getListApi({
          endTime: values?.endTime,
          fileName: values?.fileName,
          hasOrder: values?.hasOrder,
          maxResultCount: page.pageSize,
          orderNo: values?.orderNo,
          skipCount: (page.currentPage - 1) * page.pageSize,
          sorting,
          startTime: values?.startTime,
          status: values?.status,
          testedDeviceCode: values?.testedDeviceCode,
        });
      },
      queryAll: async (params) => {
        const { sort } = params;
        const formValues = await gridApi.formApi.getValues();
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        const values = formValues as Record<string, any>;
        return await getListApi({
          endTime: values?.endTime,
          fileName: values?.fileName,
          hasOrder: values?.hasOrder,
          isPaged: false,
          orderNo: values?.orderNo,
          sorting,
          startTime: values?.startTime,
          status: values?.status,
          testedDeviceCode: values?.testedDeviceCode,
        });
      },
      querySuccess: () => {
        selectedRows.value = [];
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
  checkboxAll: syncSelectedRows,
  checkboxChange: syncSelectedRows,
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

function syncSelectedRows() {
  selectedRows.value = (gridApi.grid.getCheckboxRecords() ??
    []) as WorkshopTelemetryTaskDto[];
}

function getTelemetryExtraValue(
  row: WorkshopTelemetryTaskDto,
  propertyName: string,
) {
  return row.extraProperties?.[`TelemetryInput.${propertyName}`] ?? null;
}

function getTestedDeviceLabel(row: WorkshopTelemetryTaskDto) {
  const code =
    row.testedDeviceCode ??
    getTelemetryExtraValue(row, 'TestInfo.TestedDeviceCode');
  if (!code) {
    return undefined;
  }

  const name =
    row.testedDeviceName ??
    getTelemetryExtraValue(row, 'TestInfo.TestedDeviceName');
  return name ? `${code} · ${name}` : String(code);
}

const [TelemetryUploadModal, telemetryUploadModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./TelemetryUploadModal.vue'),
  ),
});

const [TelemetryDetailDrawer, telemetryDetailDrawerApi] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('./TelemetryDetailDrawer.vue'),
  ),
});

const { PreviewModal, openFilePreview } = useFilePreview();

async function loadStatistics() {
  statistics.value = await getStatisticsApi();
}

function onDownloadTemplate() {
  const template = ['0,20', '1,20', '2,20', '3,20'].join('\n');
  const blob = new Blob([template], {
    type: 'text/csv;charset=utf-8',
  });
  downloadFileFromBlob({
    fileName: 'telemetry-template.csv',
    source: blob,
  });
}

function onUploadCsv() {
  telemetryUploadModalApi.open();
}

async function onPreviewCsv(row: WorkshopTelemetryTaskDto) {
  const file = await getFileApi(row.fileObjectId);
  openFilePreview(file);
}

function onDetail(row: WorkshopTelemetryTaskDto) {
  telemetryDetailDrawerApi.setData(row);
  telemetryDetailDrawerApi.open();
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

function onBatchDelete() {
  if (selectedRows.value.length === 0) {
    return;
  }

  Modal.confirm({
    centered: true,
    content: $t('TestWorkshop.Telemetry:BatchDeleteConfirm', [
      selectedRows.value.length,
    ]),
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await deleteManyApi(selectedRows.value.map((item) => item.id));
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
          <Button
            v-if="selectedRows.length > 0"
            :icon="h(DeleteOutlined)"
            danger
            @click="onBatchDelete"
          >
            {{ $t('TestWorkshop.Telemetry:BatchDelete') }}
          </Button>
          <Button :icon="h(UploadOutlined)" type="primary" @click="onUploadCsv">
            {{ $t('TestWorkshop.Telemetry:UploadCsv') }}
          </Button>
          <Button :icon="h(DownloadOutlined)" @click="onDownloadTemplate">
            {{ $t('TestWorkshop.Telemetry:DownloadTemplate') }}
          </Button>
        </Space>
      </template>
      <template #fileName="{ row }">
        <span class="block max-w-[440px] truncate" :title="row.fileName">
          {{ row.fileName }}
        </span>
      </template>
      <template #deviceCode="{ row }">
        {{ getTelemetryExtraValue(row, 'DeviceCode') || '-' }}
      </template>
      <template #testedDevice="{ row }">
        <span class="block truncate" :title="getTestedDeviceLabel(row)">
          {{ getTestedDeviceLabel(row) ?? '-' }}
        </span>
      </template>
      <template #orderNo="{ row }">
        <Tag v-if="row.orderNo" color="blue">{{ row.orderNo }}</Tag>
        <Tag v-else color="error">
          {{ $t('page.business.unmatchedOrder') }}
        </Tag>
      </template>
      <template #taskId="{ row }">
        <span class="font-mono text-xs">{{ row.id }}</span>
      </template>
      <template #testBy="{ row }">
        {{ getTelemetryExtraValue(row, 'TestInfo.TestBy') || '-' }}
      </template>
      <template #shipName="{ row }">
        <span
          class="block truncate"
          :title="getTelemetryExtraValue(row, 'TestInfo.ShipName') || ''"
        >
          {{ getTelemetryExtraValue(row, 'TestInfo.ShipName') || '-' }}
        </span>
      </template>
      <template #channelType="{ row }">
        <Tag v-if="getTelemetryExtraValue(row, 'ChannelType')" color="blue">
          {{ getTelemetryExtraValue(row, 'ChannelType') }}
        </Tag>
        <span v-else>-</span>
      </template>
      <template #fileTime="{ row }">
        {{ getTelemetryExtraValue(row, 'FileTime') || '-' }}
      </template>
      <template #status="{ row }">
        <Tag v-if="row.replacedByTaskId" color="default">
          {{ $t('TestWorkshop.Telemetry:Replaced') }}
        </Tag>
        <Tag v-else :color="statusColorMap[row.status]">
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
          <Space :size="4">
            <Button
              :icon="h(InfoCircleOutlined)"
              type="link"
              @click="onDetail(row)"
            >
              {{ $t('TestWorkshop.Telemetry:Parameters') }}
            </Button>
            <Button
              :icon="h(EyeOutlined)"
              type="link"
              @click="onPreviewCsv(row)"
            >
              {{ $t('TestWorkshop.FileManager:Preview') }}
            </Button>
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
    <PreviewModal />
    <TelemetryDetailDrawer />
    <TelemetryUploadModal @change="onUploadChange" />
  </Page>
</template>

<style scoped>
:deep(.vxe-body--row.unmatched-order-row > td) {
  background-color: rgb(254 226 226 / 70%) !important;
}

:global(html.dark) :deep(.vxe-body--row.unmatched-order-row > td) {
  background-color: rgb(127 29 29 / 40%) !important;
}
</style>
