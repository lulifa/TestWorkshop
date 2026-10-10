<script lang="ts" setup>
import type {
  WorkshopDeviceDto,
  WorkshopOrderCreateDto,
  WorkshopOrderDto,
  WorkshopOrderUpdateDto,
} from '@abp/core';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useWorkshopDeviceApi, useWorkshopOrderApi } from '@abp/core';
import { message } from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change', data: WorkshopOrderDto): void;
}>();

const { createApi, getApi, updateApi } = useWorkshopOrderApi();
const { getListApi: getDeviceListApi } = useWorkshopDeviceApi();

const [Form, formApi] = useVbenForm({
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
        triggerFields: ['id'],
      },
      fieldName: 'id',
    },
    {
      component: 'Input',
      componentProps: {
        maxlength: 64,
      },
      fieldName: 'orderNo',
      label: $t('TestWorkshop.WorkshopOrder:OrderNo'),
      rules: 'required',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        afterFetch: (result: { items: WorkshopDeviceDto[] }) => {
          return result.items.map((item) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
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
      fieldName: 'deviceId',
      label: $t('TestWorkshop.WorkshopOrder:Device'),
      rules: 'selectRequired',
    },
  ],
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
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

    const state = modalApi.getData<WorkshopOrderDto>();
    if (state?.id) {
      const dto = await getApi(state.id);
      formApi.setValues(dto);
      modalApi.setState({
        title: `${$t('TestWorkshop.WorkshopOrder:Edit')} - ${dto.orderNo}`,
      });
      return;
    }

    modalApi.setState({
      title: $t('TestWorkshop.WorkshopOrder:AddNew'),
    });
  } finally {
    modalApi.setState({ loading: false });
  }
}

async function onSubmit(values: Record<string, any>) {
  try {
    modalApi.setState({ submitting: true });

    const dto = values.id
      ? await updateApi(values.id, values as WorkshopOrderUpdateDto)
      : await createApi(values as WorkshopOrderCreateDto);

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
