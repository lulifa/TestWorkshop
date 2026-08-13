import type { ExtraPropertyDictionary } from '@abp/core';

export enum ChangeType {
  Created = 0,
  Deleted = 2,
  Updated = 1,
}

interface PropertyChange {
  id: string;
  newValue?: string;
  originalValue?: string;
  propertyName?: string;
  propertyTypeFullName?: string;
}

interface EntityChangeDto {
  [key: string]: any;
  changeTime?: Date;
  changeType: ChangeType;
  entityId?: string;
  entityTenantId?: string;
  entityTypeFullName?: string;
  extraProperties?: ExtraPropertyDictionary;
  id: string;
  propertyChanges?: PropertyChange[];
}

export type { EntityChangeDto, PropertyChange };
