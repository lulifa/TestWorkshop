import type { PagedResultDto } from '@abp/core';

import type { GetWorkshopAlarmListInput, WorkshopAlarmDto } from '../../types';

import { useRequest } from '../../hooks';

export function useWorkshopAlarmApi() {
  const { cancel, request } = useRequest();

  function getListApi(
    input?: GetWorkshopAlarmListInput,
  ): Promise<PagedResultDto<WorkshopAlarmDto>> {
    return request<PagedResultDto<WorkshopAlarmDto>>('/api/workshop/alarm', {
      method: 'GET',
      params: input,
    });
  }

  return {
    cancel,
    getListApi,
  };
}
