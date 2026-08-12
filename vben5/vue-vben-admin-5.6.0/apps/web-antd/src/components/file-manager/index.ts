import { useFilePreview } from '../file-preview';
import { useFileUpload } from '../file-upload';

function useFileManager() {
  const { PreviewModal, openFilePreview } = useFilePreview();
  const { UploadModal, openFileUpload } = useFileUpload();

  return {
    PreviewModal,
    UploadModal,
    openFilePreview,
    openFileUpload,
  };
}

export { useFileManager };
