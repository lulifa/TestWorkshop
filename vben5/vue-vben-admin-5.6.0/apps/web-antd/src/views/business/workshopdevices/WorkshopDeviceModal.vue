<script lang="ts" setup>
import type {
  WorkshopDeviceCreateDto,
  WorkshopDeviceDto,
  WorkshopDeviceTypeDto,
  WorkshopDeviceUpdateDto,
} from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { DeviceType, useWorkshopDeviceApi } from '@abp/core';
import { message } from 'ant-design-vue';

import { WorkshopSelect } from '#/components/workshop-select';

import { toWorkshopDeviceTypeOptions } from './device-type-options';

const emits = defineEmits<{
  (event: 'change', data: WorkshopDeviceDto): void;
}>();

const { createApi, getApi, getTypesApi, updateApi } = useWorkshopDeviceApi();

const formOptions: VbenFormProps = {
  commonConfig: {
    componentProps: {
      class: 'w-full',
    },
  },
  handleSubmit: onSubmit,
  schema: [
    {
      component: 'Input',
      dependencies: {
        show: false,
        triggerFields: ['code'],
      },
      fieldName: 'id',
    },
    {
      component: 'Input',
      dependencies: {
        disabled: (values) => {
          return !!values?.id;
        },
        triggerFields: ['id'],
      },
      fieldName: 'code',
      label: $t('TestWorkshop.DisplayName:Code'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('TestWorkshop.DisplayName:Name'),
      rules: 'required',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        afterFetch: (result: { items: WorkshopDeviceTypeDto[] }) => {
          return toWorkshopDeviceTypeOptions(result.items);
        },
        api: getTypesApi,
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'type',
      label: $t('TestWorkshop.DisplayName:Type'),
      rules: 'selectRequired',
    },
    {
      component: WorkshopSelect,
      componentProps: {
        allowClear: false,
        mode: 'tree',
        placeholder: '请选择车间',
      },
      fieldName: 'organizationUnitId',
      label: $t('TestWorkshop.DisplayName:OrganizationUnit'),
      modelPropName: 'modelValue',
      rules: 'selectRequired',
    },
  ],
  showDefaultActions: false,
};

const [Form, formApi] = useVbenForm(formOptions);

const [Modal, modalApi] = useVbenModal({
  // 表单弹窗不允许点遮罩关闭，避免误点丢失已填内容
  closeOnClickModal: false,
  closeOnPressEscape: false,
  onConfirm: async () => {
    await formApi.validateAndSubmitForm();
  },
  onOpenChange: async (isOpen) => {
    if (isOpen) {
      await onInit();
    }
  },
});

async function onInit() {
  try {
    modalApi.setState({ loading: true });
    formApi.resetForm();
    const state = modalApi.getData<WorkshopDeviceDto>();
    let title = $t('TestWorkshop.WorkshopDevice:AddNew');
    if (state?.id) {
      const dto = await getApi(state.id);
      formApi.setValues(dto);
      title = `${$t('TestWorkshop.WorkshopDevice:Edit')} - ${dto.code}`;
    } else {
      formApi.setValues({
        type: DeviceType.FIVA,
      });
    }
    modalApi.setState({ title });
  } finally {
    modalApi.setState({ loading: false });
  }
}

async function onSubmit(values: Record<string, any>) {
  try {
    modalApi.setState({ submitting: true });
    const api = values.id
      ? updateApi(values.id, values as WorkshopDeviceUpdateDto)
      : createApi(values as WorkshopDeviceCreateDto);
    const dto = await api;
    message.success($t('AbpUi.SavedSuccessfully'));
    emits('change', dto);
    modalApi.close();
  } finally {
    modalApi.setState({ submitting: false });
  }
}
</script>

<template>
  <Modal>
    <Form />
  </Modal>
</template>

<style scoped></style>
