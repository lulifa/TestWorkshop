<script lang="ts" setup>
import type { NotificationOutput } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { defineAsyncComponent, h } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  NotificationMessageLevel,
  NotificationMessageType,
  useNotificationsApi,
} from '@abp/core';
import { SendOutlined } from '@ant-design/icons-vue';
import { Button, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'BroadcastManagement',
});

const { getNotificationListApi } = useNotificationsApi();

const levelColorMap: Record<NotificationMessageLevel, string> = {
  [NotificationMessageLevel.Error]: 'error',
  [NotificationMessageLevel.Warning]: 'warning',
  [NotificationMessageLevel.Information]: 'processing',
};
const levelLabelMap: Record<NotificationMessageLevel, string> = {
  [NotificationMessageLevel.Error]: $t('TestWorkshop.NotificationLevel:Error'),
  [NotificationMessageLevel.Warning]: $t(
    'TestWorkshop.NotificationLevel:Warning',
  ),
  [NotificationMessageLevel.Information]: $t(
    'TestWorkshop.NotificationLevel:Information',
  ),
};

const levelOptions = [
  {
    label: $t('TestWorkshop.NotificationLevel:Warning'),
    value: NotificationMessageLevel.Warning,
  },
  {
    label: $t('TestWorkshop.NotificationLevel:Information'),
    value: NotificationMessageLevel.Information,
  },
  {
    label: $t('TestWorkshop.NotificationLevel:Error'),
    value: NotificationMessageLevel.Error,
  },
];

const formOptions: VbenFormProps = {
  collapsed: true,
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
      fieldName: 'title',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('TestWorkshop.DisplayName:Subject'),
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'content',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('TestWorkshop.DisplayName:Content'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: levelOptions,
      },
      fieldName: 'messageLevel',
      label: $t('TestWorkshop.Notification:Level'),
    },
  ],
  showCollapseButton: true,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<NotificationOutput> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      field: 'title',
      minWidth: 150,
      slots: { default: 'title' },
      title: $t('TestWorkshop.DisplayName:Subject'),
    },
    {
      field: 'content',
      minWidth: 200,
      slots: { default: 'content' },
      title: $t('TestWorkshop.DisplayName:Content'),
    },
    {
      align: 'center',
      field: 'messageLevel',
      slots: { default: 'level' },
      title: $t('TestWorkshop.Notification:Level'),
      width: 90,
    },
    {
      field: 'senderUserName',
      minWidth: 90,
      title: $t('TestWorkshop.DisplayName:From'),
    },
    {
      field: 'creationTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '';
      },
      minWidth: 130,
      title: $t('TestWorkshop.DisplayName:SendTime'),
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getNotificationListApi({
          ...(formValues as Record<string, any>),
          maxResultCount: page.pageSize,
          messageType: NotificationMessageType.BroadCast,
          skipCount: (page.currentPage - 1) * page.pageSize,
          sorting,
        });
      },
      queryAll: async ({ sort }) => {
        const formValues = await gridApi.formApi.getValues();
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getNotificationListApi({
          ...formValues,
          isPaged: false,
          messageType: NotificationMessageType.BroadCast,
          sorting,
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

const gridEvents: VxeGridListeners<NotificationOutput> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const [SendModal, sendModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('../components/SendNotificationModal.vue'),
  ),
});

const [DetailDrawer, detailDrawerApi] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('../components/NotificationDetailDrawer.vue'),
  ),
});

function levelColor(level: NotificationMessageLevel) {
  return levelColorMap[level];
}

function levelLabel(level: NotificationMessageLevel) {
  return levelLabelMap[level];
}

function onPreview(row: NotificationOutput) {
  detailDrawerApi.setData(row);
  detailDrawerApi.open();
}

function onSend() {
  sendModalApi.open();
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('TestWorkshop.Notification:BroadcastManagement')">
      <template #toolbar-tools>
        <Button :icon="h(SendOutlined)" type="primary" @click="onSend">
          {{ $t('TestWorkshop.Notification:SendBroadcast') }}
        </Button>
      </template>
      <template #title="{ row }">
        <Button
          class="h-auto p-0 text-left"
          type="link"
          :title="$t('TestWorkshop.Notification:Detail')"
          @click="onPreview(row)"
        >
          {{ row.title }}
        </Button>
      </template>
      <template #content="{ row }">
        <span class="block max-w-[420px] truncate" :title="row.content">
          {{ row.content }}
        </span>
      </template>
      <template #level="{ row }">
        <Tag :color="levelColor(row.messageLevel)">
          {{ levelLabel(row.messageLevel) }}
        </Tag>
      </template>
    </Grid>

    <SendModal mode="broadcast" @change="() => gridApi.query()" />
    <DetailDrawer />
  </Page>
</template>
