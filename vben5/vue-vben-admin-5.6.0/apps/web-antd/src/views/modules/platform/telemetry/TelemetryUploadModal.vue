<script lang="ts" setup>
import type { WorkshopDeviceDto } from '@abp/core';

import { ref } from 'vue';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  TelemetryMetricType,
  useWorkshopDeviceApi,
  useWorkshopTelemetryApi,
} from '@abp/core';
import { message } from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change'): void;
}>();

const { getListApi: getDeviceListApi } = useWorkshopDeviceApi();
const { uploadApi } = useWorkshopTelemetryApi();

const generatedFileName = ref('');

const metricTypeOptions = [
  {
    label: $t('TestWorkshop.Telemetry:MetricPressure'),
    value: TelemetryMetricType.Pressure,
  },
  {
    label: $t('TestWorkshop.Telemetry:MetricTemp'),
    value: TelemetryMetricType.Temp,
  },
  {
    label: $t('TestWorkshop.Telemetry:MetricFlow'),
    value: TelemetryMetricType.Flow,
  },
  {
    label: $t('TestWorkshop.Telemetry:MetricVibration'),
    value: TelemetryMetricType.Vibration,
  },
];

const metricTypeNameMap: Record<TelemetryMetricType, string> = {
  [TelemetryMetricType.Pressure]: 'Pressure',
  [TelemetryMetricType.Temp]: 'Temp',
  [TelemetryMetricType.Flow]: 'Flow',
  [TelemetryMetricType.Vibration]: 'Vibration',
};

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
        options: metricTypeOptions,
      },
      fieldName: 'metricType',
      label: $t('TestWorkshop.Telemetry:MetricType'),
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
      component: 'InputNumber',
      componentProps: {
        max: 10_000,
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
  generatedFileName.value = buildGeneratedFileName();
  formApi.setValues({
    metricType: TelemetryMetricType.Pressure,
    recordCount: 1,
    testedDeviceCode: 'DUT-A1',
    testedDeviceName: '水泵A',
  });
}

async function onSubmit(values: Record<string, any>) {
  await uploadFile(buildCsvFile(values));
}

async function uploadFile(file: File) {
  try {
    modalApi.setState({ submitting: true });
    await uploadApi(file);
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

function buildCsvFile(values: Record<string, any>): File {
  const lines = [
    'DeviceCode,MetricType,Value,Timestamp,TestedDeviceCode,TestedDeviceName',
  ];
  const metricType = values.metricType as TelemetryMetricType;
  const metricTypeName = metricTypeNameMap[metricType];
  const recordCount = Number(values.recordCount);
  const now = Date.now();

  for (let index = 0; index < recordCount; index += 1) {
    const timestamp = new Date(
      now - (recordCount - index - 1) * 1000,
    ).toISOString();
    const value = generateMetricValue(metricType);
    lines.push(
      [
        values.deviceCode,
        metricTypeName,
        value.toFixed(2),
        timestamp,
        values.testedDeviceCode,
        values.testedDeviceName,
      ].join(','),
    );
  }

  const blob = new Blob([lines.join('\n')], {
    type: 'text/csv',
  });
  return new File([blob], generatedFileName.value, {
    type: 'text/csv',
  });
}

function generateMetricValue(metricType: TelemetryMetricType) {
  switch (metricType) {
    case TelemetryMetricType.Flow: {
      return 10 + Math.random() * 20;
    }
    case TelemetryMetricType.Pressure: {
      return 0.5 + Math.random() * 1.5;
    }
    case TelemetryMetricType.Temp: {
      return 40 + Math.random() * 40;
    }
    case TelemetryMetricType.Vibration: {
      return 0.1 + Math.random() * 5;
    }
    default: {
      return 50;
    }
  }
}
</script>

<template>
  <Modal>
    <div class="flex flex-col gap-4">
      <div class="text-xs text-gray-500">
        {{ $t('TestWorkshop.Telemetry:GenerateMode') }}
      </div>

      <div class="text-xs text-gray-500">
        {{ $t('TestWorkshop.Telemetry:GeneratedFileName') }}:
        {{ generatedFileName }}
      </div>

      <Form />
    </div>
  </Modal>
</template>

<style scoped></style>
