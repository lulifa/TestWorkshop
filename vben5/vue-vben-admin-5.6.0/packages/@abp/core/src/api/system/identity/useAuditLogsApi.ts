import type {
  AuditLogDto,
  AuditLogGetListInput,
  PagedResultDto,
} from '@abp/core';

import { useRequest } from '@abp/core';

export function useAuditLogsApi() {
  const { cancel, request } = useRequest();

  /**
   * 获取审计日志
   * @param id 日志id
   */
  function getApi(id: string): Promise<AuditLogDto> {
    return request<AuditLogDto>(`/api/system/auditlog/${id}`, {
      method: 'GET',
    });
  }

  function getPagedListApi(
    input?: AuditLogGetListInput,
  ): Promise<PagedResultDto<AuditLogDto>> {
    return request<PagedResultDto<AuditLogDto>>('/api/system/auditlog', {
      method: 'GET',
      params: input,
    });
  }

  return {
    cancel,
    getApi,
    getPagedListApi,
  };
}
