import { dateUtil } from '@abp/core';
import { describe, expect, it } from 'vitest';

import { createYearRangePresets } from '../year-range-presets';

describe('createYearRangePresets', () => {
  it('creates descending year presets with full-year boundaries', () => {
    const presets = createYearRangePresets(3, dateUtil('2026-11-02'));

    expect(presets.map(({ label }) => label)).toEqual(['2026', '2025', '2024']);

    for (const preset of presets) {
      const [start, end] = preset.value;
      expect(start.format('YYYY-MM-DD HH:mm:ss')).toBe(
        `${preset.label}-01-01 00:00:00`,
      );
      expect(end.format('YYYY-MM-DD HH:mm:ss')).toBe(
        `${preset.label}-12-31 23:59:59`,
      );
    }
  });
});
