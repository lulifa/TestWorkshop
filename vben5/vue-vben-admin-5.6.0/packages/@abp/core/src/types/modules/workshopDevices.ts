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
  lastModificationTime?: string;
  name: string;
  organizationUnitId: string;
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

export { DeviceType };

export type {
  GetWorkshopDeviceListInput,
  WorkshopDeviceCreateDto,
  WorkshopDeviceDto,
  WorkshopDeviceTypeDto,
  WorkshopDeviceUpdateDto,
};
