import type { ListResultDto } from '@abp/core';

import type {
  DeviceType,
  WorkshopThresholdRuleDto,
  WorkshopThresholdSaveInput,
} from '../../types';

import { useRequest } from '../../hooks';

export function useWorkshopThresholdApi() {
  const { cancel, request } = useRequest();

  function getByDeviceTypeApi(
    deviceType: DeviceType,
  ): Promise<ListResultDto<WorkshopThresholdRuleDto>> {
    return request<ListResultDto<WorkshopThresholdRuleDto>>(
      `/api/workshop/threshold/${deviceType}`,
      {
        method: 'GET',
      },
    );
  }

  function saveApi(input: WorkshopThresholdSaveInput): Promise<void> {
    return request('/api/workshop/threshold/save', {
      data: input,
      method: 'POST',
    });
  }

  return {
    cancel,
    getByDeviceTypeApi,
    saveApi,
  };
}
