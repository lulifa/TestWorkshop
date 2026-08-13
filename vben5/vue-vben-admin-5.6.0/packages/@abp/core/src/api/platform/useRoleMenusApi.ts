import type { ListResultDto } from '@abp/core';

import type {
  MenuDto,
  MenuGetByRoleInput,
  SetRoleMenuInput,
  SetRoleMenuStartupInput,
} from '../../types';

import { useRequest } from '../../hooks';

export function useRoleMenusApi() {
  const { cancel, request } = useRequest();

  function getAllApi(
    input: MenuGetByRoleInput,
  ): Promise<ListResultDto<MenuDto>> {
    return request<ListResultDto<MenuDto>>('/api/platform/menus/by-role', {
      method: 'GET',
      params: input,
    });
  }

  function setMenusApi(input: SetRoleMenuInput): Promise<void> {
    return request('/api/platform/menus/by-role', {
      data: input,
      method: 'PUT',
    });
  }

  function setStartupMenuApi(
    meudId: string,
    input: SetRoleMenuStartupInput,
  ): Promise<void> {
    return request(`/api/platform/menus/startup/${meudId}/by-role`, {
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
