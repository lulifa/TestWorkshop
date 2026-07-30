<script setup lang="ts">
import type { AuditLogDto } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { defineAsyncComponent, h } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  AuditLogPermissions,
  formatToDateTime,
  useAuditlogs,
  useAuditLogsApi,
} from '@abp/core';
import { EditOutlined } from '@ant-design/icons-vue';
import { Button, Space, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

import { httpMethodOptions, httpStatusCodeOptions } from './mapping';

defineOptions({
  name: 'AuditLogTable',
});
const { getPagedListApi } = useAuditLogsApi();

const formOptions: VbenFormProps = {
  // 默认展开
  collapsed: true,
  collapsedRows: 2,
  fieldMappingTime: [
    [
      'executionTime',
      ['startTime', 'endTime'],
      (value) => formatToDateTime(value),
    ],
  ],
  schema: [
    {
      component: 'RangePicker',
      componentProps: {
        showTime: true,
      },
      fieldName: 'executionTime',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('TestWorkshop.DisplayName:ExecutionTime'),
    },
    {
      component: 'Input',
      fieldName: 'url',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('TestWorkshop.DisplayName:RequestUrl'),
    },
    {
      component: 'Select',
      componentProps: {
        options: httpStatusCodeOptions,
      },
      fieldName: 'httpStatusCode',
      label: $t('TestWorkshop.DisplayName:HttpStatusCode'),
    },
    {
      component: 'Select',
      componentProps: {
        options: httpMethodOptions,
      },
      fieldName: 'httpMethod',
      label: $t('TestWorkshop.DisplayName:HttpMethod'),
    },
    {
      component: 'Input',
      fieldName: 'applicationName',
      label: $t('TestWorkshop.DisplayName:ApplicationName'),
    },
    {
      component: 'Input',
      fieldName: 'userName',
      label: $t('TestWorkshop.DisplayName:UserName'),
    },
    {
      component: 'Input',
      fieldName: 'clientId',
      label: $t('TestWorkshop.DisplayName:ClientId'),
    },
    {
      component: 'Input',
      fieldName: 'clientIpAddress',
      label: $t('TestWorkshop.DisplayName:ClientIpAddress'),
    },
    {
      component: 'InputNumber',
      fieldName: 'minExecutionDuration',
      label: $t('TestWorkshop.DisplayName:MinExecutionDuration'),
      labelWidth: 150,
    },
    {
      component: 'InputNumber',
      fieldName: 'maxExecutionDuration',
      label: $t('TestWorkshop.DisplayName:MaxExecutionDuration'),
      labelWidth: 150,
    },
    {
      component: 'Input',
      fieldName: 'correlationId',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('TestWorkshop.DisplayName:CorrelationId'),
    },
    {
      component: 'Checkbox',
      componentProps: {
        render: () => {
          return h('span', $t('TestWorkshop.DisplayName:HasException'));
        },
      },
      fieldName: 'hasException',
      label: $t('TestWorkshop.DisplayName:HasException'),
    },
  ],
  // 控制表单是否显示折叠按钮
  showCollapseButton: true,
  // 按下回车时是否提交表单
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<AuditLogDto> = {
  columns: [
    {
      align: 'left',
      field: 'url',
      slots: { default: 'url' },
      sortable: true,
      title: $t('TestWorkshop.DisplayName:RequestUrl'),
      width: 500,
    },
    {
      align: 'left',
      field: 'userName',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:UserName'),
      width: 120,
    },
    {
      align: 'left',
      field: 'executionTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : cellValue;
      },
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ExecutionTime'),
      width: 150,
    },
    {
      align: 'left',
      field: 'executionDuration',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ExecutionDuration'),
      width: 140,
    },
    {
      align: 'left',
      field: 'clientId',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ClientId'),
      width: 150,
    },
    {
      align: 'left',
      field: 'clientIpAddress',
      slots: { default: 'clientIpAddress' },
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ClientIpAddress'),
      width: 150,
    },
    {
      align: 'left',
      field: 'applicationName',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ApplicationName'),
      width: 160,
    },
    {
      align: 'left',
      field: 'correlationId',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:CorrelationId'),
      width: 160,
    },
    {
      align: 'left',
      field: 'tenantName',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:TenantName'),
      width: 100,
    },
    {
      align: 'left',
      field: 'browserInfo',
      sortable: true,
      title: $t('TestWorkshop.DisplayName:BrowserInfo'),
      width: 300,
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      width: 220,
    },
  ],
  exportConfig: {},
  keepSource: true,
  height: 'auto',
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getPagedListApi({
          sorting,
          maxResultCount: page.pageSize,
          skipCount: (page.currentPage - 1) * page.pageSize,
          ...formValues,
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

const gridEvents: VxeGridListeners<AuditLogDto> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const { getHttpMethodColor, getHttpStatusCodeColor } = useAuditlogs();
const [AuditLogDrawer, logDrawerApi] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('./AuditLogDrawer.vue'),
  ),
});

function onUpdate(row: AuditLogDto) {
  logDrawerApi.setData(row);
  logDrawerApi.open();
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('TestWorkshop.DisplayName:AuditLog')">
      <template #clientIpAddress="{ row }">
        <Tag v-if="row.extraProperties?.Location" color="blue">
          {{ row.extraProperties?.Location }}
        </Tag>
        <span>{{ row.clientIpAddress }}</span>
      </template>
      <template #url="{ row }">
        <div class="flex flex-row">
          <Tag
            :color="getHttpStatusCodeColor(row.httpStatusCode)"
            class="cursor-pointer"
          >
            {{ row.httpStatusCode }}
          </Tag>
          <Tag
            :color="getHttpMethodColor(row.httpMethod)"
            class="ml-px cursor-pointer"
          >
            {{ row.httpMethod }}
          </Tag>
          <a class="link" href="javaScript:void(0);">{{ row.url }} </a>
        </div>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row justify-center">
          <Space>
            <Button
              :icon="h(EditOutlined)"
              block
              type="link"
              v-access:code="[AuditLogPermissions.Default]"
              @click="onUpdate(row)"
            >
              {{ $t('TestWorkshop.DisplayName:ShowLogDialog') }}
            </Button>
          </Space>
        </div>
      </template>
    </Grid>
    <AuditLogDrawer />
  </Page>
</template>

<style lang="scss" scoped></style>
