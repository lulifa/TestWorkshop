<script lang="ts" setup>
import type {
  WorkshopOrderAuditRecordDto,
  WorkshopOrderDto,
  WorkshopOrderReportFileDto,
} from '@abp/core';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useAbpStore,
  useWorkshopOrderApi,
  WorkshopOrderAuditResult,
  WorkshopOrderStatus,
  WorkshopTestReportStatus,
} from '@abp/core';
import {
  Alert,
  Button,
  Empty,
  Input,
  message,
  Modal,
  Radio,
  Spin,
  Table,
  Tag,
} from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change', data: WorkshopOrderDto): void;
}>();

const Textarea = Input.TextArea;
const {
  auditApi,
  getApi,
  getAuditRecordsApi,
  getTestReportFilesApi,
  previewTestReportFileApi,
} = useWorkshopOrderApi();
const abpStore = useAbpStore();

const order = ref<WorkshopOrderDto>();
const records = ref<WorkshopOrderAuditRecordDto[]>([]);
const loading = ref(false);
const submitting = ref(false);
const previewing = ref(false);
const previewVisible = ref(false);
const previewUrl = ref('');
const previewFiles = ref<WorkshopOrderReportFileDto[]>([]);
const previewFileId = ref<string>();
const auditResult = ref<WorkshopOrderAuditResult>();
const maintenanceSuggestion = ref('');

const roles = computed(() => {
  return abpStore.application?.currentUser.roles ?? [];
});
const canAudit = computed(() => {
  return roles.value.some((role) => ['admin', 'auditor'].includes(role));
});
const canSubmitAudit = computed(() => {
  return (
    canAudit.value &&
    !!order.value?.hasTestReport &&
    [
      WorkshopOrderStatus.PendingReview,
      WorkshopOrderStatus.ReviewRejected,
    ].includes(order.value.status)
  );
});
const canPreviewReport = computed(() => {
  return (
    canAudit.value &&
    !!order.value?.hasTestReport &&
    [
      WorkshopOrderStatus.PendingReview,
      WorkshopOrderStatus.ReviewRejected,
    ].includes(order.value.status)
  );
});

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

const statusColorMap: Record<WorkshopOrderStatus, string> = {
  [WorkshopOrderStatus.Draft]: 'default',
  [WorkshopOrderStatus.Testing]: 'processing',
  [WorkshopOrderStatus.PendingReview]: 'warning',
  [WorkshopOrderStatus.ReviewApproved]: 'success',
  [WorkshopOrderStatus.ReviewRejected]: 'error',
  [WorkshopOrderStatus.Cancelled]: 'error',
};

const reportStatusLabelMap: Record<WorkshopTestReportStatus, string> = {
  [WorkshopTestReportStatus.NotReady]: $t(
    'TestWorkshop.WorkshopOrder:ReportNotReady',
  ),
  [WorkshopTestReportStatus.Fetching]: $t(
    'TestWorkshop.WorkshopOrder:ReportFetching',
  ),
  [WorkshopTestReportStatus.Ready]: $t(
    'TestWorkshop.WorkshopOrder:ReportReady',
  ),
  [WorkshopTestReportStatus.Failed]: $t(
    'TestWorkshop.WorkshopOrder:ReportFailed',
  ),
};

const reportStatusColorMap: Record<WorkshopTestReportStatus, string> = {
  [WorkshopTestReportStatus.NotReady]: 'default',
  [WorkshopTestReportStatus.Fetching]: 'processing',
  [WorkshopTestReportStatus.Ready]: 'success',
  [WorkshopTestReportStatus.Failed]: 'error',
};

const columns = [
  {
    dataIndex: 'result',
    key: 'result',
    title: $t('TestWorkshop.WorkshopOrder:AuditResult'),
    width: 100,
  },
  {
    dataIndex: 'auditorName',
    key: 'auditorName',
    title: $t('TestWorkshop.WorkshopOrder:Auditor'),
    width: 130,
  },
  {
    dataIndex: 'auditedAt',
    key: 'auditedAt',
    title: $t('TestWorkshop.WorkshopOrder:AuditedAt'),
    width: 170,
  },
  {
    dataIndex: 'maintenanceSuggestion',
    key: 'maintenanceSuggestion',
    title: $t('TestWorkshop.WorkshopOrder:MaintenanceSuggestion'),
  },
];

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[760px] max-w-[96vw]',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      closePreview();
      return;
    }
    order.value = drawerApi.getData<WorkshopOrderDto>();
    auditResult.value = undefined;
    maintenanceSuggestion.value = '';
    records.value = [];
    previewFiles.value = [];
    previewFileId.value = undefined;
    await loadDetail();
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.WorkshopOrder:Audit'),
});

async function loadDetail() {
  if (!order.value?.id) {
    return;
  }
  loading.value = true;
  try {
    const [detail, auditRecords, reportFiles] = await Promise.all([
      getApi(order.value.id),
      getAuditRecordsApi(order.value.id),
      getTestReportFilesApi(order.value.id),
    ]);
    order.value = detail;
    records.value = auditRecords ?? [];
    previewFiles.value = reportFiles ?? [];
  } finally {
    loading.value = false;
  }
}

async function onSubmitAudit() {
  if (!order.value?.id || auditResult.value === undefined) {
    return;
  }
  submitting.value = true;
  try {
    const updated = await auditApi(order.value.id, {
      maintenanceSuggestion: maintenanceSuggestion.value.trim() || undefined,
      result: auditResult.value,
    });
    order.value = updated;
    emits('change', updated);
    message.success($t('AbpUi.SavedSuccessfully'));
    auditResult.value = undefined;
    maintenanceSuggestion.value = '';
    await loadDetail();
  } finally {
    submitting.value = false;
  }
}

async function onPreviewReport() {
  if (!order.value?.id) {
    return;
  }
  previewing.value = true;
  try {
    if (previewFiles.value.length === 0) {
      previewFiles.value = await getTestReportFilesApi(order.value.id);
    }
    const first = previewFiles.value[0];
    if (!first) {
      return;
    }
    await selectPreviewFile(first);
    previewVisible.value = true;
  } finally {
    previewing.value = false;
  }
}

async function selectPreviewFile(file: WorkshopOrderReportFileDto) {
  if (!order.value?.id) {
    return;
  }

  previewFileId.value = file.key;
  revokePreviewUrl();
  const blob = await previewTestReportFileApi(order.value.id, file.key);
  previewUrl.value = URL.createObjectURL(blob);
}

function closePreview() {
  previewVisible.value = false;
  revokePreviewUrl();
  previewFiles.value = [];
  previewFileId.value = undefined;
}

function revokePreviewUrl() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value);
    previewUrl.value = '';
  }
}
</script>

<template>
  <Drawer>
    <Spin :spinning="loading">
      <div v-if="order" class="flex flex-col gap-4">
        <div class="rounded-md border border-border px-4 py-3">
          <div class="flex flex-wrap items-center justify-between gap-3">
            <div class="min-w-0">
              <div class="truncate text-base font-semibold">
                {{ order.orderNo }}
              </div>
              <div class="mt-1 text-sm text-gray-500">
                {{ order.deviceCode ?? '-' }}
                <span v-if="order.deviceName">· {{ order.deviceName }}</span>
              </div>
            </div>
            <Tag :color="statusColorMap[order.status]">
              {{ statusLabelMap[order.status] ?? order.statusName }}
            </Tag>
          </div>
          <div class="mt-3 grid grid-cols-2 gap-2 text-sm text-gray-500">
            <span>
              {{ $t('page.business.projectSupervisor') }}:
              <span class="text-foreground">
                {{ order.projectSupervisorName || '-' }}
              </span>
            </span>
            <span>
              {{ $t('TestWorkshop.WorkshopOrder:TestEndedAt') }}:
              <span class="text-foreground">
                {{
                  order.testEndedAt ? formatToDateTime(order.testEndedAt) : '-'
                }}
              </span>
            </span>
          </div>
        </div>

        <div class="rounded-md border border-border px-4 py-3">
          <div class="flex flex-wrap items-center justify-between gap-3">
            <div class="flex items-center gap-2">
              <span class="text-sm font-medium">
                {{ $t('TestWorkshop.WorkshopOrder:TestReport') }}
              </span>
              <Tag
                :color="
                  reportStatusColorMap[order.testReportStatus] ?? 'default'
                "
              >
                {{
                  reportStatusLabelMap[order.testReportStatus] ??
                  $t('TestWorkshop.WorkshopOrder:ReportNotReady')
                }}
              </Tag>
            </div>
            <Button
              v-if="canPreviewReport"
              :loading="previewing"
              type="primary"
              @click="onPreviewReport"
            >
              {{ $t('TestWorkshop.WorkshopOrder:PreviewReport') }}
              <span v-if="order.testReportFileCount > 1">
                ({{ order.testReportFileCount }})
              </span>
            </Button>
          </div>
          <div class="mt-2 text-sm text-gray-500">
            <div
              v-for="file in previewFiles"
              :key="file.key"
              class="flex items-center gap-2 py-0.5"
            >
              <span class="truncate" :title="file.fileName">
                {{ file.fileName }}
              </span>
              <Tag v-if="!file.hasPreview" color="default">
                {{ $t('TestWorkshop.WorkshopOrder:NoPreviewContent') }}
              </Tag>
            </div>
            <div v-if="previewFiles.length === 0">
              {{ $t('TestWorkshop.WorkshopOrder:NoReportFiles') }}
            </div>
            <div v-if="order.testReportFetchedAt" class="mt-1">
              {{ $t('TestWorkshop.WorkshopOrder:ReportFetchedAt') }}:
              {{ formatToDateTime(order.testReportFetchedAt) }}
            </div>
          </div>
        </div>

        <Alert
          v-if="!order.hasTestReport"
          :message="$t('TestWorkshop.WorkshopOrder:ReportRequiredForAudit')"
          show-icon
          type="warning"
        />

        <div
          v-if="canSubmitAudit"
          class="rounded-md border border-border px-4 py-4"
        >
          <div class="mb-3 text-sm font-medium">
            {{ $t('TestWorkshop.WorkshopOrder:AuditResult') }}
          </div>
          <Radio.Group v-model:value="auditResult">
            <Radio :value="WorkshopOrderAuditResult.Approved">
              {{ $t('TestWorkshop.WorkshopOrder:AuditApproved') }}
            </Radio>
            <Radio :value="WorkshopOrderAuditResult.Rejected">
              {{ $t('TestWorkshop.WorkshopOrder:AuditRejected') }}
            </Radio>
          </Radio.Group>

          <div class="mt-4 text-sm font-medium">
            {{ $t('TestWorkshop.WorkshopOrder:MaintenanceSuggestion') }}
          </div>
          <Textarea
            v-model:value="maintenanceSuggestion"
            :maxlength="2000"
            :rows="3"
            class="mt-2"
            :placeholder="
              $t('TestWorkshop.WorkshopOrder:MaintenanceSuggestionPlaceholder')
            "
          />

          <div class="mt-4 flex justify-end">
            <Button
              :disabled="auditResult === undefined"
              :loading="submitting"
              type="primary"
              @click="onSubmitAudit"
            >
              {{ $t('TestWorkshop.WorkshopOrder:SubmitAudit') }}
            </Button>
          </div>
        </div>

        <div>
          <div class="mb-3 text-sm font-semibold">
            {{ $t('TestWorkshop.WorkshopOrder:AuditRecords') }}
          </div>
          <Empty
            v-if="records.length === 0"
            class="py-8"
            :description="$t('common.noData')"
          />
          <Table
            v-else
            :columns="columns"
            :data-source="records"
            :pagination="false"
            :row-key="(record: WorkshopOrderAuditRecordDto) => record.id"
            :scroll="{ x: 700 }"
            size="small"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'result'">
                <Tag
                  :color="
                    record.result === WorkshopOrderAuditResult.Approved
                      ? 'success'
                      : 'error'
                  "
                >
                  {{
                    record.result === WorkshopOrderAuditResult.Approved
                      ? $t('TestWorkshop.WorkshopOrder:AuditApproved')
                      : $t('TestWorkshop.WorkshopOrder:AuditRejected')
                  }}
                </Tag>
              </template>
              <template v-else-if="column.key === 'auditedAt'">
                {{ formatToDateTime(record.auditedAt) }}
              </template>
              <template v-else-if="column.key === 'maintenanceSuggestion'">
                {{ record.maintenanceSuggestion || '-' }}
              </template>
              <template v-else-if="column.key === 'auditorName'">
                {{ record.auditorName || '-' }}
              </template>
            </template>
          </Table>
        </div>
      </div>
    </Spin>

    <Modal
      :footer="null"
      :open="previewVisible"
      :title="$t('TestWorkshop.WorkshopOrder:PreviewReport')"
      width="90vw"
      @cancel="closePreview"
    >
      <div v-if="previewFiles.length > 1" class="mb-2 flex flex-wrap gap-2">
        <Button
          v-for="file in previewFiles"
          :key="file.key"
          :type="previewFileId === file.key ? 'primary' : 'default'"
          size="small"
          @click="selectPreviewFile(file)"
        >
          {{ file.fileName }}
        </Button>
      </div>
      <div class="h-[78vh]">
        <iframe
          v-if="previewUrl"
          :src="previewUrl"
          class="h-full w-full border-0"
        ></iframe>
      </div>
    </Modal>
  </Drawer>
</template>

<style scoped></style>
