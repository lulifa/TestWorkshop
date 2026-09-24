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

const selectedFile = ref<File>();
const selectedFileList = ref<UploadFile[]>([]);

const [Form, formApi] = useVbenForm({
  commonConfig: {
    componentProps: {
      class: 'w-full',
    },
  },
  handleSubmit: onSubmit,
  wrapperClass: 'grid-cols-2',
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
      formItemClass: 'col-span-1',
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
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:ChannelType'),
      rules: 'selectRequired',
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'fileTime',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:FileTime'),
      rules: 'selectRequired',
    },
    {
      component: 'InputNumber',
      componentProps: {
        max: 1000,
        min: 1,
      },
      fieldName: 'fileCount',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:FileCount'),
    },
    {
      component: 'Input',
      fieldName: 'testedDeviceCode',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:TestedDeviceCode'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'testedDeviceName',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:TestedDeviceName'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'pilotSN',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:PilotSN'),
    },
    {
      component: 'Input',
      fieldName: 'shipName',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:ShipName'),
    },
    {
      component: 'Input',
      fieldName: 'testBy',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:TestBy'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'startTime',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:StartTime'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'endTime',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:EndTime'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'startTime2',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:StartTime2'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'endTime2',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:EndTime2'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        showTime: true,
        type: 'datetime',
        valueFormat: 'YYYY-MM-DD HH:mm:ss',
      },
      fieldName: 'testDate',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:TestDate'),
    },
    {
      component: 'Input',
      fieldName: 'group',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:Group'),
    },
    {
      component: 'Input',
      fieldName: 'ifSource',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:IfSource'),
    },
    {
      component: 'Input',
      fieldName: 'source',
      formItemClass: 'col-span-1',
      label: $t('TestWorkshop.Telemetry:Source'),
    },
  ],
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  class: 'w-[980px] max-w-[96vw]',
  onConfirm: async () => {
    await formApi.validateAndSubmitForm();
  },
  onOpenChange: (isOpen) => {
    if (isOpen) {
      onInit();
    }
  },
  title: $t('TestWorkshop.Telemetry:UploadCsv'),
});

function onInit() {
  formApi.resetForm();
  selectedFile.value = undefined;
  selectedFileList.value = [];
  formApi.setValues({
    fileCount: 1,
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
  if (!selectedFile.value) {
    message.warning($t('TestWorkshop.Telemetry:PleaseSelectCsv'));
    return;
  }

  await uploadFile(selectedFile.value, buildInput(values, selectedFile.value));
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

function buildInput(
  values: Record<string, any>,
  file: File,
): WorkshopTelemetryFileInput {
  return {
    channelType: values.channelType,
    currentFile: file.name,
    deviceCode: values.deviceCode,
    fileTime: values.fileTime,
    fileCount: values.fileCount ?? 1,
    group: values.group,
    ifSource: values.ifSource,
    source: values.source,
    testInfo: {
      endTime: values.endTime,
      endTime2: values.endTime2,
      pilotSN: values.pilotSN,
      shipName: values.shipName,
      startTime: values.startTime,
      startTime2: values.startTime2,
      testBy: values.testBy,
      testDate: values.testDate,
      testedDeviceCode: values.testedDeviceCode,
      testedDeviceName: values.testedDeviceName,
    },
  };
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
              selectedFile?.name ?? $t('TestWorkshop.Telemetry:PleaseSelectCsv')
            }}
          </div>
          <div class="text-xs text-gray-500">
            {{
              selectedFile
                ? $t('TestWorkshop.Telemetry:UseSelectedCsv')
                : $t('TestWorkshop.Telemetry:PleaseSelectCsv')
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

      <Form />
    </div>
  </Modal>
</template>

<style scoped></style>
