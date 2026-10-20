import type { EntityDto, PagedAndSortedResultRequestDto } from '../..';

enum WorkshopAlarmType {
  Communication = 4,
  Device = 3,
  Relation = 2,
  Threshold = 1,
}

enum WorkshopAlarmStatus {
  Active = 1,
  Cleared = 2,
}

interface WorkshopAlarmDto extends EntityDto<string> {
  alarmType: WorkshopAlarmType;
  alarmTypeName?: string;
  clearedAt?: string;
  content: string;
  dataItemType: string;
  deviceCode: string;
  deviceId: string;
  organizationUnitId: string;
  startedAt: string;
  status: WorkshopAlarmStatus;
}

interface GetWorkshopAlarmListInput extends PagedAndSortedResultRequestDto {
  alarmType?: WorkshopAlarmType;
  deviceId?: string;
  endTime?: string;
  organizationUnitId?: string;
  startTime?: string;
  status?: WorkshopAlarmStatus;
}

export { WorkshopAlarmStatus, WorkshopAlarmType };

export type { GetWorkshopAlarmListInput, WorkshopAlarmDto };
