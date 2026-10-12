<script lang="ts" setup>
import type { WorkshopTelemetryGraphChannelDto } from '@abp/core';

import type { EchartsUIType } from '@vben/plugins/echarts';

import { onMounted, ref, watch } from 'vue';

import { EchartsUI, useEcharts } from '@vben/plugins/echarts';

interface Props {
  channels: WorkshopTelemetryGraphChannelDto[];
  height?: string;
}

const props = withDefaults(defineProps<Props>(), {
  height: '460px',
});

const chartRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(chartRef);

function renderChart() {
  void renderEcharts({
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
      top: 44,
    },
    legend: {
      top: 8,
      type: 'scroll',
    },
    series: (props.channels ?? []).map((channel) => ({
      data: channel.values ?? [],
      lineStyle: { width: 1 },
      name: channel.channelType,
      sampling: 'lttb' as const,
      showSymbol: false,
      type: 'line' as const,
    })),
    tooltip: {
      axisPointer: {
        type: 'line',
      },
      trigger: 'axis',
    },
    xAxis: {
      axisTick: {
        show: false,
      },
      boundaryGap: false,
      type: 'category',
    },
    yAxis: {
      scale: true,
      type: 'value',
    },
  });
}

watch(() => props.channels, renderChart);

onMounted(renderChart);
</script>

<template>
  <EchartsUI ref="chartRef" :height="height" />
</template>

<style scoped></style>
