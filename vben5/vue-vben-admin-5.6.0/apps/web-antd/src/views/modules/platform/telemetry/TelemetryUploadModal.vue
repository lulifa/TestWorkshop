<script lang="ts" setup>
import type { WorkshopDeviceDto, WorkshopTelemetryFileInput } from '@abp/core';
import type { UploadFile } from 'ant-design-vue';

import { h, ref } from 'vue';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useWorkshopDeviceApi, useWorkshopTelemetryApi } from '@abp/core';
import { UploadOutlined } from '@ant-design/icons-vue';
import { Button, message, Upload } from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change'): void;
}>();

const channelTypeOptions = [
  { label: '先导驱动反馈 (PilotDriveFb)', value: 'PilotDriveFb' },
  { label: 'FIVA 阀反馈电流 (FivaFbCurrent)', value: 'FivaFbCurrent' },
  { label: '柱塞反馈 (PlungerFb)', value: 'PlungerFb' },
  { label: '出口工作压力 (FopPressure)', value: 'FopPressure' },
  { label: '背压 (BackPressure)', value: 'BackPressure' },
  { label: '流量 (Flow)', value: 'Flow' },
  { label: '控制给定指令 (Cmd)', value: 'Cmd' },
  { label: '先导位置反馈 (PilotPosFb)', value: 'PilotPosFb' },
];

const { getListApi: getDeviceListApi } = useWorkshopDeviceApi();
const { uploadApi } = useWorkshopTelemetryApi();

const generatedFileName = ref('');
const selectedFile = ref<File>();
const selectedFileList = ref<UploadFile[]>([]);

const [Form, formApi] = useVbenForm({
  commonConfig: {
    componentProps: {
      class: 'w-full',
    },
  },
  handleSubmit: onSubmit,
  schema: [
    {
      component: 'ApiSelect',
      componentProps: {
        afterFetch: (result: { items: WorkshopDeviceDto[] }) => {
          return result.items.map((item) => ({
            label: `${item.code} - ${item.name}`,
            value: item.code,
          }));
        },
        allowClear: false,
        api: getDeviceListApi,
        labelField: 'label',
        optionFilterProp: 'label',
        params: {
          isPaged: false,
          maxResultCount: 1000,
        },
        showSearch: true,
        valueField: 'value',
      },
      fieldName: 'deviceCode',
      label: $t('TestWorkshop.Telemetry:DeviceCode'),
      rules: 'selectRequired',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: false,
        options: channelTypeOptions,
      },
      fieldName: 'channelType',
      label: $t('TestWorkshop.Telemetry:ChannelType'),
      rules: 'selectRequired',
    },
    {
      component: 'Input',
      fieldName: 'testedDeviceCode',
      label: $t('TestWorkshop.Telemetry:TestedDeviceCode'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'testedDeviceName',
      label: $t('TestWorkshop.Telemetry:TestedDeviceName'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'pilotSN',
      label: $t('TestWorkshop.Telemetry:PilotSN'),
    },
    {
      component: 'Input',
      fieldName: 'shipName',
      label: $t('TestWorkshop.Telemetry:ShipName'),
    },
    {
      component: 'Input',
      fieldName: 'testBy',
      label: $t('TestWorkshop.Telemetry:TestBy'),
    },
    {
      component: 'InputNumber',
      componentProps: {
        max: 1_000_000,
        min: 1,
      },
      fieldName: 'recordCount',
      label: $t('TestWorkshop.Telemetry:RecordCount'),
      rules: 'required',
    },
  ],
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  onConfirm: async () => {
    await formApi.validateAndSubmitForm();
  },
  onOpenChange: (isOpen) => {
    if (isOpen) {
      onInit();
    }
  },
  title: $t('TestWorkshop.Telemetry:SimulateUpload'),
});

function onInit() {
  formApi.resetForm();
  selectedFile.value = undefined;
  selectedFileList.value = [];
  generatedFileName.value = buildGeneratedFileName();
  formApi.setValues({
    channelType: 'PilotDriveFb',
    recordCount: 128,
    testedDeviceCode: 'DUT-A1',
    testedDeviceName: '水泵A',
  });
}

function onBeforeUpload(file: File) {
  if (!file.name.toLowerCase().endsWith('.csv')) {
    message.warning('仅支持 .csv 文件');
    return false;
  }

  selectedFile.value = file;
  selectedFileList.value = [
    {
      name: file.name,
      originFileObj: file,
      size: file.size,
      status: 'done',
      uid: `${Date.now()}-${file.name}`,
    } as UploadFile,
  ];
  return false;
}

function onRemoveFile() {
  selectedFile.value = undefined;
  selectedFileList.value = [];
}

async function onSubmit(values: Record<string, any>) {
  const file = selectedFile.value ?? buildCsvFile(values);
  await uploadFile(file, buildInput(values, file));
}

async function uploadFile(file: File, input: WorkshopTelemetryFileInput) {
  try {
    modalApi.setState({ submitting: true });
    await uploadApi(file, input);
    message.success($t('AbpUi.SavedSuccessfully'));
    emits('change');
    modalApi.close();
  } finally {
    modalApi.setState({ submitting: false });
  }
}

function buildGeneratedFileName() {
  const now = new Date();
  const pad = (value: number) => String(value).padStart(2, '0');
  return `simulated-telemetry-${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}-${pad(now.getHours())}${pad(now.getMinutes())}${pad(now.getSeconds())}.csv`;
}

function buildInput(
  values: Record<string, any>,
  file: File,
): WorkshopTelemetryFileInput {
  const now = new Date();
  const source = buildSource(now);

  return {
    channelType: values.channelType,
    currentFile: file.name,
    deviceCode: values.deviceCode,
    fileCount: 1,
    group: `${values.deviceCode}_${source}`,
    source,
    testInfo: {
      pilotSN: values.pilotSN,
      shipName: values.shipName,
      startTime: formatDateTime(now),
      testBy: values.testBy,
      testDate: formatDate(now),
      testedDeviceCode: values.testedDeviceCode,
      testedDeviceName: values.testedDeviceName,
    },
  };
}

function buildCsvFile(values: Record<string, any>): File {
  const recordCount = Number(values.recordCount);
  const lines: string[] = [];

  for (let index = 0; index < recordCount; index += 1) {
    const value = generateChannelValue(values.channelType, index);
    lines.push(`${index},${value.toFixed(4)}`);
  }

  const blob = new Blob([lines.join('\n')], {
    type: 'text/csv',
  });
  return new File([blob], generatedFileName.value, {
    type: 'text/csv',
  });
}

function generateChannelValue(channelType: string, index: number) {
  const wave = Math.sin(index / 10);
  switch (channelType) {
    case 'BackPressure': {
      return 5 + wave + Math.random() * 0.2;
    }
    case 'Cmd': {
      return 50 + wave * 10;
    }
    case 'FivaFbCurrent': {
      return 12 + wave * 1.5 + Math.random() * 0.2;
    }
    case 'Flow': {
      return 30 + wave * 5 + Math.random();
    }
    case 'FopPressure': {
      return 20 + wave * 3 + Math.random() * 0.5;
    }
    case 'PilotPosFb':
    case 'PlungerFb': {
      return 25 + wave * 2 + Math.random() * 0.2;
    }
    default: {
      return 45 + wave * 5 + Math.random() * 0.5;
    }
  }
}

function formatDateTime(value: Date) {
  return `${formatDate(value)} ${String(value.getHours()).padStart(2, '0')}:${String(value.getMinutes()).padStart(2, '0')}:${String(value.getSeconds()).padStart(2, '0')}`;
}

function formatDate(value: Date) {
  const pad = (part: number) => String(part).padStart(2, '0');
  return `${value.getFullYear()}-${pad(value.getMonth() + 1)}-${pad(value.getDate())}`;
}

function buildSource(value: Date) {
  const pad = (part: number) => String(part).padStart(2, '0');
  return `@${value.getFullYear()}-${value.getMonth() + 1}-${value.getDate()}-${pad(value.getHours())}-${pad(value.getMinutes())}-${pad(value.getSeconds())}`;
}
</script>

<template>
  <Modal>
    <div class="flex flex-col gap-4">
      <div
        class="flex items-center justify-between gap-3 rounded-md border border-dashed border-border p-3"
      >
        <div class="min-w-0">
          <div class="truncate text-sm font-medium">
            {{
              selectedFile?.name ?? $t('TestWorkshop.Telemetry:NoCsvSelected')
            }}
          </div>
          <div class="text-xs text-gray-500">
            {{
              selectedFile
                ? $t('TestWorkshop.Telemetry:UseSelectedCsv')
                : $t('TestWorkshop.Telemetry:GenerateMode')
            }}
          </div>
        </div>
        <div class="flex shrink-0 items-center gap-2">
          <Button
            v-if="selectedFile"
            danger
            size="small"
            type="text"
            @click="onRemoveFile"
          >
            {{ $t('AbpUi.Delete') }}
          </Button>
          <Upload
            accept=".csv"
            :before-upload="onBeforeUpload"
            :file-list="selectedFileList"
            :max-count="1"
            :show-upload-list="false"
          >
            <Button :icon="h(UploadOutlined)" size="small">
              {{ $t('TestWorkshop.Telemetry:SelectCsv') }}
            </Button>
          </Upload>
        </div>
      </div>

      <div class="text-xs text-gray-500">
        {{ $t('TestWorkshop.Telemetry:GeneratedFileName') }}:
        {{ selectedFile?.name ?? generatedFileName }}
      </div>

      <Form />
    </div>
  </Modal>
</template>

<style scoped></style>
