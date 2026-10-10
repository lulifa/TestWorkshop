import type { EntityDto, PagedAndSortedResultRequestDto } from '../..';

enum WorkshopOrderStatus {
  Cancelled = 3,
  Completed = 2,
  Draft = 0,
  Testing = 1,
}

interface WorkshopOrderDto extends EntityDto<string> {
  creationTime?: string;
  deviceCode?: string;
  deviceId: string;
  deviceName?: string;
  lastModificationTime?: string;
  orderNo: string;
  status: WorkshopOrderStatus;
  statusName?: string;
  testEndedAt?: string;
  testStartedAt?: string;
}

interface WorkshopOrderCreateOrUpdateDto {
  deviceId: string;
  orderNo: string;
}

type WorkshopOrderCreateDto = WorkshopOrderCreateOrUpdateDto;

type WorkshopOrderUpdateDto = WorkshopOrderCreateOrUpdateDto;

interface GetWorkshopOrderListInput extends PagedAndSortedResultRequestDto {
  deviceId?: string;
  endTime?: string;
  isPaged?: boolean;
  orderNo?: string;
  startTime?: string;
  status?: WorkshopOrderStatus;
}

export { WorkshopOrderStatus };

export type {
  GetWorkshopOrderListInput,
  WorkshopOrderCreateDto,
  WorkshopOrderDto,
  WorkshopOrderUpdateDto,
};
