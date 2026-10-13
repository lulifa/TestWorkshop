import type { DeviceType } from './workshopDevices';

interface WorkshopThresholdRuleDto {
  alarmContent?: string;
  dataItemName: string;
  dataItemType: string;
  deviceType: DeviceType;
  enabled: boolean;
  id?: string;
  lowerLimit?: null | number;
  unit: string;
  upperLimit?: null | number;
}

interface WorkshopThresholdItemInput {
  alarmContent?: string;
  dataItemType: string;
  enabled: boolean;
  lowerLimit?: null | number;
  upperLimit?: null | number;
}

interface WorkshopThresholdSaveInput {
  deviceType: DeviceType;
  items: WorkshopThresholdItemInput[];
}

export type {
  WorkshopThresholdItemInput,
  WorkshopThresholdRuleDto,
  WorkshopThresholdSaveInput,
};
