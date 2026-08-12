<script lang="ts" setup>
import type { FileObjectDto } from '@abp/core';

import { computed, h, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { downloadFileFromBlob } from '@vben/utils';

import { useFileApi } from '@abp/core';
import { DownloadOutlined } from '@ant-design/icons-vue';
import { Button } from 'ant-design-vue';

const MAX_TEXT_PREVIEW_BYTES = 200 * 1024;

const blob = ref<Blob>();
const contentType = ref('');
const fileName = ref('');
const fileSize = ref(0);
const fileSizeText = ref('');
const loading = ref(false);
const objectUrl = ref('');
const textContent = ref('');

const { downloadApi } = useFileApi();

const isAudio = computed(
  () =>
    contentType.value.startsWith('audio/') ||
    /\.(?:aac|flac|m4a|mp3|ogg|wav)$/i.test(fileName.value),
);
const isImage = computed(
  () =>
    contentType.value.startsWith('image/') ||
    /\.(?:bmp|gif|ico|jpe?g|png|svg|webp)$/i.test(fileName.value),
);
const isPdf = computed(
  () =>
    contentType.value === 'application/pdf' ||
    fileName.value.toLowerCase().endsWith('.pdf'),
);
const isText = computed(() => {
  const textTypes = [
    'application/csv',
    'application/javascript',
    'application/json',
    'application/sql',
    'application/x-javascript',
    'application/x-yaml',
    'application/xml',
  ];
  return (
    contentType.value.startsWith('text/') ||
    textTypes.includes(contentType.value) ||
    /\.(?:css|csv|html?|js|jsx|json|log|md|sql|ts|tsx|txt|xml|ya?ml)$/i.test(
      fileName.value,
    )
  );
});
const isJson = computed(
  () =>
    contentType.value === 'application/json' || /\.json$/i.test(fileName.value),
);
const isLargeText = computed(() => fileSize.value > MAX_TEXT_PREVIEW_BYTES);
const isVideo = computed(
  () =>
    contentType.value.startsWith('video/') ||
    /\.(?:m4v|mov|mp4|ogg|webm)$/i.test(fileName.value),
);
const displayText = computed(() => {
  if (!isJson.value || !textContent.value) {
    return textContent.value;
  }
  if (isLargeText.value) {
    return textContent.value;
  }
  try {
    return JSON.stringify(JSON.parse(textContent.value), null, 2);
  } catch {
    return textContent.value;
  }
});

const [Modal, modalApi] = useVbenModal({
  class: 'w-[90vw] max-w-[1600px]',
  contentClass: 'p-0',
  footer: false,
  fullscreen: true,
  fullscreenButton: true,
  onClosed: revokeObjectUrl,
  onOpenChange: async (isOpen) => {
    if (!isOpen) return;
    const row = modalApi.getData<FileObjectDto>();
    fileName.value = row.fileName;
    contentType.value = row.contentType ?? '';
    fileSize.value = row.fileSize ?? 0;
    fileSizeText.value = row.fileSizeText ?? '';
    loading.value = true;
    try {
      const data = await downloadApi(row.id);
      blob.value = data;
      objectUrl.value = URL.createObjectURL(data);
      if (isText.value) {
        const previewBlob =
          fileSize.value > MAX_TEXT_PREVIEW_BYTES
            ? data.slice(0, MAX_TEXT_PREVIEW_BYTES)
            : data;
        textContent.value = await previewBlob.text();
      }
      modalApi.setState({ title: row.fileName });
    } finally {
      loading.value = false;
    }
  },
  title: '',
});

function onDownload() {
  if (!blob.value) return;
  downloadFileFromBlob({
    fileName: fileName.value,
    source: blob.value,
  });
}

function revokeObjectUrl() {
  if (objectUrl.value) {
    URL.revokeObjectURL(objectUrl.value);
  }
  blob.value = undefined;
  contentType.value = '';
  fileName.value = '';
  fileSize.value = 0;
  fileSizeText.value = '';
  objectUrl.value = '';
  textContent.value = '';
}
</script>

<template>
  <Modal>
    <div class="flex h-[80vh] flex-col">
      <div
        class="flex shrink-0 items-center justify-between gap-4 border-b px-4 py-3"
      >
        <div class="min-w-0">
          <div class="truncate text-sm font-medium">{{ fileName }}</div>
          <div class="mt-0.5 text-xs opacity-60">
            {{ fileSizeText || contentType }}
          </div>
        </div>
        <Button :icon="h(DownloadOutlined)" type="link" @click="onDownload">
          下载
        </Button>
      </div>
      <div
        class="flex min-h-0 flex-1 items-center justify-center overflow-hidden p-4"
      >
        <div v-if="loading">正在加载预览...</div>
        <img
          v-else-if="isImage"
          :src="objectUrl"
          alt=""
          class="max-h-[calc(80vh-130px)] max-w-full object-contain"
        />
        <iframe
          v-else-if="isPdf"
          :src="objectUrl"
          class="h-full w-full border-0"
        ></iframe>
        <video
          v-else-if="isVideo"
          :src="objectUrl"
          controls
          class="max-h-[calc(80vh-130px)] max-w-full"
        ></video>
        <audio v-else-if="isAudio" :src="objectUrl" controls></audio>
        <template v-else-if="isText">
          <div class="flex h-full w-full flex-col">
            <pre
              class="min-h-0 flex-1 overflow-auto"
              :class="
                isLargeText
                  ? 'whitespace-pre'
                  : 'whitespace-pre-wrap break-words'
              "
            >
              {{ displayText }}
            </pre>
            <div
              v-if="isLargeText"
              class="shrink-0 border-t px-4 py-2 text-xs opacity-70"
            >
              文件较大，当前仅预览前 200KB，完整内容请下载查看。
            </div>
          </div>
        </template>
        <div v-else class="text-center">
          <p>当前格式暂不支持浏览器在线预览，可以下载后查看。</p>
          <Button
            :icon="h(DownloadOutlined)"
            type="primary"
            @click="onDownload"
          >
            下载文件
          </Button>
        </div>
      </div>
    </div>
  </Modal>
</template>

<style scoped></style>
