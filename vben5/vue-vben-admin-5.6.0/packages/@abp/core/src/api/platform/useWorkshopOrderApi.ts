import type { PagedResultDto } from '@abp/core';

import type {
  GetWorkshopOrderListInput,
  WorkshopOrderCreateDto,
  WorkshopOrderDto,
  WorkshopOrderUpdateDto,
  WorkshopTelemetryTaskDto,
  WorkshopTelemetryTaskListInput,
} from '../../types';

import { useRequest } from '../../hooks';

export function useWorkshopOrderApi() {
  const { cancel, request } = useRequest();

  function createApi(input: WorkshopOrderCreateDto): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>('/api/workshop/orders', {
      data: input,
      method: 'POST',
    });
  }

  function deleteApi(id: string): Promise<void> {
    return request(`/api/workshop/orders/${id}`, {
      method: 'DELETE',
    });
  }

  function getApi(id: string): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>(`/api/workshop/orders/${id}`, {
      method: 'GET',
    });
  }

  function getListApi(
    input?: GetWorkshopOrderListInput,
  ): Promise<PagedResultDto<WorkshopOrderDto>> {
    return request<PagedResultDto<WorkshopOrderDto>>('/api/workshop/orders', {
      method: 'GET',
      params: input,
    });
  }

  function startTestApi(id: string): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>(`/api/workshop/orders/${id}/start`, {
      method: 'POST',
    });
  }

  function endTestApi(id: string): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>(`/api/workshop/orders/${id}/end`, {
      method: 'POST',
    });
  }

  function cancelApi(id: string): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>(`/api/workshop/orders/${id}/cancel`, {
      method: 'POST',
    });
  }

  function getTelemetryTasksApi(
    id: string,
    input?: WorkshopTelemetryTaskListInput,
  ): Promise<PagedResultDto<WorkshopTelemetryTaskDto>> {
    return request<PagedResultDto<WorkshopTelemetryTaskDto>>(
      `/api/workshop/orders/${id}/telemetry-tasks`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  function updateApi(
    id: string,
    input: WorkshopOrderUpdateDto,
  ): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>(`/api/workshop/orders/${id}`, {
      data: input,
      method: 'PUT',
    });
  }

  return {
    cancel,
    cancelApi,
    createApi,
    deleteApi,
    endTestApi,
    getApi,
    getListApi,
    getTelemetryTasksApi,
    startTestApi,
    updateApi,
  };
}
