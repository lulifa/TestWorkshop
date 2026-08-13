import type {
  ExtensibleObject,
  IEnablePaging,
  PagedAndSortedResultRequestDto,
} from '@abp/core';

interface IdentitySecurityLogOutput extends ExtensibleObject {
  action?: string;
  applicationName?: string;
  browserInfo?: string;
  clientId?: string;
  clientIpAddress?: string;
  correlationId?: string;
  creationTime?: Date;
  id: string;
  identity?: string;
  tenantId?: string;
  tenantName?: string;
  userId?: string;
  userName?: string;
}

interface IdentitySecurityLogInput
  extends IEnablePaging, PagedAndSortedResultRequestDto {
  actionName?: string;
  applicationName?: string;
  clientId?: string;
  clientIpAddress?: string;
  correlationId?: string;
  endTime?: Date;
  identity?: string;
  startTime?: Date;
  userId?: string;
  userName?: string;
}

export type { IdentitySecurityLogInput, IdentitySecurityLogOutput };
