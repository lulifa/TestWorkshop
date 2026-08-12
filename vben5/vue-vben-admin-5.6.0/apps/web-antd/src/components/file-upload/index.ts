import { useVbenModal } from '@vben/common-ui';

import FileUploadModal from './FileUploadModal.vue';

interface FileUploadOptions {
  /** 是否多选，默认 false（单选） */
  multiple?: boolean;
  ownerId?: string;
  ownerType: string;
}

function useFileUpload() {
  const [UploadModal, uploadModalApi] = useVbenModal({
    connectedComponent: FileUploadModal,
  });

  function openFileUpload(options: FileUploadOptions) {
    uploadModalApi.setData(options);
    uploadModalApi.open();
  }

  return {
    UploadModal,
    openFileUpload,
  };
}

export { FileUploadModal, useFileUpload };

export type { FileUploadOptions };
