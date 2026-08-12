<script setup lang="ts">
import type { WeatherData } from '#/api';

import { computed, onBeforeUnmount, onMounted, ref } from 'vue';

import { ReloadOutlined } from '@ant-design/icons-vue';

import { getWeather } from '#/api';

const WEATHER_REFRESH_INTERVAL = 60 * 60 * 1000;

const DEFAULT_WEATHER: WeatherData = {
  city: '未知城市',
  district: '未知城市',
  report_time: '天气服务暂不可用',
  weather: '暂无数据',
  weather_icon: '999',
};

const weather = ref<WeatherData>();
const loading = ref(true);
const refreshing = ref(false);
const refreshStatus = ref<'error' | 'idle' | 'refreshing' | 'success'>('idle');
const isFallback = ref(false);
const lastRefreshedAt = ref<Date>();

const todayForecast = computed(() => weather.value?.forecast?.[0]);

const weatherMetrics = computed(() => {
  const w = weather.value;
  return [
    { label: '湿度', value: w ? `${w.humidity ?? '--'}%` : '--' },
    { label: '风向', value: w?.wind_direction ?? '--' },
    { label: '风力', value: w?.wind_power || '--' },
    { label: '能见度', value: w ? `${w.visibility ?? '--'} km` : '--' },
    { label: '气压', value: w ? `${w.pressure ?? '--'} hPa` : '--' },
    { label: '紫外线', value: w?.uv === undefined ? '--' : String(w.uv) },
    { label: '降水', value: w ? `${w.precipitation ?? '--'} mm` : '--' },
    { label: '云量', value: w ? `${w.cloud ?? '--'}%` : '--' },
    {
      label: '空气质量',
      value: w
        ? `${w.aqi_category ?? '--'}${w.aqi === undefined ? '' : ` (${w.aqi})`}${w.aqi_primary ? ` · ${w.aqi_primary}` : ''}`
        : '--',
    },
    {
      label: 'PM2.5',
      value: w ? `${w.air_pollutants?.pm25 ?? '--'} μg/m³` : '--',
    },
  ];
});

let refreshTimer: ReturnType<typeof setInterval> | undefined;
let statusTimer: ReturnType<typeof setTimeout> | undefined;

function setRefreshStatus(status: 'error' | 'idle' | 'refreshing' | 'success') {
  refreshStatus.value = status;
  if (statusTimer) {
    clearTimeout(statusTimer);
  }
  if (status === 'error' || status === 'success') {
    statusTimer = setTimeout(() => {
      refreshStatus.value = 'idle';
    }, 2500);
  }
}

async function loadWeather(force = false) {
  if (refreshing.value) {
    return;
  }
  refreshing.value = true;
  setRefreshStatus('refreshing');
  try {
    try {
      weather.value = await getWeather(undefined, force);
    } catch {
      weather.value = await getWeather('北京', force);
    }
    isFallback.value = false;
    lastRefreshedAt.value = new Date();
    setRefreshStatus('success');
  } catch {
    if (!weather.value) {
      weather.value = { ...DEFAULT_WEATHER };
      isFallback.value = true;
    }
    setRefreshStatus('error');
  } finally {
    refreshing.value = false;
    loading.value = false;
  }
}

function weatherIconSrc(icon?: string) {
  return icon ? `/icon/weather/icons/${icon}.svg` : '';
}

function formatRefreshTime(date: Date) {
  return date.toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    hour12: false,
    minute: '2-digit',
    second: '2-digit',
  });
}

function refreshStatusLabel() {
  if (refreshStatus.value === 'refreshing') {
    return '刷新中...';
  }
  if (refreshStatus.value === 'success') {
    return '已刷新';
  }
  if (refreshStatus.value === 'error') {
    return '刷新失败';
  }
  return '';
}

onMounted(async () => {
  await loadWeather();
  refreshTimer = setInterval(loadWeather, WEATHER_REFRESH_INTERVAL);
});

onBeforeUnmount(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer);
  }
  if (statusTimer) {
    clearTimeout(statusTimer);
  }
});
</script>

<template>
  <section v-if="weather" class="mt-4 w-full">
    <div class="rounded-2xl border border-border bg-card p-5 shadow-sm">
      <div class="flex flex-wrap items-center gap-x-5 gap-y-3">
        <div class="flex items-center gap-4">
          <div
            class="flex size-14 shrink-0 items-center justify-center rounded-2xl border border-primary/20 bg-primary/10"
          >
            <img
              v-if="weather.weather_icon"
              :alt="weather.weather || '天气图标'"
              :src="weatherIconSrc(weather.weather_icon)"
              class="h-10 w-10"
            />
            <span v-else class="text-xl font-semibold text-primary">
              {{ weather.weather }}
            </span>
          </div>
          <div class="min-w-0">
            <div
              class="flex flex-wrap items-center gap-x-2 text-sm text-foreground/75"
            >
              <span class="font-semibold text-foreground">
                {{ weather.district || weather.city }}
              </span>
              <template v-if="todayForecast">
                <span class="text-foreground/30">·</span>
                <span>今天 {{ todayForecast.week }}</span>
              </template>
              <template v-if="lastRefreshedAt">
                <span class="text-foreground/30">·</span>
                <span>今天 {{ formatRefreshTime(lastRefreshedAt) }} 刷新</span>
              </template>
              <span class="text-foreground/30">·</span>
              <span>{{ weather.report_time }}</span>
              <span
                v-if="isFallback"
                class="rounded bg-foreground/5 px-1.5 py-0.5 text-foreground/70"
              >
                默认展示
              </span>
            </div>
            <div class="mt-3 flex flex-wrap items-center gap-x-3 gap-y-2">
              <span class="text-3xl font-semibold leading-none">
                {{ weather.temperature ?? '--' }}°C
              </span>
              <span
                class="rounded-full border border-primary/20 bg-primary/10 px-3 py-1 text-xs font-medium text-primary"
              >
                {{ weather.weather }}
              </span>
              <span
                v-if="weather.feels_like !== undefined"
                class="rounded-full border border-primary/20 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary"
              >
                体感 {{ weather.feels_like }}°
              </span>
              <span
                v-if="todayForecast"
                class="rounded-full border border-primary/20 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary"
              >
                最高 {{ weather.temp_max ?? todayForecast.temp_max ?? '--' }}°
              </span>
              <span
                v-if="todayForecast"
                class="rounded-full border border-primary/20 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary"
              >
                最低 {{ weather.temp_min ?? todayForecast.temp_min ?? '--' }}°
              </span>
            </div>
          </div>
        </div>

        <div class="ml-auto flex min-w-0 items-center gap-2">
          <div
            v-if="weather.alerts && weather.alerts.length > 0"
            class="flex min-w-0 max-w-full items-center gap-2 rounded-full border border-border bg-foreground/5 px-2.5 py-1.5 text-sm"
          >
            <span class="truncate font-medium text-foreground">
              {{ weather.alerts?.[0]?.title }}
            </span>
            <span class="shrink-0 text-xs text-foreground/60">
              等 {{ weather.alerts.length }} 条预警
            </span>
          </div>
          <span
            v-if="refreshStatus !== 'idle'"
            aria-live="polite"
            class="inline-flex h-4 min-w-[64px] items-center justify-center text-xs"
            :class="{
              'text-primary': refreshStatus === 'refreshing',
              'text-foreground/60': refreshStatus === 'success',
              'text-red-500': refreshStatus === 'error',
            }"
          >
            {{ refreshStatusLabel() }}
          </span>
          <button
            type="button"
            :disabled="refreshing"
            class="flex size-8 shrink-0 items-center justify-center rounded-full bg-foreground/5 text-foreground transition hover:bg-foreground/10 disabled:cursor-not-allowed disabled:opacity-60"
            title="刷新天气"
            @click="loadWeather(true)"
          >
            <ReloadOutlined :class="{ 'animate-spin': refreshing }" />
          </button>
        </div>
      </div>

      <dl class="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-3 xl:grid-cols-5">
        <div
          v-for="metric in weatherMetrics"
          :key="metric.label"
          class="flex min-w-0 items-center justify-between gap-2 rounded-lg border border-border bg-foreground/[0.03] px-3 py-2"
        >
          <dt class="shrink-0 text-xs text-foreground/60">
            {{ metric.label }}
          </dt>
          <dd class="truncate text-sm font-medium">{{ metric.value }}</dd>
        </div>
      </dl>
    </div>
  </section>

  <div v-else-if="loading" class="mt-4 text-sm text-foreground/70">
    天气加载中...
  </div>
</template>
