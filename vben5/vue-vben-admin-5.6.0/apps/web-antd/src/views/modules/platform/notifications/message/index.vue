<script lang="ts" setup>
import type { NotificationOutput } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { defineAsyncComponent, h, ref } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  NotificationMessageLevel,
  NotificationMessageType,
  NotificationPermissions,
  useAuthorization,
  useNotificationsApi,
} from '@abp/core';
import {
  DeleteOutlined,
  ReadOutlined,
  SendOutlined,
} from '@ant-design/icons-vue';
import { Button, message, Modal, Space, Tag, Tooltip } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'MessageManagement',
});

const { isGranted } = useAuthorization();
const { deleteApi, getNotificationListApi, setBatchReadApi, setReadApi } =
  useNotificationsApi();

const selectedRows = ref<NotificationOutput[]>([]);

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
  showCollapseButton: true,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

function buildParams(values: Record<string, any>, extra?: Record<string, any>) {
  const params: Record<string, any> = {
    ...values,
    ...extra,
    messageType: NotificationMessageType.Common,
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
      type: 'checkbox',
      width: 50,
    },
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
      field: 'receiveUserName',
      minWidth: 90,
      title: $t('TestWorkshop.Notification:Receiver'),
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
      width: 120,
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        const params = buildParams(formValues as Record<string, any>, {
          maxResultCount: page.pageSize,
          skipCount: (page.currentPage - 1) * page.pageSize,
          sorting,
        });
        return await getNotificationListApi(params);
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

const gridEvents: VxeGridListeners<NotificationOutput> = {
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
    []) as NotificationOutput[];
}

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

async function onSetRead(row: NotificationOutput) {
  await setReadApi({ id: row.id });
  message.success($t('AbpUi.SavedSuccessfully'));
  await gridApi.query();
}

function onDelete(row: NotificationOutput) {
  Modal.confirm({
    centered: true,
    content: $t('AbpUi.ItemWillBeDeletedMessage'),
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await deleteApi({
          id: row.id,
          receiverUserId: row.receiveUserId,
        });
        message.success($t('AbpUi.DeletedSuccessfully'));
        await gridApi.query();
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('AbpUi.AreYouSure'),
  });
}

async function onBatchRead() {
  const ids = selectedRows.value.map((item) => item.id);
  if (ids.length === 0) {
    return;
  }
  await setBatchReadApi({ ids });
  message.success($t('AbpUi.SavedSuccessfully'));
  await gridApi.query();
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('TestWorkshop.Notification:MessageManagement')">
      <template #toolbar-tools>
        <Space :size="8">
          <Button :icon="h(SendOutlined)" type="primary" @click="onSend">
            {{ $t('TestWorkshop.Notification:SendMessage') }}
          </Button>
          <Button
            v-if="selectedRows.length > 0"
            class="!border-green-500 !bg-green-500 !text-white hover:!border-green-600 hover:!bg-green-600"
            :icon="h(ReadOutlined)"
            @click="onBatchRead"
          >
            {{ $t('TestWorkshop.Notification:BatchRead') }}
          </Button>
        </Space>
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
          <Tooltip
            v-if="!row.read"
            :title="$t('TestWorkshop.Notification:SetRead')"
          >
            <Button
              :icon="h(ReadOutlined)"
              type="link"
              @click="onSetRead(row)"
            />
          </Tooltip>
          <Tooltip
            v-if="isGranted([NotificationPermissions.Delete])"
            :title="$t('AbpUi.Delete')"
          >
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

    <SendModal mode="message" @change="() => gridApi.query()" />
    <DetailDrawer />
  </Page>
</template>
