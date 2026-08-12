import type { FileObjectDto } from '@abp/core';

import { useVbenModal } from '@vben/common-ui';

import FilePreviewModal from './FilePreviewModal.vue';

function useFilePreview() {
  const [PreviewModal, previewModalApi] = useVbenModal({
    connectedComponent: FilePreviewModal,
  });

  function openFilePreview(row: FileObjectDto) {
    previewModalApi.setData(row);
    previewModalApi.open();
  }

  return {
    PreviewModal,
    openFilePreview,
  };
}

export { FilePreviewModal, useFilePreview };
