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

import PdfPreview from '#/components/file-preview/PdfPreview.vue';

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
const previewContentType = ref('');
const previewFiles = ref<WorkshopOrderReportFileDto[]>([]);
const previewFileId = ref<string>();
const auditResult = ref<WorkshopOrderAuditResult>();
const maintenanceSuggestion = ref('');
const isPreviewPdf = computed(
  () =>
    previewContentType.value === 'application/pdf' ||
    previewFiles.value
      .find((file) => file.key === previewFileId.value)
      ?.fileName.toLowerCase()
      .endsWith('.pdf'),
);

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
  return !!order.value?.hasTestReport;
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
  [WorkshopTestReportStatus.Expired]: $t(
    'TestWorkshop.WorkshopOrder:ReportExpired',
  ),
};

const reportStatusColorMap: Record<WorkshopTestReportStatus, string> = {
  [WorkshopTestReportStatus.NotReady]: 'default',
  [WorkshopTestReportStatus.Fetching]: 'processing',
  [WorkshopTestReportStatus.Ready]: 'success',
  [WorkshopTestReportStatus.Failed]: 'error',
  [WorkshopTestReportStatus.Expired]: 'default',
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
    align: 'center' as const,
    dataIndex: 'reportFileCount',
    key: 'reportFileCount',
    title: $t('TestWorkshop.WorkshopOrder:FetchFileCount'),
    width: 110,
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
  class: 'w-[1100px] max-w-[96vw]',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      resetPreview();
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
    const [detail, auditRecords] = await Promise.all([
      getApi(order.value.id),
      getAuditRecordsApi(order.value.id),
    ]);
    order.value = detail;
    records.value = auditRecords ?? [];
    if (detail.hasTestReport) {
      previewFiles.value = await getTestReportFilesApi(order.value.id);
    }
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
  previewContentType.value = blob.type || file.contentType || '';
  previewUrl.value = URL.createObjectURL(blob);
}

function closePreview() {
  previewVisible.value = false;
  revokePreviewUrl();
  previewFileId.value = undefined;
}

function resetPreview() {
  closePreview();
  previewFiles.value = [];
}

function revokePreviewUrl() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value);
    previewUrl.value = '';
  }
  previewContentType.value = '';
}
</script>

<template>
  <Drawer>
    <Spin :spinning="loading">
      <div v-if="order" class="flex flex-col gap-4">
        <div class="rounded-md border border-border bg-muted/20 p-4">
          <div class="flex items-start justify-between gap-4">
            <div class="min-w-0">
              <div class="text-xs text-muted-foreground">
                {{ $t('TestWorkshop.WorkshopOrder:OrderNo') }}
              </div>
              <div class="mt-1 truncate text-lg font-semibold">
                {{ order.orderNo }}
              </div>
            </div>
            <Tag :color="statusColorMap[order.status]">
              {{ statusLabelMap[order.status] ?? order.statusName }}
            </Tag>
          </div>

          <div
            class="mt-4 grid grid-cols-1 gap-4 border-t border-border pt-3 sm:grid-cols-3"
          >
            <div class="min-w-0">
              <div class="text-xs text-muted-foreground">
                {{ $t('TestWorkshop.WorkshopOrder:Device') }}
              </div>
              <div class="mt-1 truncate text-sm font-medium">
                {{ order.deviceCode ?? '-' }}
              </div>
              <div
                v-if="order.deviceName"
                class="mt-0.5 truncate text-xs text-muted-foreground"
              >
                {{ order.deviceName }}
              </div>
            </div>
            <div class="min-w-0">
              <div class="text-xs text-muted-foreground">
                {{ $t('page.business.projectSupervisor') }}
              </div>
              <div class="mt-1 truncate text-sm font-medium">
                {{ order.projectSupervisorName || '-' }}
              </div>
            </div>
            <div class="min-w-0">
              <div class="text-xs text-muted-foreground">
                {{ $t('TestWorkshop.WorkshopOrder:TestEndedAt') }}
              </div>
              <div class="mt-1 truncate text-sm font-medium">
                {{
                  order.testEndedAt ? formatToDateTime(order.testEndedAt) : '-'
                }}
              </div>
            </div>
          </div>
        </div>

        <div class="rounded-md border border-border px-4 py-3">
          <div class="flex flex-wrap items-center justify-between gap-3">
            <div class="flex items-center gap-2">
              <span class="text-sm font-medium">
                {{ $t('TestWorkshop.WorkshopOrder:TestReport') }}
              </span>
              <Tag color="blue">
                {{ previewFiles.length }}
              </Tag>
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
              <template v-else-if="column.key === 'reportFileCount'">
                {{ record.reportFileCount }}
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
      width="94vw"
      @cancel="closePreview"
    >
      <div class="flex h-[78vh] min-h-0 flex-col gap-3 md:flex-row">
        <aside
          class="max-h-44 shrink-0 overflow-y-auto rounded-md border border-border p-2 md:max-h-none md:w-64"
        >
          <button
            v-for="file in previewFiles"
            :key="file.key"
            class="mb-2 w-full rounded-md border px-3 py-2 text-left transition-colors last:mb-0"
            :class="
              previewFileId === file.key
                ? 'border-primary bg-primary/5'
                : 'border-transparent hover:bg-muted'
            "
            type="button"
            @click="selectPreviewFile(file)"
          >
            <div class="truncate text-sm font-medium" :title="file.fileName">
              {{ file.fileName }}
            </div>
            <Tag v-if="!file.hasPreview" class="mt-1" color="default">
              {{ $t('TestWorkshop.WorkshopOrder:NoPreviewContent') }}
            </Tag>
          </button>
        </aside>
        <div
          class="min-h-0 min-w-0 flex-1 overflow-hidden rounded-md border border-border bg-muted/20"
        >
          <PdfPreview v-if="previewUrl && isPreviewPdf" :src="previewUrl" />
          <iframe
            v-else-if="previewUrl"
            :src="previewUrl"
            class="h-full w-full border-0"
          ></iframe>
          <Empty
            v-else
            class="flex h-full items-center justify-center"
            :description="$t('TestWorkshop.WorkshopOrder:NoPreviewContent')"
          />
        </div>
      </div>
    </Modal>
  </Drawer>
</template>

<style scoped></style>
