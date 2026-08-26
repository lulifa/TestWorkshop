<script lang="ts" setup>
import type {
  OrganizationUnitDto,
  WorkshopDeviceCreateDto,
  WorkshopDeviceDto,
  WorkshopDeviceTypeDto,
  WorkshopDeviceUpdateDto,
} from '@abp/core';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { DeviceType, listToTree, useWorkshopDeviceApi } from '@abp/core';
import { message } from 'ant-design-vue';

const emits = defineEmits<{
  (event: 'change', data: WorkshopDeviceDto): void;
}>();

const { createApi, getApi, getOrganizationUnitsApi, getTypesApi, updateApi } =
  useWorkshopDeviceApi();

function toOrganizationTree(items: OrganizationUnitDto[]) {
  const tree = listToTree(items, {
    id: 'id',
    pid: 'parentId',
  });
  return tree.map((node) => toTreeNode(node));
}

function toTreeNode(node: any): any {
  const children = (node.children ?? []).map((child: any) => toTreeNode(child));
  return {
    children,
    disabled: children.length > 0,
    label: node.displayName,
    value: node.id,
  };
}

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
          return result.items.map((item) => ({
            label: item.displayName || item.name,
            value: item.value,
          }));
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
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: false,
        api: async () => {
          const { items } = await getOrganizationUnitsApi();
          return toOrganizationTree(items);
        },
        childrenField: 'children',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'organizationUnitId',
      label: $t('TestWorkshop.DisplayName:OrganizationUnit'),
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
