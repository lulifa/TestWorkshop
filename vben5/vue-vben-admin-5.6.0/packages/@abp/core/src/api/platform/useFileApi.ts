import type { PagedResultDto } from '@abp/core';

import type { FileObjectDto, GetFileListInput } from '../../types';

import { requestClient, useRequest } from '../../hooks';

export function useFileApi() {
  const { cancel, request } = useRequest();

  function uploadApi(
    file: File,
    ownerType: string,
    ownerId: string,
  ): Promise<FileObjectDto> {
    return requestClient.upload<FileObjectDto>('/api/platform/files/upload', {
      file,
      ownerId,
      ownerType,
    });
  }

  function batchUploadApi(
    files: File[],
    ownerType: string,
    ownerId: string,
  ): Promise<FileObjectDto[]> {
    // ABP 的 List<IRemoteStreamContent> 要求表单字段名固定为 files，
    // requestClient.upload 会把数组转成 files[0]、files[1]，这里保持手动 FormData。
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));
    formData.append('ownerType', ownerType);
    formData.append('ownerId', ownerId);
    return request<FileObjectDto[]>('/api/platform/files/batch', {
      data: formData,
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      method: 'POST',
    });
  }

  function getPagedListApi(
    input?: GetFileListInput,
  ): Promise<PagedResultDto<FileObjectDto>> {
    return request<PagedResultDto<FileObjectDto>>('/api/platform/files', {
      method: 'GET',
      params: input,
    });
  }

  function getApi(id: string): Promise<FileObjectDto> {
    return request<FileObjectDto>(`/api/platform/files/${id}`, {
      method: 'GET',
    });
  }

  function downloadApi(id: string): Promise<Blob> {
    return requestClient.download(`/api/platform/files/${id}/download`);
  }

  function deleteApi(id: string): Promise<void> {
    return request(`/api/platform/files/${id}`, {
      method: 'DELETE',
    });
  }

  function deleteFilesApi(ownerType: string, ownerId: string): Promise<void> {
    return request('/api/platform/files', {
      method: 'DELETE',
      params: { ownerId, ownerType },
    });
  }

  return {
    batchUploadApi,
    cancel,
    deleteApi,
    deleteFilesApi,
    downloadApi,
    getApi,
    getPagedListApi,
    uploadApi,
  };
}
