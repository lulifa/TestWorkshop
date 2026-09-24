<script lang="ts" setup>
import type { WorkshopTelemetryTaskDto } from '@abp/core';

import { computed, h, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { formatToDateTime, WorkshopTelemetryStatus } from '@abp/core';
import { CopyOutlined } from '@ant-design/icons-vue';
import { Button, message, Tag } from 'ant-design-vue';

const row = ref<WorkshopTelemetryTaskDto>();

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[720px]',
  contentClass: 'p-0',
  onOpenChange: (isOpen: boolean) => {
    if (isOpen) {
      row.value = drawerApi.getData<WorkshopTelemetryTaskDto>();
    }
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.Telemetry:ReceivedParameters'),
});

const statusColorMap: Record<WorkshopTelemetryStatus, string> = {
  [WorkshopTelemetryStatus.Pending]: 'warning',
  [WorkshopTelemetryStatus.Processing]: 'processing',
  [WorkshopTelemetryStatus.Success]: 'success',
  [WorkshopTelemetryStatus.Failed]: 'error',
};

const statusLabelMap: Record<WorkshopTelemetryStatus, string> = {
  [WorkshopTelemetryStatus.Pending]: $t('TestWorkshop.Telemetry:Pending'),
  [WorkshopTelemetryStatus.Processing]: $t('TestWorkshop.Telemetry:Processing'),
  [WorkshopTelemetryStatus.Success]: $t('TestWorkshop.Telemetry:Success'),
  [WorkshopTelemetryStatus.Failed]: $t('TestWorkshop.Telemetry:Failed'),
};

const extraPropertyEntries = computed(() =>
  Object.entries(row.value?.extraProperties ?? {}).toSorted(([left], [right]) =>
    left.localeCompare(right),
  ),
);

const extraPropertiesJson = computed(() =>
  JSON.stringify(buildExtraPropertiesObject(), null, 2),
);

const propertyLabelMap: Record<string, string> = {
  'TelemetryInput.ChannelType': $t('TestWorkshop.Telemetry:ChannelType'),
  'TelemetryInput.CurrentFile': $t('TestWorkshop.Telemetry:FileName'),
  'TelemetryInput.DeviceCode': $t('TestWorkshop.Telemetry:DeviceCode'),
  'TelemetryInput.FileCount': $t('TestWorkshop.Telemetry:FileCount'),
  'TelemetryInput.FileTime': $t('TestWorkshop.Telemetry:FileTime'),
  'TelemetryInput.Group': $t('TestWorkshop.Telemetry:Group'),
  'TelemetryInput.IfSource': $t('TestWorkshop.Telemetry:IfSource'),
  'TelemetryInput.Source': $t('TestWorkshop.Telemetry:Source'),
  'TelemetryInput.TestInfo.EndTime': $t('TestWorkshop.Telemetry:EndTime'),
  'TelemetryInput.TestInfo.EndTime2': $t('TestWorkshop.Telemetry:EndTime2'),
  'TelemetryInput.TestInfo.PilotSN': $t('TestWorkshop.Telemetry:PilotSN'),
  'TelemetryInput.TestInfo.ShipName': $t('TestWorkshop.Telemetry:ShipName'),
  'TelemetryInput.TestInfo.StartTime': $t('TestWorkshop.Telemetry:StartTime'),
  'TelemetryInput.TestInfo.StartTime2': $t('TestWorkshop.Telemetry:StartTime2'),
  'TelemetryInput.TestInfo.TestBy': $t('TestWorkshop.Telemetry:TestBy'),
  'TelemetryInput.TestInfo.TestDate': $t('TestWorkshop.Telemetry:TestDate'),
  'TelemetryInput.TestInfo.TestedDeviceCode': $t(
    'TestWorkshop.Telemetry:TestedDeviceCode',
  ),
  'TelemetryInput.TestInfo.TestedDeviceName': $t(
    'TestWorkshop.Telemetry:TestedDeviceName',
  ),
};

function getPropertyLabel(key: string) {
  return propertyLabelMap[key] ?? key;
}

function getExtraPropertyValue(propertyName: string) {
  return row.value?.extraProperties?.[`TelemetryInput.${propertyName}`];
}

function buildExtraPropertiesObject() {
  const result: Record<string, any> = {};

  extraPropertyEntries.value.forEach(([key, value]) => {
    const path = key.startsWith('TelemetryInput.')
      ? key.slice('TelemetryInput.'.length)
      : key;
    const segments = path.split('.');
    let current = result;

    segments.forEach((segment, index) => {
      if (index === segments.length - 1) {
        current[segment] = value;
        return;
      }

      current[segment] ??= {};
      current = current[segment];
    });
  });

  return result;
}

async function copyExtraPropertiesJson() {
  await navigator.clipboard.writeText(extraPropertiesJson.value);
  message.success($t('TestWorkshop.Telemetry:CopyJsonSuccess'));
}

function formatPropertyValue(value: unknown) {
  if (value === null || value === undefined || value === '') {
    return '-';
  }
  if (typeof value === 'object') {
    return JSON.stringify(value, null, 2);
  }
  return String(value);
}

function formatFileSize(size?: number) {
  if (!size) {
    return '0 B';
  }
  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  let value = size;
  let index = 0;
  while (value >= 1024 && index < units.length - 1) {
    value /= 1024;
    index += 1;
  }
  return `${value.toFixed(index > 0 ? 2 : 0)} ${units[index]}`;
}
</script>

<template>
  <Drawer>
    <div v-if="row" class="flex h-full flex-col">
      <div class="border-b px-6 py-6">
        <div class="flex flex-wrap items-center gap-2">
          <Tag :color="statusColorMap[row.status]">
            {{ statusLabelMap[row.status] ?? row.statusName }}
          </Tag>
          <Tag v-if="getExtraPropertyValue('ChannelType')" color="blue">
            {{ getExtraPropertyValue('ChannelType') }}
          </Tag>
        </div>
        <h2 class="mt-3 break-all text-lg font-semibold leading-6">
          {{ row.fileName }}
        </h2>
      </div>

      <div class="border-b px-6 py-5">
        <div class="grid grid-cols-2 gap-x-8 gap-y-5">
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Telemetry:TaskId') }}
            </p>
            <p class="mt-1 font-mono text-sm">{{ row.id }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Telemetry:FileSize') }}
            </p>
            <p class="mt-1 text-sm">{{ formatFileSize(row.fileSize) }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Telemetry:SampleCount') }}
            </p>
            <p class="mt-1 text-sm">{{ row.recordCount ?? '-' }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.DisplayName:CreatedAt') }}
            </p>
            <p class="mt-1 text-sm">
              {{ formatToDateTime(row.createdAt) }}
            </p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Telemetry:RetryCount') }}
            </p>
            <p class="mt-1 text-sm">{{ row.retryCount }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Telemetry:NextRetryTime') }}
            </p>
            <p class="mt-1 text-sm">
              {{
                row.nextRetryTime ? formatToDateTime(row.nextRetryTime) : '-'
              }}
            </p>
          </div>
          <div>
            <p class="text-xs text-gray-400">
              {{ $t('TestWorkshop.Telemetry:ProcessedAt') }}
            </p>
            <p class="mt-1 text-sm">
              {{ row.processedAt ? formatToDateTime(row.processedAt) : '-' }}
            </p>
          </div>
        </div>
      </div>

      <div class="min-h-0 flex-1 overflow-auto px-6 py-5">
        <div class="mb-3 flex items-center justify-between">
          <span class="text-sm font-medium">
            {{ $t('TestWorkshop.Telemetry:ExtraProperties') }}
          </span>
          <div class="flex items-center gap-3">
            <span class="text-xs text-gray-400">
              {{ extraPropertyEntries.length }}
            </span>
            <Button
              :disabled="extraPropertyEntries.length === 0"
              :icon="h(CopyOutlined)"
              size="small"
              @click="copyExtraPropertiesJson"
            >
              {{ $t('TestWorkshop.Telemetry:CopyJson') }}
            </Button>
          </div>
        </div>

        <div
          v-if="extraPropertyEntries.length > 0"
          class="divide-y rounded-md border border-border"
        >
          <div
            v-for="[key, value] in extraPropertyEntries"
            :key="key"
            class="grid grid-cols-[220px_minmax(0,1fr)] gap-4 px-4 py-3"
          >
            <div class="break-all text-xs text-gray-500">
              {{ getPropertyLabel(key) }}
              <div class="mt-0.5 font-mono text-[11px] text-gray-400">
                {{ key }}
              </div>
            </div>
            <pre class="m-0 whitespace-pre-wrap break-all text-sm leading-5">{{
              formatPropertyValue(value)
            }}</pre>
          </div>
        </div>
        <div
          v-else
          class="rounded-md border border-dashed px-4 py-8 text-center text-sm text-gray-400"
        >
          {{ $t('TestWorkshop.Telemetry:NoExtraProperties') }}
        </div>
      </div>
    </div>
  </Drawer>
</template>

<style scoped></style>
