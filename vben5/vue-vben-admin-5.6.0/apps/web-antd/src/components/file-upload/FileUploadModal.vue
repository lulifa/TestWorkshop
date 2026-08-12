<script lang="ts" setup>
import type { UploadFile } from 'ant-design-vue';
import type { FileType } from 'ant-design-vue/es/upload/interface';

import { h, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useFileApi } from '@abp/core';
import { UploadOutlined } from '@ant-design/icons-vue';
import { Button, message, Upload } from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change'): void;
}>();

const fileList = ref<UploadFile[]>([]);
const multiple = ref(false);
const ownerId = ref('');
const ownerType = ref('');

const { batchUploadApi, uploadApi } = useFileApi();

const [Modal, modalApi] = useVbenModal({
  onConfirm: onSubmit,
  onOpenChange: (isOpen) => {
    if (isOpen) {
      const data = modalApi.getData<{
        multiple?: boolean;
        ownerId?: string;
        ownerType?: string;
      }>();
      multiple.value = data.multiple ?? false;
      ownerType.value = data.ownerType ?? '';
      ownerId.value = data.ownerId ?? '';
      fileList.value = [];
      modalApi.setState({
        title: multiple.value ? '多文件上传' : '单文件上传',
      });
    }
  },
  title: '单文件上传',
});

function onBeforeUpload(file: FileType) {
  const uploadedFile: UploadFile = {
    name: file.name,
    originFileObj: file,
    status: 'done',
    uid: `${Date.now()}-${file.name}`,
  };
  fileList.value = multiple.value
    ? [...fileList.value, uploadedFile]
    : [uploadedFile];
  return false;
}

function onRemove(file: UploadFile) {
  fileList.value = fileList.value.filter((item) => item.uid !== file.uid);
}

async function onSubmit() {
  const files = fileList.value.flatMap((item) => {
    const file = item.originFileObj;
    return file ? [file] : [];
  });
  if (files.length === 0) {
    message.warning('请先选择要上传的文件');
    return;
  }
  if (!ownerType.value.trim()) {
    message.warning('请通过业务对象传入业务类型');
    return;
  }
  if (!multiple.value && files.length > 1) {
    message.warning('单文件上传只能选择一个文件');
    return;
  }
  const [firstFile] = files;
  if (!firstFile) return;
  try {
    modalApi.setState({ submitting: true });
    if (multiple.value) {
      if (!ownerId.value.trim()) {
        message.warning('批量上传必须填写业务ID');
        return;
      }
      await batchUploadApi(files, ownerType.value.trim(), ownerId.value.trim());
    } else {
      await uploadApi(
        firstFile,
        ownerType.value.trim(),
        ownerId.value.trim() || undefined,
      );
    }
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
    <Upload
      :before-upload="onBeforeUpload"
      :file-list="fileList"
      :multiple="multiple"
      @remove="onRemove"
    >
      <Button :icon="h(UploadOutlined)">选择文件</Button>
    </Upload>
  </Modal>
</template>

<style scoped></style>
