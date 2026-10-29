<script lang="ts" setup>
import type { WorkshopAlarmDto } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';

import { onActivated, onDeactivated, onMounted, onUnmounted } from 'vue';

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
  {
    label: $t('page.business.alarm.types.threshold'),
    value: WorkshopAlarmType.Threshold,
  },
  {
    label: $t('page.business.alarm.types.relation'),
    value: WorkshopAlarmType.Relation,
  },
  {
    label: $t('page.business.alarm.types.device'),
    value: WorkshopAlarmType.Device,
  },
  {
    label: $t('page.business.alarm.types.communication'),
    value: WorkshopAlarmType.Communication,
  },
];

const statusOptions = [
  {
    label: $t('page.business.alarm.statuses.active'),
    value: WorkshopAlarmStatus.Active,
  },
  {
    label: $t('page.business.alarm.statuses.cleared'),
    value: WorkshopAlarmStatus.Cleared,
  },
];

const alarmTypeLabels: Record<number, string> = {
  [WorkshopAlarmType.Threshold]: $t('page.business.alarm.types.threshold'),
  [WorkshopAlarmType.Relation]: $t('page.business.alarm.types.relation'),
  [WorkshopAlarmType.Device]: $t('page.business.alarm.types.device'),
  [WorkshopAlarmType.Communication]: $t(
    'page.business.alarm.types.communication',
  ),
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
        placeholder: $t('page.business.allWorkshops'),
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
        placeholder: $t('page.business.all'),
      },
      fieldName: 'alarmType',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('page.business.alarm.type'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: statusOptions,
        placeholder: $t('page.business.all'),
      },
      fieldName: 'status',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('page.business.alarm.status'),
    },
    {
      component: 'RangePicker',
      componentProps: {
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'timeRange',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('page.business.alarm.startedAt'),
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
      title: $t('page.business.alarm.type'),
    },
    {
      field: 'deviceCode',
      minWidth: 140,
      title: $t('page.business.alarm.device'),
    },
    {
      field: 'content',
      minWidth: 160,
      title: $t('page.business.alarm.content'),
    },
    {
      field: 'startedAt',
      formatter: ({ cellValue }) =>
        cellValue ? formatToDateTime(cellValue) : '',
      minWidth: 170,
      title: $t('page.business.alarm.startedAt'),
    },
    {
      align: 'center',
      field: 'status',
      minWidth: 100,
      slots: { default: 'status' },
      title: $t('page.business.alarm.status'),
    },
    {
      field: 'clearedAt',
      formatter: ({ cellValue }) =>
        cellValue ? formatToDateTime(cellValue) : '',
      minWidth: 170,
      title: $t('page.business.alarm.clearedAt'),
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
let hasDeactivated = false;

function startAutoRefresh() {
  stopAutoRefresh();
  refreshTimer = setInterval(() => {
    void gridApi.query();
  }, 10_000);
}

function stopAutoRefresh() {
  if (refreshTimer) {
    clearInterval(refreshTimer);
    refreshTimer = undefined;
  }
}

onMounted(() => {
  startAutoRefresh();
});

onActivated(() => {
  if (hasDeactivated) {
    void gridApi.query();
    startAutoRefresh();
  }
});

onDeactivated(() => {
  hasDeactivated = true;
  stopAutoRefresh();
});
onUnmounted(stopAutoRefresh);
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
          {{
            row.status === WorkshopAlarmStatus.Active
              ? $t('page.business.alarm.statuses.active')
              : $t('page.business.alarm.statuses.cleared')
          }}
        </Tag>
      </template>
    </Grid>
  </Page>
</template>
