import type { WorkshopDeviceTypeDto } from '@abp/core';

export function toWorkshopDeviceTypeOptions(
  items: undefined | WorkshopDeviceTypeDto[],
) {
  return (items ?? []).map((item) => ({
    label: item.displayName || item.name,
    value: item.value,
  }));
}
