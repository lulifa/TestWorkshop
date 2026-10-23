<script lang="ts" setup>
import type { WorkshopOrderDto } from '@abp/core';
import type { MenuInfo } from 'ant-design-vue/es/menu/src/interface';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h, onMounted } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { downloadFileFromBlob } from '@vben/utils';

import {
  formatToDateTime,
  useAbpStore,
  useWorkshopOrderApi,
  WorkshopOrderStatus,
  WorkshopTestReportStatus,
} from '@abp/core';
import {
  AuditOutlined,
  CloseCircleOutlined,
  DeleteOutlined,
  DownloadOutlined,
  EditOutlined,
  EllipsisOutlined,
  EyeOutlined,
  FileTextOutlined,
  LineChartOutlined,
  PlayCircleOutlined,
  PlusOutlined,
  ReloadOutlined,
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
import { WorkshopSelect } from '#/components/workshop-select';

defineOptions({
  name: 'WorkshopOrderManagement',
});

const {
  cancelApi,
  deleteApi,
  downloadTestReportApi,
  endTestApi,
  getListApi,
  requestTestReportFetchApi,
  startTestApi,
} = useWorkshopOrderApi();
const MenuItem = Menu.Item;
const abpStore = useAbpStore();

const currentRoles = computed(() => {
  return abpStore.application?.currentUser.roles ?? [];
});

// 订单只能由调度或管理员新建/编辑
const canManageOrder = computed(() => {
  return currentRoles.value.some((role) =>
    ['admin', 'dispatcher'].includes(role),
  );
});
const canOperate = computed(() => {
  // 开始/结束测试：管理员、车间主管、测试工程师
  return currentRoles.value.some((role) =>
    ['admin', 'supervisor', 'tester'].includes(role),
  );
});
const canCancel = computed(() => {
  // 取消订单与开始/结束测试同一套角色：管理员、车间主管、测试工程师
  return currentRoles.value.some((role) =>
    ['admin', 'supervisor', 'tester'].includes(role),
  );
});
const canAudit = computed(() => {
  return currentRoles.value.some((role) => ['admin', 'auditor'].includes(role));
});
const canDownloadAllReports = computed(() => {
  return currentRoles.value.some((role) =>
    ['admin', 'auditor', 'dispatcher'].includes(role),
  );
});
const isProjectSupervisor = computed(() => {
  return currentRoles.value.includes('projectSupervisor');
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
    label: $t('TestWorkshop.WorkshopOrder:PendingReview'),
    value: WorkshopOrderStatus.PendingReview,
  },
  {
    label: $t('TestWorkshop.WorkshopOrder:ReviewApproved'),
    value: WorkshopOrderStatus.ReviewApproved,
  },
  {
    label: $t('TestWorkshop.WorkshopOrder:ReviewRejected'),
    value: WorkshopOrderStatus.ReviewRejected,
  },
  {
    label: $t('TestWorkshop.WorkshopOrder:Cancelled'),
    value: WorkshopOrderStatus.Cancelled,
  },
];

const statusColorMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: 'default',
  [WorkshopOrderStatus.Testing]: 'processing',
  [WorkshopOrderStatus.PendingReview]: 'warning',
  [WorkshopOrderStatus.ReviewApproved]: 'success',
  [WorkshopOrderStatus.ReviewRejected]: 'error',
  [WorkshopOrderStatus.Cancelled]: 'error',
};

const statusLabelMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: $t('TestWorkshop.WorkshopOrder:Draft'),
  [WorkshopOrderStatus.Testing]: $t('TestWorkshop.WorkshopOrder:Testing'),
  [WorkshopOrderStatus.PendingReview]: $t(
    'TestWorkshop.WorkshopOrder:PendingReview',
  ),
  [WorkshopOrderStatus.ReviewApproved]: $t(
    'TestWorkshop.WorkshopOrder:ReviewApproved',
  ),
  [WorkshopOrderStatus.ReviewRejected]: $t(
    'TestWorkshop.WorkshopOrder:ReviewRejected',
  ),
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
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'orderNo',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.WorkshopOrder:OrderNo'),
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
  wrapperClass: 'grid-cols-3',
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
      align: 'left',
      field: 'projectSupervisorName',
      formatter: ({ cellValue }) => cellValue || '-',
      minWidth: 140,
      title: $t('page.business.projectSupervisor'),
    },
    {
      align: 'center',
      field: 'status',
      minWidth: 150,
      slots: { default: 'status' },
      title: $t('TestWorkshop.WorkshopOrder:Status'),
    },
    {
      field: 'requiredCompletionTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '-';
      },
      minWidth: 180,
      title: $t('page.business.requiredCompletionTime'),
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
      width: 480,
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
          maxResultCount: page.pageSize,
          orderNo: values?.orderNo,
          organizationUnitId: values?.organizationUnitId,
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
          isPaged: false,
          orderNo: values?.orderNo,
          organizationUnitId: values?.organizationUnitId,
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

const [WorkshopOrderAuditDrawer, workshopOrderAuditDrawerApi] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkshopOrderAuditDrawer.vue'),
  ),
});

const [
  WorkshopOrderReportFetchRecordsDrawer,
  workshopOrderReportFetchRecordsDrawerApi,
] = useVbenDrawer({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkshopOrderReportFetchRecordsDrawer.vue'),
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

function onAudit(row: WorkshopOrderDto) {
  workshopOrderAuditDrawerApi.setData(row);
  workshopOrderAuditDrawerApi.open();
}

function onViewReportFetchRecords(row: WorkshopOrderDto) {
  workshopOrderReportFetchRecordsDrawerApi.setData(row);
  workshopOrderReportFetchRecordsDrawerApi.open();
}

function onFetchReport(row: WorkshopOrderDto) {
  confirmAction(
    $t('TestWorkshop.WorkshopOrder:FetchReport'),
    $t('TestWorkshop.WorkshopOrder:FetchReportConfirm', [row.orderNo]),
    () => requestTestReportFetchApi(row.id),
  );
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

async function onDownloadReport(row: WorkshopOrderDto) {
  gridApi.setLoading(true);
  try {
    const blob = await downloadTestReportApi(row.id);
    downloadFileFromBlob({
      fileName: `${row.orderNo}-测试报告.zip`,
      source: blob,
    });
  } finally {
    gridApi.setLoading(false);
  }
}

function canEdit(row: WorkshopOrderDto) {
  return canManageOrder.value && row.status === WorkshopOrderStatus.Draft;
}

function canStart(row: WorkshopOrderDto) {
  return canOperate.value && row.status === WorkshopOrderStatus.Draft;
}

function canEnd(row: WorkshopOrderDto) {
  return canOperate.value && row.status === WorkshopOrderStatus.Testing;
}

function canAuditOrder(row: WorkshopOrderDto) {
  return (
    canAudit.value &&
    [
      WorkshopOrderStatus.PendingReview,
      WorkshopOrderStatus.ReviewRejected,
    ].includes(row.status)
  );
}

function canViewAuditRecords(row: WorkshopOrderDto) {
  return [
    WorkshopOrderStatus.PendingReview,
    WorkshopOrderStatus.ReviewApproved,
    WorkshopOrderStatus.ReviewRejected,
  ].includes(row.status);
}

function canDownloadReport(row: WorkshopOrderDto) {
  if (row.status !== WorkshopOrderStatus.ReviewApproved || !row.hasTestReport) {
    return false;
  }

  if (canDownloadAllReports.value) {
    return true;
  }

  return (
    isProjectSupervisor.value &&
    row.projectSupervisorUserId?.toLowerCase() ===
      abpStore.application?.currentUser.id?.toLowerCase()
  );
}

function canFetchReport(row: WorkshopOrderDto) {
  return (
    canAudit.value &&
    row.status === WorkshopOrderStatus.PendingReview &&
    [
      WorkshopTestReportStatus.Expired,
      WorkshopTestReportStatus.Failed,
      WorkshopTestReportStatus.NotReady,
    ].includes(row.testReportStatus)
  );
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
    canManageOrder.value &&
    [WorkshopOrderStatus.Cancelled, WorkshopOrderStatus.Draft].includes(
      row.status,
    )
  );
}

function hasMoreActions(row: WorkshopOrderDto) {
  return (
    canEdit(row) ||
    canCancelOrder(row) ||
    canDelete(row) ||
    canViewAuditRecords(row) ||
    canFetchReport(row)
  );
}

function handleMoreMenuClick(row: WorkshopOrderDto, info: MenuInfo) {
  switch (info.key) {
    case 'auditRecords': {
      onAudit(row);
      break;
    }
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
    case 'fetchRecords': {
      onViewReportFetchRecords(row);
      break;
    }
    case 'fetchReport': {
      onFetchReport(row);
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
          v-if="canManageOrder"
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
              v-if="canAuditOrder(row)"
              :icon="h(AuditOutlined)"
              size="small"
              type="primary"
              @click="onAudit(row)"
            >
              {{ $t('TestWorkshop.WorkshopOrder:Audit') }}
            </Button>
            <Button
              v-else-if="canStart(row)"
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
              v-if="canDownloadReport(row)"
              :icon="h(DownloadOutlined)"
              size="small"
              type="text"
              @click="onDownloadReport(row)"
            >
              {{ $t('TestWorkshop.WorkshopOrder:DownloadReport') }}
              <span v-if="row.testReportFileCount > 1">
                ({{ row.testReportFileCount }})
              </span>
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
                  <MenuItem
                    v-if="canFetchReport(row)"
                    key="fetchReport"
                    :icon="h(ReloadOutlined)"
                  >
                    {{ $t('TestWorkshop.WorkshopOrder:FetchReport') }}
                  </MenuItem>
                  <MenuItem
                    v-if="canViewAuditRecords(row)"
                    key="fetchRecords"
                    :icon="h(FileTextOutlined)"
                  >
                    {{ $t('TestWorkshop.WorkshopOrder:FetchRecords') }}
                  </MenuItem>
                  <MenuItem
                    v-if="canViewAuditRecords(row)"
                    key="auditRecords"
                    :icon="h(AuditOutlined)"
                  >
                    {{ $t('TestWorkshop.WorkshopOrder:AuditRecords') }}
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
    <WorkshopOrderAuditDrawer @change="() => gridApi.query()" />
    <WorkshopOrderReportFetchRecordsDrawer />
  </Page>
</template>

<style scoped></style>
