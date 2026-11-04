<script lang="ts" setup>
import type { WorkshopTelemetryGraphChannelDto } from '@abp/core';

import type { EchartsUIType } from '@vben/plugins/echarts';

import { computed, h, onBeforeUnmount, onMounted, ref, watch } from 'vue';

import { $t } from '@vben/locales';
import { EchartsUI, useEcharts } from '@vben/plugins/echarts';

import { CloseOutlined, SwapOutlined } from '@ant-design/icons-vue';
import { Button, Tooltip } from 'ant-design-vue';

interface Props {
  channels: WorkshopTelemetryGraphChannelDto[];
  durationMs?: number;
  height?: string;
  startTime?: string;
}

interface MeasurementPoint {
  channelType: string;
  timestamp: number;
  value: number;
}

const props = withDefaults(defineProps<Props>(), {
  durationMs: 0,
  height: '460px',
  startTime: undefined,
});

const chartRef = ref<EchartsUIType>();
const { getChartInstance, renderEcharts } = useEcharts(chartRef);
type ChartInstance = NonNullable<ReturnType<typeof getChartInstance>>;

const measurementMode = ref(false);
const measurementPoints = ref<MeasurementPoint[]>([]);
const firstMeasurementPoint = computed(() => measurementPoints.value[0]);
const secondMeasurementPoint = computed(() => measurementPoints.value[1]);
const measurementDeltaMs = computed(() => {
  if (!firstMeasurementPoint.value || !secondMeasurementPoint.value) {
    return undefined;
  }
  return Math.abs(
    secondMeasurementPoint.value.timestamp -
      firstMeasurementPoint.value.timestamp,
  );
});
const measurementButtonText = computed(() => {
  if (!measurementMode.value) {
    return $t('page.business.graphMeasure');
  }
  if (measurementPoints.value.length === 0) {
    return $t('page.business.graphMeasureFirstPoint');
  }
  if (measurementPoints.value.length === 1) {
    return $t('page.business.graphMeasureSecondPoint');
  }
  return $t('page.business.graphMeasureAgain');
});
const durationMs = computed(() => Math.max(props.durationMs ?? 0, 0));
const startTimestamp = computed(() =>
  props.startTime ? new Date(props.startTime).getTime() : Number.NaN,
);
const hasTimeAxis = computed(
  () => durationMs.value > 0 && !Number.isNaN(startTimestamp.value),
);

const channelUnits: Record<string, string> = {
  CMD: '%',
  Drive: '%',
  FivaFB: '%',
  Flow: 'L/min',
  FOP: 'MPa',
  LoadPress: 'MPa',
  PilotFB: '%',
  Plunger: 'mm',
};

const channelColors = [
  '#5470c6',
  '#91cc75',
  '#fac858',
  '#ee6666',
  '#73c0de',
  '#3ba272',
  '#fc8452',
  '#9a60b4',
];

const measurementSeriesName = '__measurement__';

const htmlEntities: Record<string, string> = {
  '"': '&quot;',
  '&': '&amp;',
  "'": '&#39;',
  '<': '&lt;',
  '>': '&gt;',
};

function getChannelUnit(channelType: string) {
  const normalized = channelType.trim().toLowerCase();
  return (
    Object.entries(channelUnits).find(
      ([key]) => key.toLowerCase() === normalized,
    )?.[1] ?? ''
  );
}

function getChannelColor(index: number) {
  return channelColors[index % channelColors.length] ?? channelColors[0];
}

function escapeHtml(value: string) {
  return value.replaceAll(
    /[&<>"']/g,
    (character) => htmlEntities[character] ?? character,
  );
}

function formatElapsedTime(value: number) {
  const milliseconds = Math.round(value * 1000) / 1000;
  if (Math.abs(milliseconds) < 1000) {
    return `${milliseconds.toLocaleString()} ms`;
  }
  return `${(milliseconds / 1000).toLocaleString(undefined, {
    maximumFractionDigits: 3,
  })} s`;
}

function formatMeasurementDelta(value: number) {
  return `${(Math.round(value * 1000) / 1000).toLocaleString()} ms`;
}

function formatDateTime(timestamp: number) {
  const date = new Date(timestamp);
  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  const pad = (value: number, length = 2) =>
    String(value).padStart(length, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(
    date.getDate(),
  )} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(
    date.getSeconds(),
  )}.${pad(date.getMilliseconds(), 3)}`;
}

function formatPointValue(point: MeasurementPoint) {
  const unit = getChannelUnit(point.channelType);
  return `${point.value}${unit ? ` ${unit}` : ''}`;
}

function buildSeriesData(values: number[], startTimestamp: number) {
  const durationMs = Math.max(props.durationMs ?? 0, 0);
  if (values.length <= 1 || durationMs <= 0 || Number.isNaN(startTimestamp)) {
    return values;
  }

  const intervalMs = durationMs / (values.length - 1);
  return values.map((value, index) => [
    startTimestamp + index * intervalMs,
    value,
  ]);
}

function clearMeasurement() {
  measurementPoints.value = [];
  updateMeasurementOverlay();
}

function toggleMeasurementMode() {
  measurementMode.value = !measurementMode.value;
  if (!measurementMode.value) {
    measurementPoints.value = [];
  }
  updateMeasurementOverlay();
}

function updateMeasurementOverlay() {
  getChartInstance()?.setOption({
    series: [
      {
        data: measurementPoints.value.map((point) => [
          point.timestamp,
          point.value,
        ]),
        id: measurementSeriesName,
      },
    ],
  });
}

function findNearestPoint(
  chart: ChartInstance,
  offsetX: number,
  offsetY: number,
) {
  const currentDurationMs = durationMs.value;
  const currentStartTimestamp = startTimestamp.value;
  if (currentDurationMs <= 0 || Number.isNaN(currentStartTimestamp)) {
    return undefined;
  }

  const dataCoordinate = chart.convertFromPixel({ gridIndex: 0 }, [
    offsetX,
    offsetY,
  ]);
  const clickedTimestamp = Array.isArray(dataCoordinate)
    ? Number(dataCoordinate[0])
    : Number.NaN;
  if (
    !Number.isFinite(clickedTimestamp) ||
    clickedTimestamp < currentStartTimestamp ||
    clickedTimestamp > currentStartTimestamp + currentDurationMs
  ) {
    return undefined;
  }

  let nearestPoint: MeasurementPoint | undefined;
  let nearestDistance = Number.POSITIVE_INFINITY;

  props.channels.forEach((channel) => {
    const values = channel.values ?? [];
    if (values.length === 0) {
      return;
    }

    const index =
      values.length === 1
        ? 0
        : Math.max(
            0,
            Math.min(
              values.length - 1,
              Math.round(
                ((clickedTimestamp - currentStartTimestamp) /
                  currentDurationMs) *
                  (values.length - 1),
              ),
            ),
          );
    const timestamp =
      currentStartTimestamp +
      (values.length === 1
        ? 0
        : (index / (values.length - 1)) * currentDurationMs);
    const value = Number(values[index]);
    const pixel = chart.convertToPixel({ gridIndex: 0 }, [timestamp, value]);
    if (!Array.isArray(pixel) || !Number.isFinite(value)) {
      return;
    }

    const distance =
      (Number(pixel[0]) - offsetX) ** 2 + (Number(pixel[1]) - offsetY) ** 2;
    if (distance < nearestDistance) {
      nearestDistance = distance;
      nearestPoint = {
        channelType: channel.channelType,
        timestamp,
        value,
      };
    }
  });

  return nearestPoint;
}

function handleChartPointerClick(event: { offsetX: number; offsetY: number }) {
  if (!measurementMode.value) {
    return;
  }

  const chart = getChartInstance();
  if (
    !chart ||
    !chart.containPixel({ gridIndex: 0 }, [event.offsetX, event.offsetY])
  ) {
    return;
  }
  const point = findNearestPoint(chart, event.offsetX, event.offsetY);
  if (!point) {
    return;
  }

  measurementPoints.value =
    measurementPoints.value.length >= 2
      ? [point]
      : [...measurementPoints.value, point];
  updateMeasurementOverlay();
}

async function renderChart() {
  const currentDurationMs = durationMs.value;
  const currentStartTimestamp = startTimestamp.value;
  const currentHasTimeAxis = hasTimeAxis.value;
  const baseSeries = (props.channels ?? []).map((channel, index) => {
    const color = getChannelColor(index);
    return {
      data: buildSeriesData(channel.values ?? [], currentStartTimestamp),
      itemStyle: { color },
      lineStyle: { color, width: 1 },
      name: channel.channelType,
      sampling: 'lttb' as const,
      showSymbol: false,
      type: 'line' as const,
    };
  });
  const measurementSeries = [
    {
      data: measurementPoints.value.map((point) => [
        point.timestamp,
        point.value,
      ]),
      id: measurementSeriesName,
      itemStyle: { color: '#faad14' },
      lineStyle: {
        color: '#faad14',
        type: 'dashed' as const,
        width: 2,
      },
      name: measurementSeriesName,
      showSymbol: true,
      silent: true,
      symbol: 'circle',
      symbolSize: 9,
      tooltip: { show: false },
      type: 'line' as const,
      z: 20,
    },
  ];

  const chart = await renderEcharts({
    animation: false,
    dataZoom: [
      {
        type: 'inside',
      },
      {
        bottom: 8,
        height: 18,
        type: 'slider',
      },
    ],
    grid: {
      bottom: 56,
      containLabel: true,
      left: 16,
      right: 24,
      top: 20,
    },
    series: [...baseSeries, ...measurementSeries],
    tooltip: {
      axisPointer: {
        type: 'line',
      },
      formatter: (params) => {
        const items = (Array.isArray(params) ? params : [params]).filter(
          (item) => item.seriesName !== measurementSeriesName,
        );
        const first = items[0];
        const firstValue = first?.value;
        const timestamp =
          currentHasTimeAxis && Array.isArray(firstValue)
            ? Number(firstValue[0])
            : Number.NaN;

        const rows = items
          .map((item) => {
            const channelType = item.seriesName ?? '';
            const unit = getChannelUnit(channelType);
            const itemValue = Array.isArray(item.value)
              ? Number(item.value[1])
              : Number(item.value);
            return `${item.marker ?? ''}${escapeHtml(channelType)}: ${
              Number.isFinite(itemValue) ? itemValue : '-'
            }${unit ? ` ${escapeHtml(unit)}` : ''}`;
          })
          .join('<br/>');

        if (!Number.isFinite(timestamp)) {
          return rows;
        }

        return `${$t('page.business.graphRelativeTime')}: ${formatElapsedTime(
          timestamp - currentStartTimestamp,
        )}<br/>${$t('page.business.graphActualTime')}: ${formatDateTime(
          timestamp,
        )}<br/>${rows}`;
      },
      trigger: 'axis',
    },
    xAxis: currentHasTimeAxis
      ? {
          axisTick: {
            show: false,
          },
          boundaryGap: [0, 0],
          max: currentStartTimestamp + currentDurationMs,
          min: currentStartTimestamp,
          name: $t('page.business.graphActualTime'),
          nameGap: 28,
          nameLocation: 'middle',
          type: 'time',
        }
      : {
          axisTick: {
            show: false,
          },
          boundaryGap: false,
          type: 'category',
        },
    yAxis: {
      name: $t('page.business.graphChannelValue'),
      scale: true,
      type: 'value',
    },
  });

  if (!chart) {
    return;
  }
  const zr = chart.getZr();
  zr.off('click', handleChartPointerClick);
  zr.on('click', handleChartPointerClick);
}

watch(
  () => [props.channels, props.durationMs, props.startTime] as const,
  () => {
    void renderChart();
  },
);

onMounted(() => {
  void renderChart();
});

onBeforeUnmount(() => {
  getChartInstance()?.getZr().off('click', handleChartPointerClick);
});
</script>

<template>
  <div
    class="flex flex-col gap-2"
    :class="{ 'graph-measure-mode': measurementMode }"
  >
    <div class="flex flex-wrap items-center justify-between gap-2 px-1">
      <div class="flex flex-wrap items-center gap-x-3 gap-y-1">
        <span
          v-for="(channel, index) in channels"
          :key="channel.channelType"
          class="inline-flex items-center gap-1 text-xs text-gray-600"
        >
          <span
            class="h-2 w-2 rounded-full"
            :style="{ backgroundColor: getChannelColor(index) }"
          ></span>
          <span>{{ channel.channelType }}</span>
        </span>
      </div>
      <Tooltip :title="$t('page.business.graphMeasureTip')">
        <Button
          :disabled="!hasTimeAxis"
          :icon="h(SwapOutlined)"
          size="small"
          :type="measurementMode ? 'primary' : 'default'"
          @click="toggleMeasurementMode"
        >
          {{ measurementButtonText }}
        </Button>
      </Tooltip>
    </div>

    <div
      v-if="firstMeasurementPoint"
      class="flex flex-wrap items-center gap-x-4 gap-y-1 rounded-md border border-border bg-gray-50 px-3 py-2 text-xs dark:bg-gray-900"
    >
      <span>P1: {{ formatDateTime(firstMeasurementPoint.timestamp) }}</span>
      <span class="text-gray-500">
        {{ formatPointValue(firstMeasurementPoint) }}
      </span>
      <template v-if="secondMeasurementPoint">
        <span>P2: {{ formatDateTime(secondMeasurementPoint.timestamp) }}</span>
        <span class="text-gray-500">
          {{ formatPointValue(secondMeasurementPoint) }}
        </span>
        <span class="font-semibold text-amber-600">
          {{ $t('page.business.graphTimeDifference') }}:
          {{ formatMeasurementDelta(measurementDeltaMs ?? 0) }}
        </span>
      </template>
      <Tooltip :title="$t('page.business.graphClearMeasurement')">
        <Button
          :icon="h(CloseOutlined)"
          size="small"
          type="text"
          @click="clearMeasurement"
        />
      </Tooltip>
    </div>

    <EchartsUI ref="chartRef" :height="height" />
  </div>
</template>

<style scoped>
.graph-measure-mode :deep(canvas) {
  cursor: crosshair;
}
</style>
