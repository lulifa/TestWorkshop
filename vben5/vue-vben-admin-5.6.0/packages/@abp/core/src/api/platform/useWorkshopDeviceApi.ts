import type {
  ListResultDto,
  OrganizationUnitDto,
  PagedResultDto,
} from '@abp/core';

import type {
  GetWorkshopDeviceListInput,
  WorkshopDeviceCreateDto,
  WorkshopDeviceDto,
  WorkshopDeviceTypeDto,
  WorkshopDeviceUpdateDto,
} from '../../types';

import { useRequest } from '../../hooks';

export function useWorkshopDeviceApi() {
  const { cancel, request } = useRequest();

  function createApi(
    input: WorkshopDeviceCreateDto,
  ): Promise<WorkshopDeviceDto> {
    return request<WorkshopDeviceDto>('/api/workshop/device', {
      data: input,
      method: 'POST',
    });
  }

  function deleteApi(id: string): Promise<void> {
    return request(`/api/workshop/device/${id}`, {
      method: 'DELETE',
    });
  }

  function getApi(id: string): Promise<WorkshopDeviceDto> {
    return request<WorkshopDeviceDto>(`/api/workshop/device/${id}`, {
      method: 'GET',
    });
  }

  function getListApi(
    input?: GetWorkshopDeviceListInput,
  ): Promise<PagedResultDto<WorkshopDeviceDto>> {
    return request<PagedResultDto<WorkshopDeviceDto>>('/api/workshop/device', {
      method: 'GET',
      params: input,
    });
  }

  function getOrganizationUnitsApi(): Promise<
    ListResultDto<OrganizationUnitDto>
  > {
    return request<ListResultDto<OrganizationUnitDto>>(
      '/api/workshop/device/organization-units',
      {
        method: 'GET',
      },
    );
  }

  function getTypesApi(): Promise<ListResultDto<WorkshopDeviceTypeDto>> {
    return request<ListResultDto<WorkshopDeviceTypeDto>>(
      '/api/workshop/device/types',
      {
        method: 'GET',
      },
    );
  }

  function updateApi(
    id: string,
    input: WorkshopDeviceUpdateDto,
  ): Promise<WorkshopDeviceDto> {
    return request<WorkshopDeviceDto>(`/api/workshop/device/${id}`, {
      data: input,
      method: 'PUT',
    });
  }

  return {
    cancel,
    createApi,
    deleteApi,
    getApi,
    getListApi,
    getOrganizationUnitsApi,
    getTypesApi,
    updateApi,
  };
}
