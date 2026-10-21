import type { EntityDto, PagedAndSortedResultRequestDto } from '../..';

enum DeviceType {
  FIVA = 1,
  ICU = 3,
  PUMP = 2,
  SENSOR = 4,
}

interface WorkshopDeviceDto extends EntityDto<string> {
  code: string;
  creationTime?: string;
  lastHeartbeatAt?: string;
  lastModificationTime?: string;
  name: string;
  organizationUnitId: string;
  totalRuntimeSeconds: number;
  type: DeviceType;
  typeName?: string;
}

interface WorkshopDeviceCreateOrUpdateDto {
  code: string;
  name: string;
  organizationUnitId: string;
  type: DeviceType;
}

type WorkshopDeviceCreateDto = WorkshopDeviceCreateOrUpdateDto;

type WorkshopDeviceUpdateDto = WorkshopDeviceCreateOrUpdateDto;

interface GetWorkshopDeviceListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  isPaged?: boolean;
  organizationUnitId?: string;
  type?: DeviceType;
}

interface WorkshopDeviceTypeDto {
  displayName: string;
  name: string;
  value: DeviceType;
}

interface InitializeWorkshopInput {
  accountPrefix?: string;
  businessCode: string;
  displayName: string;
  initialPassword: string;
  parentOrganizationUnitId?: string;
}

interface InitializeWorkshopResultDto {
  accounts: InitializeWorkshopAccountDto[];
  businessCode: string;
  displayName: string;
  organizationUnitId: string;
  parentOrganizationUnitId?: string;
}

interface InitializeWorkshopAccountDto {
  displayName: string;
  roleName: string;
  userId: string;
  userName: string;
}

export { DeviceType };

export type {
  GetWorkshopDeviceListInput,
  InitializeWorkshopAccountDto,
  InitializeWorkshopInput,
  InitializeWorkshopResultDto,
  WorkshopDeviceCreateDto,
  WorkshopDeviceDto,
  WorkshopDeviceTypeDto,
  WorkshopDeviceUpdateDto,
};
