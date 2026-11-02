import type { Dayjs } from 'dayjs';

import { dateUtil } from '@abp/core';

interface YearRangePreset {
  label: string;
  value: [Dayjs, Dayjs];
}

export function createYearRangePresets(
  count = 10,
  now: Dayjs = dateUtil(),
): YearRangePreset[] {
  const currentYear = now.year();

  return Array.from({ length: count }, (_, index) => {
    const year = currentYear - index;
    return {
      label: String(year),
      value: [
        dateUtil().year(year).startOf('year'),
        dateUtil().year(year).endOf('year'),
      ],
    };
  });
}
