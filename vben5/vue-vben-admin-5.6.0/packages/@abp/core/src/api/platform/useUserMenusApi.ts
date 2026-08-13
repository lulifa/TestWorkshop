import type { ListResultDto } from '@abp/core';

import type {
  MenuDto,
  MenuGetByUserInput,
  SetUserMenuInput,
  SetUserMenuStartupInput,
} from '../../types';

import { useRequest } from '../../hooks';

export function useUserMenusApi() {
  const { cancel, request } = useRequest();

  function getAllApi(
    input: MenuGetByUserInput,
  ): Promise<ListResultDto<MenuDto>> {
    return request<ListResultDto<MenuDto>>('/api/platform/menus/by-user', {
      method: 'GET',
      params: input,
    });
  }

  function setMenusApi(input: SetUserMenuInput): Promise<void> {
    return request('/api/platform/menus/by-user', {
      data: input,
      method: 'PUT',
    });
  }

  function setStartupMenuApi(
    meudId: string,
    input: SetUserMenuStartupInput,
  ): Promise<void> {
    return request(`/api/platform/menus/startup/${meudId}/by-user`, {
      data: input,
      method: 'PUT',
    });
  }

  return {
    cancel,
    getAllApi,
    setMenusApi,
    setStartupMenuApi,
  };
}
