<script lang="ts" setup>
import type { WorkshopOrderDto, WorkshopTelemetryTaskDto } from '@abp/core';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  useWorkshopOrderApi,
  WorkshopTelemetryStatus,
} from '@abp/core';
import { Empty, Switch, Table, Tag, Tooltip } from 'ant-design-vue';

interface TreeRow {
  channelType?: string;
  children?: TreeRow[];
  count?: number;
  createdAt?: string;
  error?: string;
  fileName?: string;
  fileSize?: number;
  id?: number;
  key: string;
  label?: string;
  latestTaskId?: number;
  processedAt?: string;
  recordCount?: number;
  replaced?: boolean;
  shipName?: string;
  status?: number;
  statusName?: string;
  testBy?: string;
  type: 'batch' | 'device' | 'task';
}

const { getTelemetryTasksApi } = useWorkshopOrderApi();

const order = ref<WorkshopOrderDto>();
const loading = ref(false);
const tasks = ref<WorkshopTelemetryTaskDto[]>([]);
const expandedKeys = ref<string[]>([]);
const showReplaced = ref(false);

const replacedCount = computed(
  () => tasks.value.filter((task) => task.replacedByTaskId).length,
);

const hasFailedTask = computed(() =>
  tasks.value.some((task) => task.status === WorkshopTelemetryStatus.Failed),
);

/** 默认只看当前有效的任务；被替换的历史任务属于异常数据，需要时手动展开。 */
const visibleTasks = computed(() =>
  showReplaced.value
    ? tasks.value
    : tasks.value.filter((task) => !task.replacedByTaskId),
);

const columns = computed(() => [
  {
    dataIndex: 'fileName',
    ellipsis: true,
    key: 'name',
    title: $t('TestWorkshop.Telemetry:FileName'),
    width: 340,
  },
  {
    dataIndex: 'id',
    key: 'id',
    title: $t('page.business.taskIdCurrent'),
    width: 90,
  },
  {
    dataIndex: 'channelType',
    key: 'channelType',
    title: $t('TestWorkshop.Telemetry:ChannelType'),
    width: 130,
  },
  {
    dataIndex: 'status',
    key: 'status',
    title: $t('TestWorkshop.Telemetry:Status'),
    width: 120,
  },
  ...(showReplaced.value
    ? [
        {
          dataIndex: 'latestTaskId',
          key: 'latestTaskId',
          title: $t('page.business.replacedBy'),
          width: 110,
        },
      ]
    : []),
  {
    dataIndex: 'recordCount',
    key: 'recordCount',
    title: $t('TestWorkshop.Telemetry:SampleCount'),
    width: 120,
  },
  {
    dataIndex: 'fileSize',
    key: 'fileSize',
    title: $t('TestWorkshop.Telemetry:FileSize'),
    width: 110,
  },
  {
    dataIndex: 'testBy',
    key: 'testBy',
    title: $t('TestWorkshop.Telemetry:TestBy'),
    width: 110,
  },
  {
    dataIndex: 'shipName',
    key: 'shipName',
    title: $t('TestWorkshop.Telemetry:ShipName'),
    width: 160,
  },
  {
    dataIndex: 'createdAt',
    key: 'createdAt',
    title: $t('TestWorkshop.DisplayName:CreatedAt'),
    width: 170,
  },
  {
    dataIndex: 'processedAt',
    key: 'processedAt',
    title: $t('TestWorkshop.Telemetry:ProcessedAt'),
    width: 170,
  },
  ...(hasFailedTask.value
    ? [
        {
          dataIndex: 'error',
          key: 'error',
          title: $t('TestWorkshop.Telemetry:Error'),
          width: 220,
        },
      ]
    : []),
]);

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

const UNSET_DEVICE_KEY = '__unset__';

function getExtraValue(row: WorkshopTelemetryTaskDto, propertyName: string) {
  const value = row.extraProperties?.[`TelemetryInput.${propertyName}`];
  return value === null || value === undefined || value === ''
    ? undefined
    : String(value);
}

function resolveTestedDeviceCode(row: WorkshopTelemetryTaskDto) {
  return (
    row.testedDeviceCode ??
    getExtraValue(row, 'TestInfo.TestedDeviceCode') ??
    ''
  ).trim();
}

function resolveTestedDeviceName(row: WorkshopTelemetryTaskDto) {
  return (
    row.testedDeviceName ?? getExtraValue(row, 'TestInfo.TestedDeviceName')
  );
}

function resolveBatch(row: WorkshopTelemetryTaskDto) {
  return getExtraValue(row, 'Group') ?? getExtraValue(row, 'FileTime') ?? '-';
}

function sumSampleCount(list: WorkshopTelemetryTaskDto[]) {
  return list.reduce((total, item) => total + (item.recordCount ?? 0), 0);
}

function bucketBy<T>(list: T[], resolve: (item: T) => string) {
  const buckets = new Map<string, T[]>();
  list.forEach((item) => {
    const key = resolve(item);
    const bucket = buckets.get(key);
    if (bucket) {
      bucket.push(item);
    } else {
      buckets.set(key, [item]);
    }
  });
  return [...buckets.entries()].toSorted(([left], [right]) =>
    left.localeCompare(right),
  );
}

/**
 * 顺着替换链走到头，返回最终接管该条数据的任务ID。
 * 替换链上的中间任务会指向下一个，只有链尾的 ReplacedByTaskId 为空。
 */
function resolveLatestTaskId(
  taskId: number,
  taskMap: Map<number, WorkshopTelemetryTaskDto>,
) {
  const visited = new Set<number>();
  // 注意：后端 JSON 里该字段为空时是 null，不是 undefined，这里必须归一化，
  // 否则会顺着 null 继续走一环，把整条链的最终结果算成 null。
  let current = taskMap.get(taskId)?.replacedByTaskId ?? undefined;
  while (current !== undefined && !visited.has(current)) {
    visited.add(current);
    const next = taskMap.get(current)?.replacedByTaskId ?? undefined;
    if (next === undefined) {
      return current;
    }
    current = next;
  }
  return current;
}

const treeData = computed<TreeRow[]>(() => {
  const taskMap = new Map(tasks.value.map((task) => [task.id, task]));

  return bucketBy(
    visibleTasks.value,
    (task) => resolveTestedDeviceCode(task) || UNSET_DEVICE_KEY,
  ).map(([deviceKey, deviceTasks]) => {
    const first = deviceTasks[0];
    const code =
      deviceKey === UNSET_DEVICE_KEY || !first
        ? undefined
        : resolveTestedDeviceCode(first);
    const name =
      deviceKey === UNSET_DEVICE_KEY || !first
        ? undefined
        : resolveTestedDeviceName(first);

    const batches: TreeRow[] = bucketBy(deviceTasks, resolveBatch).map(
      ([batchKey, batchTasks]) => ({
        children: batchTasks
          .toSorted((left, right) =>
            (getExtraValue(left, 'ChannelType') ?? '').localeCompare(
              getExtraValue(right, 'ChannelType') ?? '',
            ),
          )
          .map((task) => ({
            channelType: getExtraValue(task, 'ChannelType'),
            createdAt: task.createdAt,
            error: task.error,
            fileName: task.fileName,
            fileSize: task.fileSize,
            id: task.id,
            key: `task:${task.id}`,
            latestTaskId: resolveLatestTaskId(task.id, taskMap),
            processedAt: task.processedAt,
            recordCount: task.recordCount,
            replaced: Boolean(task.replacedByTaskId),
            shipName: getExtraValue(task, 'TestInfo.ShipName'),
            status: task.status,
            statusName: task.statusName,
            testBy: getExtraValue(task, 'TestInfo.TestBy'),
            type: 'task' as const,
          })),
        count: batchTasks.length,
        key: `batch:${deviceKey}:${batchKey}`,
        label: batchKey,
        recordCount: sumSampleCount(batchTasks),
        type: 'batch' as const,
      }),
    );

    return {
      children: batches,
      count: deviceTasks.length,
      key: `device:${deviceKey}`,
      label: code
        ? `${code}${name ? ` · ${name}` : ''}`
        : $t('page.business.testedDeviceUnset'),
      recordCount: sumSampleCount(deviceTasks),
      type: 'device' as const,
    };
  });
});

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-[96vw] max-w-[96vw]',
  onOpenChange: async (isOpen: boolean) => {
    if (!isOpen) {
      return;
    }

    order.value = drawerApi.getData<WorkshopOrderDto>();
    tasks.value = [];
    expandedKeys.value = [];
    await loadTasks();
  },
  showConfirmButton: false,
  title: $t('TestWorkshop.WorkshopOrder:TelemetryTasks'),
});

async function loadTasks() {
  if (!order.value?.id) {
    return;
  }

  loading.value = true;
  try {
    const result = await getTelemetryTasksApi(order.value.id, {
      isPaged: false,
    });
    tasks.value = result.items ?? [];
    expandedKeys.value = treeData.value.flatMap((device) => [
      device.key,
      ...(device.children ?? []).map((batch) => batch.key),
    ]);
  } finally {
    loading.value = false;
  }
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

function getStatusColor(status?: number) {
  return status === undefined
    ? 'default'
    : (statusColorMap[status as WorkshopTelemetryStatus] ?? 'default');
}

function getStatusLabel(status?: number, statusName?: string) {
  if (status === undefined) {
    return statusName ?? '-';
  }
  return (
    statusLabelMap[status as WorkshopTelemetryStatus] ??
    statusName ??
    String(status)
  );
}

function onExpand(expanded: boolean, record: TreeRow) {
  expandedKeys.value = expanded
    ? [...expandedKeys.value, record.key]
    : expandedKeys.value.filter((key) => key !== record.key);
}
</script>

<template>
  <Drawer>
    <div
      class="mb-4 flex items-center justify-between gap-4 rounded-md border border-border px-4 py-3"
    >
      <div class="min-w-0">
        <div class="text-sm font-medium">{{ order?.orderNo }}</div>
        <div class="mt-1 text-xs text-gray-500">
          {{ order?.deviceCode }} - {{ order?.deviceName }}
        </div>
      </div>
      <label
        v-if="replacedCount > 0"
        class="flex shrink-0 cursor-pointer items-center gap-2 text-xs text-gray-500"
      >
        {{ $t('page.business.showReplaced') }}（{{ replacedCount }}）
        <Switch v-model:checked="showReplaced" size="small" />
      </label>
    </div>

    <Empty
      v-if="!loading && treeData.length === 0"
      class="py-16"
      :description="$t('common.noData')"
    />

    <Table
      v-else
      :columns="columns"
      :data-source="treeData"
      :expanded-row-keys="expandedKeys"
      :loading="loading"
      :pagination="false"
      :row-key="(record: TreeRow) => record.key"
      :scroll="{ x: 1850 }"
      size="small"
      @expand="onExpand"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'name'">
          <span
            v-if="record.type === 'task'"
            class="block max-w-[320px] truncate"
            :title="record.fileName"
          >
            {{ record.fileName }}
          </span>
          <span v-else class="font-medium">
            {{ record.label }}
            <span class="ml-2 text-xs text-gray-400">{{ record.count }}</span>
          </span>
        </template>
        <template v-else-if="column.key === 'channelType'">
          <Tag v-if="record.type === 'task' && record.channelType" color="blue">
            {{ record.channelType }}
          </Tag>
        </template>
        <template v-else-if="column.key === 'id'">
          <span v-if="record.type === 'task'" class="font-mono text-xs">
            {{ record.id }}
          </span>
        </template>
        <template v-else-if="column.key === 'status'">
          <template v-if="record.type === 'task'">
            <Tag v-if="record.replaced" color="default">
              {{ $t('TestWorkshop.Telemetry:Replaced') }}
            </Tag>
            <Tag v-else :color="getStatusColor(record.status)">
              {{ getStatusLabel(record.status, record.statusName) }}
            </Tag>
          </template>
        </template>
        <template v-else-if="column.key === 'latestTaskId'">
          <span
            v-if="record.type === 'task' && record.latestTaskId"
            class="font-mono text-xs"
          >
            {{ record.latestTaskId }}
          </span>
        </template>
        <template v-else-if="column.key === 'testBy'">
          <span v-if="record.type === 'task'">{{ record.testBy || '-' }}</span>
        </template>
        <template v-else-if="column.key === 'shipName'">
          <span v-if="record.type === 'task'">{{
            record.shipName || '-'
          }}</span>
        </template>
        <template v-else-if="column.key === 'fileSize'">
          <span v-if="record.type === 'task'">
            {{ formatFileSize(record.fileSize) }}
          </span>
        </template>
        <template v-else-if="column.key === 'recordCount'">
          {{ record.recordCount ?? '-' }}
        </template>
        <template v-else-if="column.key === 'createdAt'">
          {{
            record.type === 'task' && record.createdAt
              ? formatToDateTime(record.createdAt)
              : ''
          }}
        </template>
        <template v-else-if="column.key === 'processedAt'">
          {{
            record.type === 'task' && record.processedAt
              ? formatToDateTime(record.processedAt)
              : ''
          }}
        </template>
        <template v-else-if="column.key === 'error'">
          <Tooltip
            v-if="record.type === 'task' && record.error"
            :title="record.error"
          >
            <span class="block max-w-[200px] truncate text-red-500">
              {{ record.error }}
            </span>
          </Tooltip>
        </template>
      </template>
    </Table>
  </Drawer>
</template>

<style scoped></style>
