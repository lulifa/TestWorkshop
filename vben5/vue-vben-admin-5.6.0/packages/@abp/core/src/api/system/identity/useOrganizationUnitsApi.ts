import type {
  GetIdentityRolesInput,
  GetIdentityUsersInput,
  GetOrganizationUnitPagedListInput,
  GetUnaddedRoleListInput,
  GetUnaddedUserListInput,
  IdentityRoleDto,
  IdentityUserDto,
  ListResultDto,
  OrganizationUnitAddRoleDto,
  OrganizationUnitAddUserDto,
  OrganizationUnitCreateDto,
  OrganizationUnitDto,
  OrganizationUnitGetChildrenDto,
  OrganizationUnitUpdateDto,
  PagedResultDto,
} from '@abp/core';

import { useRequest } from '@abp/core';

export function useOrganizationUnitsApi() {
  const { cancel, request } = useRequest();

  /**
   * 查询组织机构列表
   * @returns 组织机构实体数据传输对象列表
   */
  function getAllListApi(): Promise<ListResultDto<OrganizationUnitDto>> {
    return request<ListResultDto<OrganizationUnitDto>>(
      `/api/system/identity/organization-units/all`,
      {
        method: 'GET',
      },
    );
  }

  /**
   * 移动组织机构
   * @param id 组织机构id
   * @param parentId 父级组织机构id
   */
  function moveTo(id: string, parentId?: string): Promise<void> {
    return request(`api/system/identity/organization-units/${id}/move`, {
      data: { parentId },
      method: 'PUT',
    });
  }

  /**
   * 查询根组织机构列表
   * @returns 组织机构实体数据传输对象列表
   */
  function getRootListApi(): Promise<ListResultDto<OrganizationUnitDto>> {
    return request<ListResultDto<OrganizationUnitDto>>(
      `/api/system/identity/organization-units/root-node`,
      {
        method: 'GET',
      },
    );
  }

  /**
   * 查询下级组织机构列表
   * @param input 查询参数
   * @returns 组织机构实体数据传输对象列表
   */
  function getChildrenApi(
    input: OrganizationUnitGetChildrenDto,
  ): Promise<ListResultDto<OrganizationUnitDto>> {
    return request<ListResultDto<OrganizationUnitDto>>(
      `/api/system/identity/organization-units/find-children`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  /**
   * 查询未加入组织机构的角色列表
   * @param input 查询过滤参数
   * @returns 角色实体数据传输对象分页列表
   */
  function getUnaddedRoleListApi(
    input: GetUnaddedRoleListInput,
  ): Promise<PagedResultDto<IdentityRoleDto>> {
    return request<PagedResultDto<IdentityRoleDto>>(
      `/api/system/identity/organization-units/${input.id}/unadded-roles`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  /**
   * 查询组织机构角色列表
   * @param id 组织机构id
   * @param input 查询过滤参数
   * @returns 角色实体数据传输对象分页列表
   */
  function getRoleListApi(
    id: string,
    input?: GetIdentityRolesInput,
  ): Promise<PagedResultDto<IdentityRoleDto>> {
    return request<PagedResultDto<IdentityRoleDto>>(
      `/api/system/identity/organization-units/${id}/roles`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  /**
   * 角色添加到组织机构
   * @param id 组织机构id
   * @param input 角色id列表
   */
  function addRoles(
    id: string,
    input: OrganizationUnitAddRoleDto,
  ): Promise<void> {
    return request(`/api/system/identity/organization-units/${id}/roles`, {
      data: input,
      method: 'POST',
    });
  }

  /**
   * 查询未加入组织机构的用户列表
   * @param input 查询过滤参数
   * @returns 用户实体数据传输对象分页列表
   */
  function getUnaddedUserListApi(
    input: GetUnaddedUserListInput,
  ): Promise<PagedResultDto<IdentityUserDto>> {
    return request<PagedResultDto<IdentityUserDto>>(
      `/api/system/identity/organization-units/${input.id}/unadded-users`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  /**
   * 查询组织机构用户列表
   * @param id 组织机构id
   * @param input 查询过滤参数
   * @returns 用户实体数据传输对象分页列表
   */
  function getUserListApi(
    id: string,
    input?: GetIdentityUsersInput,
  ): Promise<PagedResultDto<IdentityUserDto>> {
    return request<PagedResultDto<IdentityUserDto>>(
      `/api/system/identity/organization-units/${id}/users`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  /**
   * 用户添加到组织机构
   * @param id 组织机构id
   * @param input 用户id列表
   */
  function addMembers(
    id: string,
    input: OrganizationUnitAddUserDto,
  ): Promise<void> {
    return request(`/api/system/identity/organization-units/${id}/users`, {
      data: input,
      method: 'POST',
    });
  }

  /**
   * 新增组织机构
   * @param input 参数
   * @returns 组织机构实体数据传输对象
   */
  function createApi(
    input: OrganizationUnitCreateDto,
  ): Promise<OrganizationUnitDto> {
    return request<OrganizationUnitDto>(
      '/api/system/identity/organization-units',
      {
        data: input,
        method: 'POST',
      },
    );
  }

  /**
   * 删除组织机构
   * @param id 组织机构id
   */
  function deleteApi(id: string): Promise<void> {
    return request(`/api/system/identity/organization-units/${id}`, {
      method: 'DELETE',
    });
  }

  /**
   * 查询组织机构
   * @param id 组织机构id
   * @returns 组织机构实体数据传输对象
   */
  function getApi(id: string): Promise<OrganizationUnitDto> {
    return request<OrganizationUnitDto>(
      `/api/system/identity/organization-units/${id}`,
      {
        method: 'GET',
      },
    );
  }

  /**
   * 更新组织机构
   * @param id 组织机构id
   * @returns 组织机构实体数据传输对象
   */
  function updateApi(
    id: string,
    input: OrganizationUnitUpdateDto,
  ): Promise<OrganizationUnitDto> {
    return request<OrganizationUnitDto>(
      `/api/system/identity/organization-units/${id}`,
      {
        data: input,
        method: 'PUT',
      },
    );
  }

  /**
   * 查询组织机构分页列表
   * @param input 过滤参数
   * @returns 组织机构实体数据传输对象分页列表
   */
  function getPagedListApi(
    input?: GetOrganizationUnitPagedListInput,
  ): Promise<PagedResultDto<OrganizationUnitDto>> {
    return request<PagedResultDto<OrganizationUnitDto>>(
      `/api/system/identity/organization-units`,
      {
        method: 'GET',
        params: input,
      },
    );
  }

  return {
    addMembers,
    addRoles,
    cancel,
    createApi,
    deleteApi,
    getAllListApi,
    getApi,
    getChildrenApi,
    getPagedListApi,
    getRoleListApi,
    getRootListApi,
    getUnaddedRoleListApi,
    getUnaddedUserListApi,
    getUserListApi,
    moveTo,
    updateApi,
  };
}
