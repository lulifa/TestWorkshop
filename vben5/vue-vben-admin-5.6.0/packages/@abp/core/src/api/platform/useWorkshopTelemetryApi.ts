import type { PagedResultDto } from '@abp/core';

import type {
  WorkshopTelemetryStatisticsDto,
  WorkshopTelemetryTaskDto,
  WorkshopTelemetryTaskListInput,
} from '../../types';

import { requestClient, useRequest } from '../../hooks';

export function useWorkshopTelemetryApi() {
  const { cancel, request } = useRequest();

  function deleteApi(id: number): Promise<void> {
    return request(`/api/workshop/telemetry/${id}`, {
      method: 'DELETE',
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

  function uploadApi(file: File): Promise<WorkshopTelemetryTaskDto> {
    return requestClient.upload<WorkshopTelemetryTaskDto>(
      '/api/workshop/telemetry/upload',
      { file },
    );
  }

  return {
    cancel,
    deleteApi,
    getListApi,
    getStatisticsApi,
    retryApi,
    uploadApi,
  };
}
