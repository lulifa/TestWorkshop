<script lang="ts" setup>
import type {
  WorkshopOrderDto,
  WorkshopOrderReportFetchRecordDto,
} from '@abp/core';
import type { TableColumnsType } from 'ant-design-vue';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useWorkshopOrderApi,
  WorkshopTestReportFetchRecordStatus,
  WorkshopTestReportFetchTriggerType,
} from '@abp/core';
import { Empty, Table, Tag } from 'ant-design-vue';

const { getTestReportFetchRecordsApi } = useWorkshopOrderApi();

const order = ref<WorkshopOrderDto>();
const records = ref<WorkshopOrderReportFetchRecordDto[]>([]);
const loading = ref(false);

const triggerLabelMap: Record<WorkshopTestReportFetchTriggerType, string> = {
  [WorkshopTestReportFetchTriggerType.Automatic]: $t(
    'TestWorkshop.WorkshopOrder:FetchTriggerAutomatic',
  ),
  [WorkshopTestReportFetchTriggerType.Manual]: $t(
    'TestWorkshop.WorkshopOrder:FetchTriggerManual',
  ),
};

const statusLabelMap: Record<WorkshopTestReportFetchRecordStatus, string> = {
  [WorkshopTestReportFetchRecordStatus.Pending]: $t(
    'TestWorkshop.WorkshopOrder:FetchRecordPending',
  ),
  [WorkshopTestReportFetchRecordStatus.Processing]: $t(
    'TestWorkshop.WorkshopOrder:FetchRecordProcessing',
  ),
  [WorkshopTestReportFetchRecordStatus.NoReport]: $t(
    'TestWorkshop.WorkshopOrder:FetchRecordNoReport',
  ),
  [WorkshopTestReportFetchRecordStatus.Succeeded]: $t(
    'TestWorkshop.WorkshopOrder:FetchRecordSucceeded',
  ),
  [WorkshopTestReportFetchRecordStatus.Failed]: $t(
    'TestWorkshop.WorkshopOrder:FetchRecordFailed',
  ),
};

const statusColorMap: Record<WorkshopTestReportFetchRecordStatus, string> = {
  [WorkshopTestReportFetchRecordStatus.Pending]: 'default',
  [WorkshopTestReportFetchRecordStatus.Processing]: 'processing',
  [WorkshopTestReportFetchRecordStatus.NoReport]: 'warning',
  [WorkshopTestReportFetchRecordStatus.Succeeded]: 'success',
  [WorkshopTestReportFetchRecordStatus.Failed]: 'error',
};

const columns = computed<TableColumnsType>(() => [
  {
    dataIndex: 'attemptNo',
    title: $t('TestWorkshop.WorkshopOrder:FetchAttemptNo'),
    width: 90,
  },
  {
    dataIndex: 'triggerType',
    title: $t('TestWorkshop.WorkshopOrder:FetchTrigger'),
    width: 100,
    customRender: ({ record }) =>
      triggerLabelMap[record.triggerType as WorkshopTestReportFetchTriggerType],
  },
  {
    dataIndex: 'status',
    title: $t('TestWorkshop.WorkshopOrder:FetchStatus'),
    width: 110,
  },
  {
    dataIndex: 'scheduledAt',
    title: $t('TestWorkshop.WorkshopOrder:FetchScheduledAt'),
    width: 170,
    customRender: ({ text }) => (text ? formatToDateTime(text) : '-'),
  },
  {
    dataIndex: 'startedAt',
    title: $t('TestWorkshop.WorkshopOrder:FetchStartedAt'),
    width: 170,
    customRender: ({ text }) => (text ? formatToDateTime(text) : '-'),
  },
  {
    dataIndex: 'completedAt',
    title: $t('TestWorkshop.WorkshopOrder:FetchCompletedAt'),
    width: 170,
    customRender: ({ text }) => (text ? formatToDateTime(text) : '-'),
  },
  {
    dataIndex: 'fileCount',
    title: $t('TestWorkshop.WorkshopOrder:FetchFileCount'),
    width: 90,
  },
  {
    dataIndex: 'errorMessage',
    ellipsis: true,
    title: $t('TestWorkshop.WorkshopOrder:FetchError'),
  },
]);

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[92vw] max-w-[1300px]',
  contentClass: 'p-4',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      return;
    }

    order.value = drawerApi.getData<WorkshopOrderDto>();
    await loadRecords();
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.WorkshopOrder:FetchRecords'),
});

async function loadRecords() {
  if (!order.value?.id) {
    return;
  }

  loading.value = true;
  try {
    records.value = await getTestReportFetchRecordsApi(order.value.id);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <Drawer>
    <div class="mb-4 text-base font-semibold">{{ order?.orderNo }}</div>
    <Table
      v-if="records.length > 0"
      :columns="columns"
      :data-source="records"
      :loading="loading"
      :pagination="false"
      :scroll="{ x: 1200, y: 'calc(100vh - 260px)' }"
      row-key="id"
      size="small"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'status'">
          <Tag
            :color="
              statusColorMap[
                record.status as WorkshopTestReportFetchRecordStatus
              ]
            "
          >
            {{
              statusLabelMap[
                record.status as WorkshopTestReportFetchRecordStatus
              ]
            }}
          </Tag>
        </template>
      </template>
    </Table>
    <Empty
      v-else-if="!loading"
      class="py-20"
      :description="$t('TestWorkshop.WorkshopOrder:FetchRecordsEmpty')"
    />
  </Drawer>
</template>

<style scoped></style>
