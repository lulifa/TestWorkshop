import type { PagedResultDto } from '@abp/core';

import type {
  WorkshopTelemetryFileInput,
  WorkshopTelemetryStatisticsDto,
  WorkshopTelemetryTaskDto,
  WorkshopTelemetryTaskListInput,
} from '../../types';

import { useRequest } from '../../hooks';

export function useWorkshopTelemetryApi() {
  const { cancel, request } = useRequest();

  function deleteApi(id: number): Promise<void> {
    return request(`/api/workshop/telemetry/${id}`, {
      method: 'DELETE',
    });
  }

  function deleteManyApi(ids: number[]): Promise<void> {
    return request('/api/workshop/telemetry/batch-delete', {
      data: { ids },
      method: 'POST',
    });
  }

  function getListApi(
    input?: WorkshopTelemetryTaskListInput,
  ): Promise<PagedResultDto<WorkshopTelemetryTaskDto>> {
    return request<PagedResultDto<WorkshopTelemetryTaskDto>>(
      '/api/workshop/telemetry',
      {
        method: 'GET',
        params: input,
      },
    );
  }

  function getStatisticsApi(): Promise<WorkshopTelemetryStatisticsDto> {
    return request<WorkshopTelemetryStatisticsDto>(
      '/api/workshop/telemetry/statistics',
      {
        method: 'GET',
      },
    );
  }

  function retryApi(id: number): Promise<void> {
    return request(`/api/workshop/telemetry/${id}/retry`, {
      method: 'POST',
    });
  }

  function uploadApi(
    file: File,
    input: WorkshopTelemetryFileInput,
  ): Promise<WorkshopTelemetryTaskDto> {
    const formData = new FormData();
    formData.append('file', file);
    appendFormData(formData, input);

    return request<WorkshopTelemetryTaskDto>('/api/workshop/telemetry/upload', {
      data: formData,
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      method: 'POST',
    });
  }

  return {
    cancel,
    deleteApi,
    deleteManyApi,
    getListApi,
    getStatisticsApi,
    retryApi,
    uploadApi,
  };
}

function appendFormData(
  formData: FormData,
  value: Record<string, any>,
  prefix = '',
) {
  Object.entries(value).forEach(([key, item]) => {
    if (item === null || item === undefined) {
      return;
    }

    const fieldName = prefix ? `${prefix}.${key}` : key;
    if (item instanceof Blob) {
      formData.append(fieldName, item);
      return;
    }

    if (typeof item === 'object' && !(item instanceof Date)) {
      appendFormData(formData, item, fieldName);
      return;
    }

    formData.append(fieldName, String(item));
  });
}
