<script lang="ts" setup>
import type { UploadFile } from 'ant-design-vue';
import type { FileType } from 'ant-design-vue/es/upload/interface';

import { h, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useFileApi } from '@abp/core';
import {
  DeleteOutlined,
  EyeOutlined,
  FileExcelOutlined,
  FileImageOutlined,
  FileOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  FileWordOutlined,
  FileZipOutlined,
  InboxOutlined,
} from '@ant-design/icons-vue';
import { Button, Image, message, Tag, UploadDragger } from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change'): void;
}>();

const imageExtensions = new Set([
  'bmp',
  'gif',
  'ico',
  'jpeg',
  'jpg',
  'png',
  'svg',
  'webp',
]);

const fileList = ref<UploadFile[]>([]);
const multiple = ref(false);
const ownerId = ref('');
const ownerType = ref('');

const { batchUploadApi, uploadApi } = useFileApi();

const [Modal, modalApi] = useVbenModal({
  class: 'max-w-[720px]',
  onClosed: clearFileList,
  onConfirm: onSubmit,
  onOpenChange: (isOpen) => {
    if (isOpen) {
      clearFileList();
      const data = modalApi.getData<{
        multiple?: boolean;
        ownerId?: string;
        ownerType?: string;
      }>();
      multiple.value = data.multiple ?? false;
      ownerType.value = data.ownerType ?? '';
      ownerId.value = data.ownerId ?? '';
      modalApi.setState({
        title: multiple.value
          ? $t('TestWorkshop.FileManager:MultipleUpload')
          : $t('TestWorkshop.FileManager:SingleUpload'),
      });
    }
  },
  title: $t('TestWorkshop.FileManager:SingleUpload'),
});

function formatFileSize(size?: number) {
  if (size === undefined) {
    return '';
  }
  if (size < 1024) {
    return `${size} B`;
  }
  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }
  if (size < 1024 * 1024 * 1024) {
    return `${(size / (1024 * 1024)).toFixed(1)} MB`;
  }
  return `${(size / (1024 * 1024 * 1024)).toFixed(1)} GB`;
}

function getFileExtension(fileName: string) {
  return fileName.split('.').pop()?.toLowerCase() ?? '';
}

function getFileIcon(fileName: string) {
  const ext = getFileExtension(fileName);
  if (imageExtensions.has(ext)) {
    return FileImageOutlined;
  }
  if (ext === 'pdf') {
    return FilePdfOutlined;
  }
  if (['doc', 'docx'].includes(ext)) {
    return FileWordOutlined;
  }
  if (['csv', 'xls', 'xlsx'].includes(ext)) {
    return FileExcelOutlined;
  }
  if (['7z', 'gz', 'rar', 'tar', 'zip'].includes(ext)) {
    return FileZipOutlined;
  }
  if (
    [
      'css',
      'html',
      'js',
      'json',
      'log',
      'md',
      'sql',
      'ts',
      'txt',
      'xml',
      'yaml',
      'yml',
    ].includes(ext)
  ) {
    return FileTextOutlined;
  }
  return FileOutlined;
}

function isImageFile(file: FileType) {
  return (
    file.type.startsWith('image/') ||
    imageExtensions.has(getFileExtension(file.name))
  );
}

function revokeThumbUrl(file: UploadFile) {
  if (file.thumbUrl) {
    URL.revokeObjectURL(file.thumbUrl);
  }
}

function clearFileList() {
  fileList.value.forEach((file) => revokeThumbUrl(file));
  fileList.value = [];
}

function onBeforeUpload(file: FileType) {
  if (!multiple.value) {
    fileList.value.forEach((file) => revokeThumbUrl(file));
  }
  const uploadedFile: UploadFile = {
    name: file.name,
    originFileObj: file,
    size: file.size,
    status: 'done',
    thumbUrl: isImageFile(file) ? URL.createObjectURL(file) : undefined,
    uid: `${Date.now()}-${file.name}`,
  };
  fileList.value = multiple.value
    ? [...fileList.value, uploadedFile]
    : [uploadedFile];
  return false;
}

function onRemove(file: UploadFile) {
  revokeThumbUrl(file);
  fileList.value = fileList.value.filter((item) => item.uid !== file.uid);
}

async function onSubmit() {
  const files = fileList.value.flatMap((item) => {
    const file = item.originFileObj;
    return file ? [file] : [];
  });
  if (files.length === 0) {
    message.warning($t('TestWorkshop.FileManager:PleaseSelectFile'));
    return;
  }
  if (!ownerType.value.trim()) {
    message.warning($t('TestWorkshop.FileManager:OwnerTypeRequired'));
    return;
  }
  if (!ownerId.value.trim()) {
    message.warning('请填写业务ID');
    return;
  }
  if (!multiple.value && files.length > 1) {
    message.warning($t('TestWorkshop.FileManager:SingleUploadOnlyOneFile'));
    return;
  }
  const [firstFile] = files;
  if (!firstFile) return;
  try {
    modalApi.setState({ submitting: true });
    await (multiple.value
      ? batchUploadApi(files, ownerType.value.trim(), ownerId.value.trim())
      : uploadApi(firstFile, ownerType.value.trim(), ownerId.value.trim()));
    message.success($t('AbpUi.SavedSuccessfully'));
    emits('change');
    modalApi.close();
  } finally {
    modalApi.setState({ submitting: false });
  }
}
</script>

<template>
  <Modal>
    <div class="flex flex-col gap-4">
      <UploadDragger
        :before-upload="onBeforeUpload"
        :multiple="multiple"
        :show-upload-list="false"
      >
        <div class="flex flex-col items-center py-6 text-center">
          <div
            class="mb-3 flex size-12 items-center justify-center rounded-full bg-primary/10 text-2xl text-primary"
          >
            <InboxOutlined />
          </div>
          <div class="text-sm font-medium">
            {{
              multiple
                ? $t('TestWorkshop.FileManager:SelectOrDragMultiple')
                : $t('TestWorkshop.FileManager:SelectOrDragSingle')
            }}
          </div>
        </div>
      </UploadDragger>

      <div v-if="fileList.length > 0" class="flex min-h-0 flex-col">
        <div class="mb-2 flex items-center justify-between">
          <span class="text-xs font-medium text-muted-foreground">
            {{
              $t('TestWorkshop.FileManager:SelectedFilesCount', [
                fileList.length,
              ])
            }}
          </span>
          <Button
            :icon="h(DeleteOutlined)"
            size="small"
            type="text"
            @click="clearFileList"
          >
            {{ $t('TestWorkshop.FileManager:ClearAll') }}
          </Button>
        </div>
        <ul class="max-h-64 space-y-2 overflow-auto pr-1">
          <li
            v-for="file in fileList"
            :key="file.uid"
            class="flex items-center gap-3 rounded-md border border-border p-2.5"
          >
            <Image
              v-if="file.thumbUrl"
              :alt="file.name"
              :height="40"
              :src="file.thumbUrl"
              :width="40"
              class="size-10 shrink-0 rounded-md border border-border object-cover"
            >
              <template #previewMask>
                <EyeOutlined />
              </template>
            </Image>
            <div
              v-else
              class="flex size-10 shrink-0 items-center justify-center rounded-md bg-primary/10 text-lg text-primary"
            >
              <component :is="getFileIcon(file.name)" />
            </div>
            <div class="min-w-0 flex-1">
              <div class="truncate text-sm font-medium" :title="file.name">
                {{ file.name }}
              </div>
              <div
                class="mt-0.5 flex items-center gap-2 text-xs text-muted-foreground"
              >
                <span>{{ formatFileSize(file.size) }}</span>
                <Tag color="processing" class="!m-0">
                  {{ $t('TestWorkshop.FileManager:PendingUpload') }}
                </Tag>
              </div>
            </div>
            <Button
              :aria-label="$t('AbpUi.Delete')"
              :icon="h(DeleteOutlined)"
              danger
              size="small"
              type="text"
              @click="onRemove(file)"
            />
          </li>
        </ul>
      </div>

      <div
        v-else
        class="flex flex-col items-center gap-1 rounded-md border border-dashed border-border py-6 text-center text-muted-foreground"
      >
        <FileOutlined class="text-xl" />
        <span class="text-xs">
          {{ $t('TestWorkshop.FileManager:NoFileSelected') }}
        </span>
      </div>
    </div>
  </Modal>
</template>

<style scoped></style>
