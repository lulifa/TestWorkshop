<script lang="ts" setup>
import type { WorkshopOrderDto, WorkshopOrderReportFileDto } from '@abp/core';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { formatToDateTime, useWorkshopOrderApi } from '@abp/core';
import { Empty, Spin } from 'ant-design-vue';

const { getTestReportFilesApi, previewTestReportFileApi } =
  useWorkshopOrderApi();

const order = ref<WorkshopOrderDto>();
const files = ref<WorkshopOrderReportFileDto[]>([]);
const selectedFile = ref<WorkshopOrderReportFileDto>();
const loading = ref(false);
const previewing = ref(false);
const previewUrl = ref('');
const previewContentType = ref('');
const textContent = ref('');

const isImage = computed(() => {
  const contentType =
    previewContentType.value || selectedFile.value?.contentType || '';
  const fileName = selectedFile.value?.fileName ?? '';
  return (
    contentType.startsWith('image/') ||
    /\.(?:bmp|gif|ico|jpe?g|png|svg|webp)$/i.test(fileName)
  );
});
const isPdf = computed(() => {
  const contentType =
    previewContentType.value || selectedFile.value?.contentType || '';
  const fileName = selectedFile.value?.fileName ?? '';
  return contentType === 'application/pdf' || /\.pdf$/i.test(fileName);
});
const isText = computed(() => {
  const contentType =
    previewContentType.value || selectedFile.value?.contentType || '';
  const fileName = selectedFile.value?.fileName ?? '';
  return (
    contentType.startsWith('text/') ||
    /\.(?:csv|json|log|md|txt|xml|ya?ml)$/i.test(fileName)
  );
});

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[94vw] max-w-[1500px]',
  contentClass: 'p-4',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      revokePreview();
      return;
    }

    order.value = drawerApi.getData<WorkshopOrderDto>();
    files.value = [];
    selectedFile.value = undefined;
    await loadFiles();
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.WorkshopOrder:TestReport'),
});

async function loadFiles() {
  if (!order.value?.id) {
    return;
  }

  loading.value = true;
  try {
    files.value = await getTestReportFilesApi(order.value.id);
    if (files.value.length > 0) {
      await selectFile(files.value[0]!);
    }
  } finally {
    loading.value = false;
  }
}

async function selectFile(file: WorkshopOrderReportFileDto) {
  if (!order.value?.id) {
    return;
  }

  selectedFile.value = file;
  revokePreview();
  if (!file.hasPreview) {
    return;
  }
  previewing.value = true;
  try {
    const blob = await previewTestReportFileApi(order.value.id, file.key);
    previewContentType.value = blob.type || file.contentType || '';
    previewUrl.value = URL.createObjectURL(blob);
    if (isText.value) {
      textContent.value = await blob.text();
    }
  } finally {
    previewing.value = false;
  }
}

function revokePreview() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value);
    previewUrl.value = '';
  }
  textContent.value = '';
  previewContentType.value = '';
}
</script>

<template>
  <Drawer>
    <Spin :spinning="loading">
      <div class="mb-4 rounded-md border border-border px-4 py-3">
        <div class="text-base font-semibold">{{ order?.orderNo }}</div>
        <div class="mt-1 text-sm text-gray-500">
          {{ order?.deviceCode ?? '-' }}
          <span v-if="order?.deviceName">· {{ order.deviceName }}</span>
          <span class="ml-4">
            {{
              $t('TestWorkshop.WorkshopOrder:ReportFileCount', [files.length])
            }}
          </span>
        </div>
      </div>

      <Empty
        v-if="!loading && files.length === 0"
        class="py-20"
        :description="$t('TestWorkshop.WorkshopOrder:NoReportFiles')"
      />

      <div v-else class="flex h-[calc(100vh-180px)] min-h-[520px] gap-4">
        <div
          class="w-[300px] shrink-0 overflow-y-auto rounded-md border border-border p-2"
        >
          <button
            v-for="file in files"
            :key="file.key"
            class="mb-2 w-full rounded-md border px-3 py-2 text-left transition-colors"
            :class="
              selectedFile?.key === file.key
                ? 'border-primary bg-primary/5'
                : 'border-transparent hover:bg-muted'
            "
            type="button"
            @click="selectFile(file)"
          >
            <div class="truncate text-sm font-medium" :title="file.fileName">
              {{ file.fileName }}
            </div>
            <div class="mt-1 text-xs text-gray-500">
              {{ formatToDateTime(file.creationTime) }}
            </div>
          </button>
        </div>

        <div
          class="flex min-w-0 flex-1 items-center justify-center overflow-hidden rounded-md border border-border bg-muted/20"
        >
          <Spin v-if="previewing" />
          <img
            v-else-if="previewUrl && isImage"
            :src="previewUrl"
            alt=""
            class="max-h-full max-w-full object-contain"
          />
          <iframe
            v-else-if="previewUrl && isPdf"
            :src="previewUrl"
            class="h-full w-full border-0"
          ></iframe>
          <pre
            v-else-if="previewUrl && isText"
            class="h-full w-full overflow-auto whitespace-pre-wrap break-words p-4 text-sm"
            >{{ textContent }}
          </pre>
          <Empty
            v-else-if="selectedFile"
            :description="
              selectedFile.hasPreview
                ? $t('TestWorkshop.WorkshopOrder:PreviewUnsupported')
                : $t('TestWorkshop.WorkshopOrder:NoPreviewContent')
            "
          />
        </div>
      </div>
    </Spin>
  </Drawer>
</template>

<style scoped></style>
