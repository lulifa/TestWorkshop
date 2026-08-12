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
import { DeleteOutlined, SendOutlined } from '@ant-design/icons-vue';
import { Button, message, Space, Tag, Tooltip } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'BroadcastManagement',
});

const { deleteApi, getNotificationListApi } = useNotificationsApi();

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

const readOptions = [
  {
    label: $t('TestWorkshop.Notification:Read'),
    value: true,
  },
  {
    label: $t('TestWorkshop.Notification:Unread'),
    value: false,
  },
];

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
      fieldName: 'title',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.DisplayName:Subject'),
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'content',
      formItemClass: 'col-span-1 items-baseline',
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
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: readOptions,
      },
      fieldName: 'read',
      label: $t('TestWorkshop.Notification:ReadState'),
    },
  ],
  showCollapseButton: false,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

function buildParams(values: Record<string, any>, extra?: Record<string, any>) {
  const params: Record<string, any> = {
    ...values,
    ...extra,
    messageType: NotificationMessageType.BroadCast,
  };
  if (params.read !== undefined && params.read !== null) {
    params.read = params.read === 'true' || params.read === true;
  }
  return params;
}

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
      align: 'center',
      field: 'read',
      slots: { default: 'read' },
      title: $t('TestWorkshop.Notification:ReadState'),
      width: 80,
    },
    {
      field: 'readTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '';
      },
      minWidth: 130,
      title: $t('TestWorkshop.Notification:ReadTime'),
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
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      width: 70,
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getNotificationListApi(
          buildParams(formValues as Record<string, any>, {
            maxResultCount: page.pageSize,
            skipCount: (page.currentPage - 1) * page.pageSize,
            sorting,
          }),
        );
      },
      queryAll: async ({ sort }) => {
        const formValues = await gridApi.formApi.getValues();
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getNotificationListApi(
          buildParams(formValues, {
            isPaged: false,
            sorting,
          }),
        );
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

async function onDelete(row: NotificationOutput) {
  await deleteApi({ id: row.id });
  message.success($t('AbpUi.SavedSuccessfully'));
  await gridApi.query();
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
      <template #read="{ row }">
        <Tag :color="row.read ? 'success' : 'warning'">
          {{
            row.read
              ? $t('TestWorkshop.Notification:Read')
              : $t('TestWorkshop.Notification:Unread')
          }}
        </Tag>
      </template>
      <template #action="{ row }">
        <Space :size="4">
          <Tooltip :title="$t('common.delete')">
            <Button
              :icon="h(DeleteOutlined)"
              danger
              type="link"
              @click="onDelete(row)"
            />
          </Tooltip>
        </Space>
      </template>
    </Grid>

    <SendModal mode="broadcast" @change="() => gridApi.query()" />
    <DetailDrawer />
  </Page>
</template>
