<script lang="ts" setup>
import type { WorkshopDeviceDto, WorkshopOrderDto } from '@abp/core';
import type { MenuInfo } from 'ant-design-vue/es/menu/src/interface';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h, onMounted } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useAbpStore,
  useWorkshopDeviceApi,
  useWorkshopOrderApi,
  WorkshopOrderStatus,
} from '@abp/core';
import {
  CloseCircleOutlined,
  DeleteOutlined,
  EditOutlined,
  EllipsisOutlined,
  EyeOutlined,
  LineChartOutlined,
  PlayCircleOutlined,
  PlusOutlined,
  StopOutlined,
} from '@ant-design/icons-vue';
import {
  Button,
  Dropdown,
  Menu,
  message,
  Modal,
  Space,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'WorkshopOrderManagement',
});

const { cancelApi, deleteApi, endTestApi, getListApi, startTestApi } =
  useWorkshopOrderApi();
const { getListApi: getDeviceListApi } = useWorkshopDeviceApi();
const MenuItem = Menu.Item;
const abpStore = useAbpStore();

const isAdmin = computed(
  () => abpStore.application?.currentUser.roles?.includes('admin') ?? false,
);
const canOperate = computed(() => {
  const roles = abpStore.application?.currentUser.roles ?? [];
  return roles.some((role) => ['admin', 'supervisor', 'tester'].includes(role));
});
const canCancel = computed(() => {
  const roles = abpStore.application?.currentUser.roles ?? [];
  return roles.some((role) => ['admin', 'supervisor'].includes(role));
});

const statusOptions = [
  {
    label: $t('TestWorkshop.WorkshopOrder:Draft'),
    value: WorkshopOrderStatus.Draft,
  },
  {
    label: $t('TestWorkshop.WorkshopOrder:Testing'),
    value: WorkshopOrderStatus.Testing,
  },
  {
    label: $t('TestWorkshop.WorkshopOrder:Completed'),
    value: WorkshopOrderStatus.Completed,
  },
  {
    label: $t('TestWorkshop.WorkshopOrder:Cancelled'),
    value: WorkshopOrderStatus.Cancelled,
  },
];

const statusColorMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: 'default',
  [WorkshopOrderStatus.Testing]: 'processing',
  [WorkshopOrderStatus.Completed]: 'success',
  [WorkshopOrderStatus.Cancelled]: 'error',
};

const statusLabelMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: $t('TestWorkshop.WorkshopOrder:Draft'),
  [WorkshopOrderStatus.Testing]: $t('TestWorkshop.WorkshopOrder:Testing'),
  [WorkshopOrderStatus.Completed]: $t('TestWorkshop.WorkshopOrder:Completed'),
  [WorkshopOrderStatus.Cancelled]: $t('TestWorkshop.WorkshopOrder:Cancelled'),
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
      fieldName: 'orderNo',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.WorkshopOrder:OrderNo'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        afterFetch: (result: { items: WorkshopDeviceDto[] }) => {
          return result.items.map((item) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
          }));
        },
        allowClear: true,
        api: getDeviceListApi,
        labelField: 'label',
        optionFilterProp: 'label',
        params: {
          isPaged: false,
          maxResultCount: 1000,
        },
        showSearch: true,
        valueField: 'value',
      },
      fieldName: 'deviceId',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.WorkshopOrder:Device'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: statusOptions,
      },
      fieldName: 'status',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.WorkshopOrder:Status'),
    },
  ],
  showCollapseButton: false,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<WorkshopOrderDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      align: 'left',
      field: 'orderNo',
      minWidth: 220,
      slots: { default: 'orderNo' },
      title: $t('TestWorkshop.WorkshopOrder:OrderNo'),
    },
    {
      align: 'left',
      field: 'deviceCode',
      minWidth: 240,
      slots: { default: 'device' },
      title: $t('TestWorkshop.WorkshopOrder:Device'),
    },
    {
      align: 'center',
      field: 'status',
      minWidth: 100,
      slots: { default: 'status' },
      title: $t('TestWorkshop.WorkshopOrder:Status'),
    },
    {
      field: 'testStartedAt',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '-';
      },
      minWidth: 180,
      title: $t('TestWorkshop.WorkshopOrder:TestStartedAt'),
    },
    {
      field: 'testEndedAt',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '-';
      },
      minWidth: 180,
      title: $t('TestWorkshop.WorkshopOrder:TestEndedAt'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('TestWorkshop.WorkshopOrder:TestingActions'),
      width: 320,
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const values = formValues as Record<string, any>;
        return await getListApi({
          deviceId: values?.deviceId,
          maxResultCount: page.pageSize,
          orderNo: values?.orderNo,
          skipCount: (page.currentPage - 1) * page.pageSize,
          sorting: sort.order ? `${sort.field} ${sort.order}` : undefined,
          status: values?.status,
        });
      },
      queryAll: async (params) => {
        const formValues = await gridApi.formApi.getValues();
        const values = formValues as Record<string, any>;
        const { sort } = params;
        return await getListApi({
          deviceId: values?.deviceId,
          isPaged: false,
          orderNo: values?.orderNo,
          sorting: sort.order ? `${sort.field} ${sort.order}` : undefined,
          status: values?.status,
        });
      },
    },
    response: {
      list: 'items',
      total: 'totalCount',
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

const gridEvents: VxeGridListeners<WorkshopOrderDto> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const [WorkshopOrderModal, workshopOrderModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkshopOrderModal.vue'),
  ),
});

const [WorkshopOrderTasksDrawer, workshopOrderTasksDrawerApi] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkshopOrderTasksDrawer.vue'),
  ),
});

const [WorkshopOrderGraphDrawer, workshopOrderGraphDrawerApi] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkshopOrderGraphDrawer.vue'),
  ),
});

function onCreate() {
  workshopOrderModalApi.setData({});
  workshopOrderModalApi.open();
}

function onEdit(row: WorkshopOrderDto) {
  workshopOrderModalApi.setData(row);
  workshopOrderModalApi.open();
}

function onViewTasks(row: WorkshopOrderDto) {
  workshopOrderTasksDrawerApi.setData(row);
  workshopOrderTasksDrawerApi.open();
}

function onViewGraph(row: WorkshopOrderDto) {
  workshopOrderGraphDrawerApi.setData(row);
  workshopOrderGraphDrawerApi.open();
}

function getDeviceLabel(row: WorkshopOrderDto) {
  if (!row.deviceName) {
    return row.deviceCode ?? '-';
  }

  return `${row.deviceCode ?? '-'} · ${row.deviceName}`;
}

function confirmAction(
  title: string,
  content: string,
  action: () => Promise<any>,
) {
  Modal.confirm({
    centered: true,
    content,
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await action();
        message.success($t('AbpUi.SavedSuccessfully'));
        await gridApi.query();
      } finally {
        gridApi.setLoading(false);
      }
    },
    title,
  });
}

function onStart(row: WorkshopOrderDto) {
  confirmAction(
    $t('TestWorkshop.WorkshopOrder:StartTest'),
    $t('TestWorkshop.WorkshopOrder:StartTestConfirm', [row.orderNo]),
    () => startTestApi(row.id),
  );
}

function onEnd(row: WorkshopOrderDto) {
  confirmAction(
    $t('TestWorkshop.WorkshopOrder:EndTest'),
    $t('TestWorkshop.WorkshopOrder:EndTestConfirm', [row.orderNo]),
    () => endTestApi(row.id),
  );
}

function onCancel(row: WorkshopOrderDto) {
  confirmAction(
    $t('TestWorkshop.WorkshopOrder:CancelOrder'),
    $t('TestWorkshop.WorkshopOrder:CancelConfirm', [row.orderNo]),
    () => cancelApi(row.id),
  );
}

function onDelete(row: WorkshopOrderDto) {
  confirmAction(
    $t('AbpUi.AreYouSure'),
    $t('AbpUi.ItemWillBeDeletedMessage'),
    () => deleteApi(row.id),
  );
}

function canEdit(row: WorkshopOrderDto) {
  return isAdmin.value && row.status === WorkshopOrderStatus.Draft;
}

function canStart(row: WorkshopOrderDto) {
  return canOperate.value && row.status === WorkshopOrderStatus.Draft;
}

function canEnd(row: WorkshopOrderDto) {
  return canOperate.value && row.status === WorkshopOrderStatus.Testing;
}

function canCancelOrder(row: WorkshopOrderDto) {
  return (
    canCancel.value &&
    [WorkshopOrderStatus.Draft, WorkshopOrderStatus.Testing].includes(
      row.status,
    )
  );
}

function canDelete(row: WorkshopOrderDto) {
  return (
    isAdmin.value &&
    [WorkshopOrderStatus.Cancelled, WorkshopOrderStatus.Draft].includes(
      row.status,
    )
  );
}

function hasMoreActions(row: WorkshopOrderDto) {
  return canEdit(row) || canCancelOrder(row) || canDelete(row);
}

function handleMoreMenuClick(row: WorkshopOrderDto, info: MenuInfo) {
  switch (info.key) {
    case 'cancel': {
      onCancel(row);
      break;
    }
    case 'delete': {
      onDelete(row);
      break;
    }
    case 'edit': {
      onEdit(row);
      break;
    }
  }
}

onMounted(() => {
  void gridApi.query();
});
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('page.business.workshoporders')">
      <template #toolbar-tools>
        <Button
          v-if="isAdmin"
          :icon="h(PlusOutlined)"
          type="primary"
          @click="onCreate"
        >
          {{ $t('TestWorkshop.WorkshopOrder:AddNew') }}
        </Button>
      </template>
      <template #orderNo="{ row }">
        <span class="block truncate font-medium" :title="row.orderNo">
          {{ row.orderNo }}
        </span>
      </template>
      <template #device="{ row }">
        <span class="block truncate" :title="getDeviceLabel(row)">
          <span class="font-medium">{{ row.deviceCode ?? '-' }}</span>
          <span v-if="row.deviceName" class="text-gray-500">
            · {{ row.deviceName }}
          </span>
        </span>
      </template>
      <template #status="{ row }">
        <Tag :color="statusColorMap[row.status]">
          {{ statusLabelMap[row.status] ?? row.statusName }}
        </Tag>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row items-center justify-center">
          <Space :size="4">
            <Button
              v-if="canStart(row)"
              :icon="h(PlayCircleOutlined)"
              size="small"
              type="primary"
              @click="onStart(row)"
            >
              {{ $t('TestWorkshop.WorkshopOrder:StartTest') }}
            </Button>
            <Button
              v-else-if="canEnd(row)"
              :icon="h(StopOutlined)"
              danger
              size="small"
              type="primary"
              @click="onEnd(row)"
            >
              {{ $t('TestWorkshop.WorkshopOrder:EndTest') }}
            </Button>
            <Button
              :icon="h(EyeOutlined)"
              size="small"
              type="text"
              @click="onViewTasks(row)"
            >
              {{ $t('TestWorkshop.WorkshopOrder:TelemetryTasks') }}
            </Button>
            <Button
              :icon="h(LineChartOutlined)"
              size="small"
              type="text"
              @click="onViewGraph(row)"
            >
              {{ $t('page.business.orderGraph') }}
            </Button>
            <Dropdown v-if="hasMoreActions(row)" :trigger="['click']">
              <template #overlay>
                <Menu @click="(info) => handleMoreMenuClick(row, info)">
                  <MenuItem
                    v-if="canEdit(row)"
                    key="edit"
                    :icon="h(EditOutlined)"
                  >
                    {{ $t('AbpUi.Edit') }}
                  </MenuItem>
                  <MenuItem
                    v-if="canCancelOrder(row)"
                    key="cancel"
                    :icon="h(CloseCircleOutlined)"
                  >
                    {{ $t('TestWorkshop.WorkshopOrder:CancelOrder') }}
                  </MenuItem>
                  <MenuItem
                    v-if="canDelete(row)"
                    key="delete"
                    danger
                    :icon="h(DeleteOutlined)"
                  >
                    {{ $t('AbpUi.Delete') }}
                  </MenuItem>
                </Menu>
              </template>
              <Button :icon="h(EllipsisOutlined)" size="small" type="text" />
            </Dropdown>
          </Space>
        </div>
      </template>
    </Grid>

    <WorkshopOrderModal @change="() => gridApi.query()" />
    <WorkshopOrderTasksDrawer />
    <WorkshopOrderGraphDrawer />
  </Page>
</template>

<style scoped></style>
