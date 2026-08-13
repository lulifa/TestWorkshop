import type {
  IdentitySecurityLogInput,
  IdentitySecurityLogOutput,
  PagedResultDto,
} from '@abp/core';

import { useRequest } from '@abp/core';

export function useSecurityLogsApi() {
  const { cancel, request } = useRequest();

  function getPagedListApi(
    input?: IdentitySecurityLogInput,
  ): Promise<PagedResultDto<IdentitySecurityLogOutput>> {
    return request<PagedResultDto<IdentitySecurityLogOutput>>(
      '/api/system/securitylog',
      {
        method: 'GET',
        params: input,
      },
    );
  }

  return {
    cancel,
    getPagedListApi,
  };
}
