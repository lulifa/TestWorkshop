<script lang="ts" setup>
import type {
  WorkshopRealtimeDashboardDto,
  WorkshopRealtimeDeviceDto,
} from '@abp/core';

import { computed, h, onMounted, onUnmounted, ref } from 'vue';

import { Page } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { formatToDateTime, useWorkshopRealtimeDataApi } from '@abp/core';
import {
  ApartmentOutlined,
  ClockCircleOutlined,
  ReloadOutlined,
  SearchOutlined,
  WifiOutlined,
} from '@ant-design/icons-vue';
import { Button, Input, Spin } from 'ant-design-vue';

defineOptions({
  name: 'RealtimeAnalytics',
});

const { getDashboardApi } = useWorkshopRealtimeDataApi();

const data = ref<WorkshopRealtimeDashboardDto>();
const loading = ref(false);
const searchText = ref('');
const selectedWorkshop = ref('all');
let refreshTimer: ReturnType<typeof setInterval> | undefined;

const devices = computed(() => data.value?.devices ?? []);
const workshopGroups = computed(() => {
  const groups = new Map<string, WorkshopRealtimeDeviceDto[]>();
  for (const device of devices.value) {
    const name = device.organizationUnitName?.trim() || '-';
    const group = groups.get(name) ?? [];
    group.push(device);
    groups.set(name, group);
  }

  return [...groups.entries()].map(([name, items]) => ({
    devices: items,
    name,
  }));
});

const onlineDeviceCount = computed(() => devices.value.length);
const workshopCount = computed(() => workshopGroups.value.length);
const workshopOptions = computed(() =>
  workshopGroups.value.map((group) => ({
    count: group.devices.length,
    name: group.name,
  })),
);
const filteredDevices = computed(() => {
  const keyword = searchText.value.trim().toLowerCase();
  return devices.value.filter((device) => {
    const matchesWorkshop =
      selectedWorkshop.value === 'all' ||
      (device.organizationUnitName?.trim() || '-') === selectedWorkshop.value;
    if (!matchesWorkshop) {
      return false;
    }

    if (!keyword) {
      return true;
    }

    return [device.deviceName, device.deviceCode].some((value) =>
      value?.toLowerCase().includes(keyword),
    );
  });
});

async function refresh() {
  loading.value = true;
  try {
    data.value = await getDashboardApi();
    const workshopNames = new Set(
      data.value.devices.map(
        (device) => device.organizationUnitName?.trim() || '-',
      ),
    );
    if (
      selectedWorkshop.value !== 'all' &&
      !workshopNames.has(selectedWorkshop.value)
    ) {
      selectedWorkshop.value = 'all';
    }
  } catch {
    return;
  } finally {
    loading.value = false;
  }
}

function formatMetric(value?: number) {
  if (value === null || value === undefined) {
    return '--';
  }

  return new Intl.NumberFormat('zh-CN', {
    maximumFractionDigits: 2,
  }).format(value);
}

function formatOnlineDuration(seconds: number) {
  const totalMinutes = Math.max(0, Math.floor(seconds / 60));
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  return `${hours} 小时 ${minutes} 分钟`;
}

function formatTime(value?: string) {
  return value ? formatToDateTime(value) : $t('page.dashboard.dataPending');
}

function startAutoRefresh() {
  stopAutoRefresh();
  const seconds = Math.max(data.value?.refreshAfterSeconds ?? 60, 30);
  refreshTimer = setInterval(refresh, seconds * 1000);
}

function stopAutoRefresh() {
  if (refreshTimer) {
    clearInterval(refreshTimer);
    refreshTimer = undefined;
  }
}

onMounted(async () => {
  await refresh();
  startAutoRefresh();
});

onUnmounted(stopAutoRefresh);
</script>

<template>
  <Page auto-content-height>
    <div
      class="dashboard-shell flex h-full min-h-0 flex-col gap-5 overflow-hidden p-5 text-slate-900"
    >
      <header
        class="flex flex-col gap-4 border-b border-slate-200/80 pb-4 lg:flex-row lg:items-end lg:justify-between"
      >
        <div class="min-w-0">
          <div class="flex flex-wrap items-center gap-3">
            <span class="relative flex size-2.5">
              <span
                class="absolute inline-flex size-full animate-ping rounded-full bg-blue-500 opacity-60"
              ></span>
              <span
                class="relative inline-flex size-2.5 rounded-full bg-blue-500"
              ></span>
            </span>
            <h1 class="truncate text-xl font-semibold text-slate-900">
              {{ $t('page.dashboard.realtimeTitle') }}
            </h1>
            <span
              class="inline-flex items-center rounded-full border border-blue-200 bg-blue-50 px-2.5 py-1 text-[11px] font-medium text-blue-700"
            >
              LIVE
            </span>
          </div>
          <p class="mt-1 text-sm text-slate-500">
            {{ $t('page.dashboard.realtimeSubtitle') }}
          </p>
        </div>
        <div class="flex flex-wrap items-center gap-3">
          <span class="text-xs text-slate-400">
            {{ $t('page.dashboard.autoRefreshHint') }}
          </span>
          <Button
            :icon="h(ReloadOutlined)"
            :loading="loading"
            type="primary"
            @click="refresh"
          >
            {{ $t('page.dashboard.refresh') }}
          </Button>
        </div>
      </header>

      <div
        class="grid grid-cols-1 divide-y divide-slate-200/80 rounded-lg border border-slate-200 bg-white shadow-sm sm:grid-cols-3 sm:divide-x sm:divide-y-0"
      >
        <div class="px-5 py-4">
          <div class="flex items-center gap-2 text-xs text-slate-500">
            <WifiOutlined class="text-blue-600" />
            {{ $t('page.dashboard.onlineDevices') }}
          </div>
          <div
            class="mt-1.5 text-2xl font-semibold tabular-nums text-slate-900"
          >
            {{ onlineDeviceCount }}
          </div>
        </div>
        <div class="px-5 py-4">
          <div class="flex items-center gap-2 text-xs text-slate-500">
            <ApartmentOutlined class="text-blue-600" />
            {{ $t('page.dashboard.workshopCount') }}
          </div>
          <div
            class="mt-1.5 text-2xl font-semibold tabular-nums text-slate-900"
          >
            {{ workshopCount }}
          </div>
        </div>
        <div class="px-5 py-4">
          <div class="flex items-center gap-2 text-xs text-slate-500">
            <ClockCircleOutlined class="text-blue-600" />
            {{ $t('page.dashboard.lastUpdated') }}
          </div>
          <div
            class="mt-1.5 truncate text-lg font-semibold tabular-nums text-slate-900"
          >
            {{ formatTime(data?.generatedAt) }}
          </div>
        </div>
      </div>

      <div class="flex min-h-0 flex-1 gap-4">
        <aside
          class="hidden w-[230px] shrink-0 flex-col rounded-lg border border-slate-200 bg-white shadow-sm lg:flex"
        >
          <div class="border-b border-slate-100 p-2">
            <button
              class="flex w-full items-center justify-between rounded-md border px-3 py-2 text-left text-sm transition-colors"
              :class="
                selectedWorkshop === 'all'
                  ? 'border-blue-200 bg-blue-50 text-blue-700'
                  : 'border-transparent text-slate-600 hover:bg-slate-50'
              "
              type="button"
              @click="selectedWorkshop = 'all'"
            >
              <span class="flex min-w-0 items-center gap-2">
                <ApartmentOutlined class="text-blue-500" />
                <span class="truncate">
                  {{ $t('page.dashboard.allWorkshops') }}
                </span>
              </span>
              <span class="text-xs tabular-nums">{{ onlineDeviceCount }}</span>
            </button>
          </div>
          <div class="min-h-0 flex-1 space-y-1 overflow-auto p-2">
            <button
              v-for="workshop in workshopOptions"
              :key="workshop.name"
              class="flex w-full items-center justify-between rounded-md border px-3 py-2 text-left text-sm transition-colors"
              :class="
                selectedWorkshop === workshop.name
                  ? 'border-blue-200 bg-blue-50 text-blue-700'
                  : 'border-transparent text-slate-600 hover:bg-slate-50'
              "
              type="button"
              @click="selectedWorkshop = workshop.name"
            >
              <span class="truncate">{{ workshop.name }}</span>
              <span class="text-xs tabular-nums">{{ workshop.count }}</span>
            </button>
          </div>
        </aside>

        <div class="flex min-w-0 flex-1 flex-col gap-3">
          <div class="flex gap-2 overflow-x-auto pb-1 lg:hidden">
            <button
              class="shrink-0 rounded-full border px-3 py-1.5 text-xs"
              :class="
                selectedWorkshop === 'all'
                  ? 'border-blue-200 bg-blue-50 text-blue-700'
                  : 'border-slate-200 bg-white text-slate-600'
              "
              type="button"
              @click="selectedWorkshop = 'all'"
            >
              {{ $t('page.dashboard.allWorkshops') }}
              ({{ onlineDeviceCount }})
            </button>
            <button
              v-for="workshop in workshopOptions"
              :key="workshop.name"
              class="shrink-0 rounded-full border px-3 py-1.5 text-xs"
              :class="
                selectedWorkshop === workshop.name
                  ? 'border-blue-200 bg-blue-50 text-blue-700'
                  : 'border-slate-200 bg-white text-slate-600'
              "
              type="button"
              @click="selectedWorkshop = workshop.name"
            >
              {{ workshop.name }} ({{ workshop.count }})
            </button>
          </div>

          <div class="flex flex-wrap items-center justify-between gap-3">
            <div class="text-sm font-medium text-slate-700">
              {{
                selectedWorkshop === 'all'
                  ? $t('page.dashboard.allWorkshops')
                  : selectedWorkshop
              }}
              <span class="ml-1 text-xs font-normal text-slate-400">
                {{ filteredDevices.length }}
              </span>
            </div>
            <Input
              v-model:value="searchText"
              allow-clear
              class="w-full sm:w-[280px]"
              :placeholder="$t('page.dashboard.searchDevice')"
            >
              <template #prefix>
                <SearchOutlined class="text-slate-400" />
              </template>
            </Input>
          </div>

          <div class="min-h-0 flex-1 overflow-auto pr-1">
            <Spin :spinning="loading">
              <div
                v-if="!loading && filteredDevices.length === 0"
                class="flex min-h-[420px] flex-col items-center justify-center gap-3 text-slate-500"
              >
                <WifiOutlined class="text-4xl text-slate-300" />
                <div class="text-sm">
                  {{ $t('page.dashboard.noOnlineDevices') }}
                </div>
              </div>

              <div
                v-else
                class="grid grid-cols-1 gap-4 lg:grid-cols-2 2xl:grid-cols-3"
              >
                <article
                  v-for="device in filteredDevices"
                  :key="device.deviceId"
                  class="relative overflow-hidden rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:border-sky-300 hover:shadow-lg"
                >
                  <div
                    class="absolute inset-x-0 top-0 h-px bg-blue-500/50"
                    aria-hidden="true"
                  ></div>
                  <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                      <div class="flex items-center gap-2">
                        <span
                          class="size-2 shrink-0 rounded-full bg-emerald-500 shadow-[0_0_0_4px_rgba(16,185,129,0.10)]"
                        ></span>
                        <h3
                          class="truncate text-base font-semibold text-slate-900"
                          :title="device.deviceName"
                        >
                          {{ device.deviceName }}
                        </h3>
                      </div>
                      <div
                        class="mt-1.5 flex min-w-0 items-center gap-2 text-xs"
                      >
                        <span
                          class="truncate font-mono text-slate-400"
                          :title="device.deviceCode"
                        >
                          {{ device.deviceCode }}
                        </span>
                        <span class="truncate text-blue-600">
                          {{ device.organizationUnitName || '-' }}
                        </span>
                      </div>
                    </div>
                    <span
                      class="shrink-0 rounded border border-blue-100 bg-blue-50 px-2.5 py-1 text-[11px] font-medium text-blue-700"
                    >
                      {{ device.deviceTypeName || device.deviceType }}
                    </span>
                  </div>

                  <div class="mt-4 grid grid-cols-3 gap-2.5">
                    <div
                      class="rounded-md border border-slate-100 bg-slate-50 px-3 py-2.5"
                    >
                      <div class="text-xs text-slate-500">
                        {{ $t('page.dashboard.pressure') }}
                      </div>
                      <div
                        class="mt-1.5 truncate text-2xl font-semibold tabular-nums text-blue-600"
                      >
                        {{ formatMetric(device.pressure) }}
                      </div>
                      <div class="text-[11px] text-slate-500">MPa</div>
                    </div>
                    <div
                      class="rounded-md border border-slate-100 bg-slate-50 px-3 py-2.5"
                    >
                      <div class="text-xs text-slate-500">
                        {{ $t('page.dashboard.flow') }}
                      </div>
                      <div
                        class="mt-1.5 truncate text-2xl font-semibold tabular-nums text-emerald-600"
                      >
                        {{ formatMetric(device.flow) }}
                      </div>
                      <div class="text-[11px] text-slate-500">L/min</div>
                    </div>
                    <div
                      class="rounded-md border border-slate-100 bg-slate-50 px-3 py-2.5"
                    >
                      <div class="text-xs text-slate-500">
                        {{ $t('page.dashboard.temperature') }}
                      </div>
                      <div
                        class="mt-1.5 truncate text-2xl font-semibold tabular-nums text-amber-600"
                      >
                        {{ formatMetric(device.temperature) }}
                      </div>
                      <div class="text-[11px] text-slate-500">℃</div>
                    </div>
                  </div>

                  <div
                    class="mt-4 flex flex-wrap items-center justify-between gap-x-4 gap-y-1 border-t border-slate-100 pt-3 text-[11px] text-slate-500"
                  >
                    <div class="flex items-center gap-1.5">
                      <ClockCircleOutlined />
                      <span>{{ $t('page.dashboard.onlineDuration') }}</span>
                      <span
                        class="whitespace-nowrap font-medium tabular-nums text-slate-700"
                      >
                        {{ formatOnlineDuration(device.onlineDurationSeconds) }}
                      </span>
                    </div>
                    <div class="flex items-center gap-1.5">
                      <span>{{ $t('page.dashboard.latestData') }}</span>
                      <span
                        class="whitespace-nowrap font-medium tabular-nums text-slate-700"
                        :title="formatTime(device.dataTimestamp)"
                      >
                        {{ formatTime(device.dataTimestamp) }}
                      </span>
                    </div>
                  </div>
                </article>
              </div>
            </Spin>
          </div>
        </div>
      </div>
    </div>
  </Page>
</template>

<style scoped>
.dashboard-shell {
  background-color: #f3f8fd;
  background-image: radial-gradient(
    rgba(37, 99, 235, 0.08) 1px,
    transparent 1px
  );
  background-size: 22px 22px;
}
</style>
