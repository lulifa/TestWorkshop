<script lang="ts" setup>
import type {
  DeviceType,
  WorkshopThresholdItemInput,
  WorkshopThresholdRuleDto,
} from '@abp/core';

import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  useAbpStore,
  useWorkshopDeviceApi,
  useWorkshopThresholdApi,
} from '@abp/core';
import {
  Button,
  Card,
  Input,
  InputNumber,
  message,
  Select,
  Switch,
  Table,
  Tag,
} from 'ant-design-vue';

defineOptions({
  name: 'WorkshopThresholdManagement',
});

const { getTypesApi } = useWorkshopDeviceApi();
const { getByDeviceTypeApi, saveApi } = useWorkshopThresholdApi();

const abpStore = useAbpStore();
const isAdmin = computed(
  () => abpStore.application?.currentUser.roles?.includes('admin') ?? false,
);

const deviceTypes = ref<
  { displayName: string; name: string; value: DeviceType }[]
>([]);
const selectedType = ref<DeviceType>();
const rows = ref<WorkshopThresholdRuleDto[]>([]);
const loading = ref(false);
const saving = ref(false);

const columns = [
  {
    dataIndex: 'dataItemName',
    title: $t('TestWorkshop.DisplayName:DataItemType'),
    width: 180,
  },
  {
    dataIndex: 'upperLimit',
    title: $t('TestWorkshop.DisplayName:UpperLimit'),
    width: 220,
  },
  {
    dataIndex: 'lowerLimit',
    title: $t('TestWorkshop.DisplayName:LowerLimit'),
    width: 220,
  },
  {
    dataIndex: 'alarmContent',
    title: $t('TestWorkshop.DisplayName:AlarmContent'),
    width: 240,
  },
  {
    dataIndex: 'enabled',
    title: $t('TestWorkshop.DisplayName:Enabled'),
    width: 100,
  },
];

async function loadDeviceTypes() {
  const { items } = await getTypesApi();
  deviceTypes.value = items;
  selectedType.value = items[0]?.value;
}

async function loadRules() {
  if (selectedType.value === undefined) {
    rows.value = [];
    return;
  }

  loading.value = true;
  try {
    const { items } = await getByDeviceTypeApi(selectedType.value);
    rows.value = items;
  } finally {
    loading.value = false;
  }
}

function validate(): boolean {
  for (const row of rows.value) {
    if (row.upperLimit === null && row.lowerLimit === null) {
      continue;
    }

    if (
      row.upperLimit !== null &&
      row.upperLimit !== undefined &&
      row.lowerLimit !== null &&
      row.lowerLimit !== undefined &&
      row.lowerLimit > row.upperLimit
    ) {
      message.error(
        `${row.dataItemName}：${$t('TestWorkshop.WorkshopThreshold:LimitRequired')}`,
      );
      return false;
    }
  }

  return true;
}

async function onSave() {
  if (selectedType.value === undefined || !validate()) {
    return;
  }

  saving.value = true;
  try {
    const items: WorkshopThresholdItemInput[] = rows.value.map((row) => ({
      alarmContent: row.alarmContent,
      dataItemType: row.dataItemType,
      enabled: row.enabled,
      lowerLimit: row.lowerLimit ?? null,
      upperLimit: row.upperLimit ?? null,
    }));

    await saveApi({ deviceType: selectedType.value, items });
    message.success($t('TestWorkshop.WorkshopThreshold:SaveSuccess'));
    await loadRules();
  } finally {
    saving.value = false;
  }
}

async function onTypeChange() {
  await loadRules();
}

onMounted(async () => {
  await loadDeviceTypes();
  await loadRules();
});
</script>

<template>
  <Page auto-content-height>
    <Card :title="$t('page.business.thresholds')">
      <div class="mb-4 flex flex-row items-center gap-3">
        <span class="text-sm">
          {{ $t('TestWorkshop.WorkshopThreshold:DeviceType') }}
        </span>
        <Select
          v-model:value="selectedType"
          :options="
            deviceTypes.map((item) => ({
              label: item.displayName || item.name,
              value: item.value,
            }))
          "
          class="w-64"
          @change="onTypeChange"
        />
        <Button v-if="isAdmin" :loading="saving" type="primary" @click="onSave">
          {{ $t('TestWorkshop.WorkshopThreshold:Save') }}
        </Button>
      </div>

      <div class="mb-3 text-xs text-foreground/60">
        注：只监控一侧时，另一侧留空即可；上限、下限都留空表示该数据类型不设阈值。
      </div>

      <Table
        :columns="columns"
        :data-source="rows"
        :loading="loading"
        :pagination="false"
        :scroll="{ x: 960 }"
        row-key="dataItemType"
        size="middle"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'dataItemName'">
            <div class="flex flex-row items-center gap-2">
              <span>{{ record.dataItemName }}</span>
              <Tag v-if="record.unit">{{ record.unit }}</Tag>
            </div>
          </template>
          <template v-else-if="column.dataIndex === 'upperLimit'">
            <InputNumber
              v-model:value="record.upperLimit"
              :disabled="!isAdmin"
              class="w-full"
            />
          </template>
          <template v-else-if="column.dataIndex === 'lowerLimit'">
            <InputNumber
              v-model:value="record.lowerLimit"
              :disabled="!isAdmin"
              class="w-full"
            />
          </template>
          <template v-else-if="column.dataIndex === 'alarmContent'">
            <Input
              v-model:value="record.alarmContent"
              :disabled="!isAdmin"
              :placeholder="$t('TestWorkshop.DisplayName:AlarmContent')"
            />
          </template>
          <template v-else-if="column.dataIndex === 'enabled'">
            <Switch v-model:checked="record.enabled" :disabled="!isAdmin" />
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>
