import type { ListResultDto } from '@abp/core';

import type { MenuDto, MenuGetInput } from '../../types';

import { useRequest } from '../../hooks';

export function useMyMenusApi() {
  const { cancel, request } = useRequest();

  function getAllApi(input?: MenuGetInput): Promise<ListResultDto<MenuDto>> {
    return request<ListResultDto<MenuDto>>(
      '/api/platform/menus/by-current-user',
      {
        method: 'GET',
        params: input,
      },
    );
  }

  return {
    cancel,
    getAllApi,
  };
}
