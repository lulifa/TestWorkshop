import type {
  ChangeMyPasswordInput,
  ChangeUserPasswordInput,
  GetUserPagedListInput,
  IdentityRoleDto,
  IdentityUserCreateDto,
  IdentityUserDto,
  IdentityUserProfileInput,
  IdentityUserUpdateDto,
  ListResultDto,
  OrganizationUnitDto,
  PagedResultDto,
} from '@abp/core';

import { useRequest } from '@abp/core';

export function useUsersApi() {
  const { cancel, request } = useRequest();

  /**
   * 更改用户密码
   * @param id 用户id
   * @param input 密码变更dto
   */
  function changePasswordApi(
    id: string,
    input: ChangeUserPasswordInput,
  ): Promise<void> {
    return request(`/api/system/identity/users/change-password?id=${id}`, {
      data: input,
      method: 'PUT',
    });
  }

  /**
   * 锁定用户
   * @param id 用户id
   * @param seconds 锁定时长(秒)
   */
  function lockApi(id: string, seconds: number): Promise<void> {
    return request(`/api/system/identity/users/${id}/lock/${seconds}`, {
      method: 'PUT',
    });
  }

  /**
   * 解锁用户
   * @param id 用户id
   */
  function unLockApi(id: string): Promise<void> {
    return request(`/api/system/identity/users/${id}/unlock`, {
      method: 'PUT',
    });
  }

  /**
   * 获取用户组织机构列表
   * @param id 用户id
   */
  function getOrganizationUnitsApi(
    id: string,
  ): Promise<ListResultDto<OrganizationUnitDto>> {
    return request<ListResultDto<OrganizationUnitDto>>(
      `/api/system/identity/users/${id}/organization-units`,
      {
        method: 'GET',
      },
    );
  }

  /**
   * 从组织机构中移除用户
   * @param id 用户id
   * @param ouId 组织机构id
   */
  function removeOrganizationUnitApi(id: string, ouId: string): Promise<void> {
    return request(
      `/api/system/identity/users/${id}/organization-units/${ouId}`,
      {
        method: 'DELETE',
      },
    );
  }

  /**
   * 获取用户角色列表
   * @param id 用户id
   */
  function getRolesApi(id: string): Promise<ListResultDto<IdentityRoleDto>> {
    return request<ListResultDto<IdentityRoleDto>>(
      `/api/system/identity/users/${id}/roles`,
      {
        method: 'GET',
      },
    );
  }

  /**
   * 获取可用的角色列表
   */
  function getAssignableRolesApi(): Promise<ListResultDto<IdentityRoleDto>> {
    return request<ListResultDto<IdentityRoleDto>>(
      `/api/system/identity/users/assignable-roles`,
      {
        method: 'GET',
      },
    );
  }

  /**
   * 新增用户
   * @param input 参数
   * @returns 用户实体数据传输对象
   */
  function createApi(input: IdentityUserCreateDto): Promise<IdentityUserDto> {
    return request<IdentityUserDto>('/api/system/identity/users', {
      data: input,
      method: 'POST',
    });
  }

  /**
   * 删除用户
   * @param id 用户id
   */
  function deleteApi(id: string): Promise<void> {
    return request(`/api/system/identity/users/${id}`, {
      method: 'DELETE',
    });
  }

  /**
   * 查询用户
   * @param id 用户id
   * @returns 用户实体数据传输对象
   */
  function getApi(id: string): Promise<IdentityUserDto> {
    return request<IdentityUserDto>(`/api/system/identity/users/${id}`, {
      method: 'GET',
    });
  }

  /**
   * 修改当前登录用户密码
   */
  function changeCurrentUserPasswordApi(
    input: ChangeMyPasswordInput,
  ): Promise<void> {
    return request('/api/system/identity/users/my-password', {
      data: input,
      method: 'PUT',
    });
  }

  /**
   * 获取当前登录用户资料
   */
  function getCurrentUserProfileApi(): Promise<IdentityUserDto> {
    return request<IdentityUserDto>('/api/system/identity/users/my-profile', {
      method: 'GET',
    });
  }

  /**
   * 更新当前登录用户资料
   */
  function updateCurrentUserProfileApi(
    input: IdentityUserProfileInput,
  ): Promise<IdentityUserDto> {
    return request<IdentityUserDto>('/api/system/identity/users/my-profile', {
      data: input,
      method: 'PUT',
    });
  }

  /**
   * 更新用户
   * @param id 用户id
   * @returns 用户实体数据传输对象
   */
  function updateApi(
    id: string,
    input: IdentityUserUpdateDto,
  ): Promise<IdentityUserDto> {
    return request<IdentityUserDto>(`/api/system/identity/users/${id}`, {
      data: input,
      method: 'PUT',
    });
  }

  /**
   * 查询用户分页列表
   * @param input 过滤参数
   * @returns 用户实体数据传输对象分页列表
   */
  function getPagedListApi(
    input?: GetUserPagedListInput,
  ): Promise<PagedResultDto<IdentityUserDto>> {
    return request<PagedResultDto<IdentityUserDto>>(
      `/api/system/identity/users`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  return {
    cancel,
    changeCurrentUserPasswordApi,
    changePasswordApi,
    createApi,
    deleteApi,
    getApi,
    getAssignableRolesApi,
    getCurrentUserProfileApi,
    getOrganizationUnitsApi,
    getPagedListApi,
    getRolesApi,
    lockApi,
    removeOrganizationUnitApi,
    unLockApi,
    updateApi,
    updateCurrentUserProfileApi,
  };
}
