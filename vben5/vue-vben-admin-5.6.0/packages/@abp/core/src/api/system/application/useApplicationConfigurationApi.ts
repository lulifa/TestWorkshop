import type { ApplicationConfigurationDto } from '@abp/core';

import { useRequest } from '@abp/core';

export function useApplicationConfigurationApi() {
  const { cancel, request } = useRequest();

  /**
   * 获取应用程序配置信息
   */
  function getConfigApi(options?: {
    includeLocalizationResources?: boolean;
  }): Promise<ApplicationConfigurationDto> {
    return request<ApplicationConfigurationDto>(
      '/api/system/application-configuration',
      {
        params: options,
        method: 'GET',
      },
    );
  }

  return {
    cancel,
    getConfigApi,
  };
}
