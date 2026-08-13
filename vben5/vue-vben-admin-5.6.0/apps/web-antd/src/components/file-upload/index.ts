import { useVbenModal } from '@vben/common-ui';

import FileUploadModal from './FileUploadModal.vue';

interface FileUploadOptions {
  /** 接受的文件类型 */
  accept?: string;
  /** 是否多选，默认 false（单选） */
  multiple?: boolean;
  ownerId?: string;
  ownerType: string;
  /** 自定义弹窗标题 */
  title?: string;
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
