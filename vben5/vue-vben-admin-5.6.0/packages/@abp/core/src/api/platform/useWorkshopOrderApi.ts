import type { PagedResultDto } from '@abp/core';

import type {
  GetWorkshopOrderListInput,
  WorkshopOrderAuditInput,
  WorkshopOrderAuditRecordDto,
  WorkshopOrderCreateDto,
  WorkshopOrderDto,
  WorkshopOrderProjectSupervisorDto,
  WorkshopOrderTelemetryGraphDto,
  WorkshopOrderUpdateDto,
  WorkshopTelemetryTaskDto,
  WorkshopTelemetryTaskListInput,
} from '../../types';

import { requestClient, useRequest } from '../../hooks';

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

  function getProjectSupervisorOptionsApi(): Promise<
    WorkshopOrderProjectSupervisorDto[]
  > {
    return request<WorkshopOrderProjectSupervisorDto[]>(
      '/api/workshop/orders/project-supervisors',
      {
        method: 'GET',
      },
    );
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

  function auditApi(
    id: string,
    input: WorkshopOrderAuditInput,
  ): Promise<WorkshopOrderDto> {
    return request<WorkshopOrderDto>(`/api/workshop/orders/${id}/audits`, {
      data: input,
      method: 'POST',
    });
  }

  function getAuditRecordsApi(
    id: string,
  ): Promise<WorkshopOrderAuditRecordDto[]> {
    return request<WorkshopOrderAuditRecordDto[]>(
      `/api/workshop/orders/${id}/audits`,
      {
        method: 'GET',
      },
    );
  }

  function downloadTestReportApi(id: string): Promise<Blob> {
    return requestClient.download(
      `/api/workshop/orders/${id}/test-report/download`,
    );
  }

  function previewTestReportApi(id: string): Promise<Blob> {
    return requestClient.download(
      `/api/workshop/orders/${id}/test-report/preview`,
    );
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

  function getTelemetryGraphApi(
    id: string,
  ): Promise<WorkshopOrderTelemetryGraphDto> {
    return request<WorkshopOrderTelemetryGraphDto>(
      `/api/workshop/orders/${id}/telemetry-graphs`,
      {
        method: 'GET',
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
    auditApi,
    cancel,
    cancelApi,
    createApi,
    deleteApi,
    downloadTestReportApi,
    endTestApi,
    getAuditRecordsApi,
    getApi,
    getListApi,
    getProjectSupervisorOptionsApi,
    previewTestReportApi,
    getTelemetryGraphApi,
    getTelemetryTasksApi,
    startTestApi,
    updateApi,
  };
}
