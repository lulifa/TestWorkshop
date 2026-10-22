<script lang="ts" setup>
import type { WorkshopAlarmDto } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';

import { onMounted, onUnmounted } from 'vue';

import { Page } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useWorkshopAlarmApi,
  WorkshopAlarmStatus,
  WorkshopAlarmType,
} from '@abp/core';
import { Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { WorkshopSelect } from '#/components/workshop-select';

defineOptions({
  name: 'WorkshopAlarmManagement',
});

const { getListApi } = useWorkshopAlarmApi();

const alarmTypeOptions = [
  { label: '阈值报警', value: WorkshopAlarmType.Threshold },
  { label: '关联报警', value: WorkshopAlarmType.Relation },
  { label: '设备报警', value: WorkshopAlarmType.Device },
  { label: '通信报警', value: WorkshopAlarmType.Communication },
];

const statusOptions = [
  { label: '进行中', value: WorkshopAlarmStatus.Active },
  { label: '已恢复', value: WorkshopAlarmStatus.Cleared },
];

const alarmTypeLabels: Record<number, string> = {
  [WorkshopAlarmType.Threshold]: '阈值报警',
  [WorkshopAlarmType.Relation]: '关联报警',
  [WorkshopAlarmType.Device]: '设备报警',
  [WorkshopAlarmType.Communication]: '通信报警',
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
      component: WorkshopSelect,
      componentProps: {
        allowClear: true,
        mode: 'tree',
        placeholder: '全部车间',
      },
      fieldName: 'organizationUnitId',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.DisplayName:OrganizationUnit'),
      modelPropName: 'modelValue',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: alarmTypeOptions,
        placeholder: '全部',
      },
      fieldName: 'alarmType',
      formItemClass: 'col-span-1 items-baseline',
      label: '报警类型',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: statusOptions,
        placeholder: '全部',
      },
      fieldName: 'status',
      formItemClass: 'col-span-1 items-baseline',
      label: '状态',
    },
    {
      component: 'RangePicker',
      componentProps: {
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'timeRange',
      formItemClass: 'col-span-1 items-baseline',
      label: '发生时间',
    },
  ],
  showCollapseButton: false,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<WorkshopAlarmDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      align: 'center',
      field: 'alarmType',
      minWidth: 110,
      slots: { default: 'alarmType' },
      title: '报警类型',
    },
    {
      field: 'deviceCode',
      minWidth: 140,
      title: '测试台',
    },
    {
      field: 'content',
      minWidth: 160,
      title: '报警内容',
    },
    {
      field: 'startedAt',
      formatter: ({ cellValue }) =>
        cellValue ? formatToDateTime(cellValue) : '',
      minWidth: 170,
      title: '发生时间',
    },
    {
      align: 'center',
      field: 'status',
      minWidth: 100,
      slots: { default: 'status' },
      title: '状态',
    },
    {
      field: 'clearedAt',
      formatter: ({ cellValue }) =>
        cellValue ? formatToDateTime(cellValue) : '',
      minWidth: 170,
      title: '恢复时间',
    },
  ],
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues) => {
        const values = formValues as Record<string, any>;
        const timeRange = values?.timeRange as string[] | undefined;
        return await getListApi({
          alarmType: values?.alarmType,
          endTime: timeRange?.[1],
          maxResultCount: page.pageSize,
          organizationUnitId: values?.organizationUnitId,
          skipCount: (page.currentPage - 1) * page.pageSize,
          startTime: timeRange?.[0],
          status: values?.status,
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
    refresh: true,
    refreshOptions: {
      code: 'query',
    },
    zoom: true,
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

let refreshTimer: ReturnType<typeof setInterval> | undefined;

onMounted(() => {
  refreshTimer = setInterval(() => {
    void gridApi.query();
  }, 10_000);
});

onUnmounted(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer);
  }
});
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('page.business.alarms')">
      <template #alarmType="{ row }">
        <Tag color="blue">
          {{ alarmTypeLabels[row.alarmType] ?? row.alarmTypeName }}
        </Tag>
      </template>
      <template #status="{ row }">
        <Tag
          :color="
            row.status === WorkshopAlarmStatus.Active ? 'error' : 'default'
          "
        >
          {{ row.status === WorkshopAlarmStatus.Active ? '进行中' : '已恢复' }}
        </Tag>
      </template>
    </Grid>
  </Page>
</template>
