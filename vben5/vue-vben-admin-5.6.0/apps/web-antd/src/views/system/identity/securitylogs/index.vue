<script lang="ts" setup>
import type { IdentitySecurityLogOutput } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { Page } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useSecurityLogsApi } from '@abp/core';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'SecurityLogsTable',
});

const { getPagedListApi } = useSecurityLogsApi();

const formOptions: VbenFormProps = {
  // 默认展开
  collapsed: true,
  collapsedRows: 2, // 新增：折叠时显示2行，避免表单太长
  // 所有表单项共用，可单独在表单内覆盖
  commonConfig: {
    // 在label后显示一个冒号
    colon: true,
    // 所有表单项
    componentProps: {
      class: 'w-full',
    },
  },
  schema: [
    {
      component: 'DatePicker',
      fieldName: 'startTime',
      label: $t('TestWorkshop.DisplayName:StartTime'),
      formItemClass: 'col-span-1',
      componentProps: {
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD',
      },
    },
    {
      component: 'DatePicker',
      fieldName: 'endTime',
      label: $t('TestWorkshop.DisplayName:EndTime'),
      formItemClass: 'col-span-1',
      componentProps: {
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD',
      },
    },
    {
      component: 'Input',
      fieldName: 'applicationName',
      label: $t('TestWorkshop.DisplayName:ApplicationName'),
      formItemClass: 'col-span-1',
      componentProps: {
        allowClear: true,
      },
    },
    {
      component: 'Input',
      fieldName: 'userName',
      label: $t('TestWorkshop.DisplayName:UserName'),
      formItemClass: 'col-span-1',
      componentProps: {
        allowClear: true,
      },
    },
  ],
  // 控制表单是否显示折叠按钮
  showCollapseButton: true,
  // 按下回车时是否提交表单
  submitOnEnter: true,
};

const gridOptions: VxeGridProps<IdentitySecurityLogOutput> = {
  columns: [
    {
      align: 'center',
      fixed: 'left',
      type: 'seq',
      width: 80,
    },
    {
      align: 'left',
      field: 'applicationName',
      fixed: 'left',
      minWidth: 200,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ApplicationName'),
    },
    {
      align: 'left',
      field: 'identity',
      minWidth: 120,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:Identity'),
    },
    {
      align: 'center',
      field: 'action',
      minWidth: 150,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:Action'),
    },
    {
      align: 'center',
      field: 'userName',
      minWidth: 100,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:UserName'),
    },
    {
      align: 'left',
      field: 'clientIpAddress',
      minWidth: 180,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ClientIpAddress'),
    },
    {
      align: 'left',
      field: 'browserInfo',
      minWidth: 200,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:BrowserInfo'),
    },
    {
      align: 'left',
      field: 'creationTime',
      minWidth: 180,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:CreationTime'),
    },
    {
      align: 'center',
      field: 'correlationId',
      minWidth: 240,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:CorrelationId'),
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
const gridEvents: VxeGridListeners<IdentitySecurityLogOutput> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
  gridEvents,
});
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('TestWorkshop.DisplayName:SecurityLog')" />

    <LayoutModal @change="() => gridApi.query()" />
  </Page>
</template>

<style scoped></style>
