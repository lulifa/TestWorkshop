<script setup lang="ts">
import type {
  OrganizationUnitCreateDto,
  OrganizationUnitDto,
  OrganizationUnitUpdateDto,
} from '@abp/core';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useOrganizationUnitsApi, useWorkshopDeviceApi } from '@abp/core';
import { message } from 'ant-design-vue';

interface OrganizationUnitChangeResult {
  id: string;
  parentId?: string;
}

defineOptions({
  name: 'OrganizationUnitModal',
});
const emits = defineEmits<{
  (event: 'change', data: OrganizationUnitChangeResult): void;
}>();

const defaultModel = {
  displayName: '',
  extraProperties: {},
} as OrganizationUnitDto;

const { cancel, createApi, getApi, updateApi } = useOrganizationUnitsApi();
const { initializeWorkshopApi } = useWorkshopDeviceApi();
const [Form, formApi] = useVbenForm({
  handleSubmit: onSubmit,
  schema: [
    {
      component: 'Input',
      componentProps: {
        style: {
          display: 'none',
        },
      },
      fieldName: 'id',
    },
    {
      component: 'Input',
      componentProps: {
        style: {
          display: 'none',
        },
      },
      fieldName: 'parentId',
    },
    {
      component: 'Input',
      componentProps: {
        autocomplete: 'off',
      },
      fieldName: 'displayName',
      label: $t('AbpIdentity.OrganizationUnit:DisplayName'),
      rules: 'required',
    },
    {
      component: 'Input',
      componentProps: {
        autocomplete: 'off',
      },
      dependencies: {
        required: (values) =>
          !values?.id && Boolean(values?.initializeWorkshopPersonnel),
        triggerFields: ['id', 'initializeWorkshopPersonnel'],
      },
      fieldName: 'extraProperties.BusinessCode',
      label: $t('AbpIdentity.OrganizationUnit:BusinessCode'),
    },
    {
      component: 'Checkbox',
      dependencies: {
        if: (values) => !values?.id,
        triggerFields: ['id'],
      },
      fieldName: 'initializeWorkshopPersonnel',
      label: $t('page.business.initializeWorkshopPersonnel'),
    },
    {
      component: 'InputPassword',
      componentProps: {
        autocomplete: 'new-password',
      },
      dependencies: {
        if: (values) => !values?.id && values?.initializeWorkshopPersonnel,
        triggerFields: ['id', 'initializeWorkshopPersonnel'],
      },
      fieldName: 'initialPassword',
      label: $t('page.business.initialPassword'),
      rules: 'required',
    },
  ],
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  closeOnClickModal: false,
  closeOnPressEscape: false,
  draggable: true,
  fullscreenButton: false,
  onCancel() {
    modalApi.close();
  },
  onClosed() {
    cancel('Organization Unit Modal has closed!');
  },
  onConfirm: async () => {
    await formApi.validateAndSubmitForm();
  },
  async onOpenChange(isOpen: boolean) {
    if (isOpen) {
      await formApi.resetForm();
      const { id, parentId } = modalApi.getData<Record<string, any>>();
      if (id) {
        modalApi.setState({ loading: true });
        try {
          const dto = await getApi(id);
          modalApi.setState({
            title: $t('AbpIdentity.OrganizationUnits', [dto.displayName]),
          });
          formApi.setValues(dto);
        } finally {
          modalApi.setState({ loading: false });
        }
        return;
      }
      formApi.setValues({
        ...defaultModel,
        initialPassword: '1q2w3E*',
        initializeWorkshopPersonnel: Boolean(parentId),
        parentId,
      });
      modalApi.setState({
        title: $t('AbpIdentity.OrganizationUnit:New'),
      });
    }
  },
  title: $t('AbpIdentity.OrganizationUnit:New'),
});

async function onSubmit(input: Record<string, any>) {
  const isUpdate = Boolean(input.id);
  const initializePersonnel =
    !isUpdate && Boolean(input.initializeWorkshopPersonnel);
  const businessCode = String(input.extraProperties?.BusinessCode ?? '').trim();
  if (initializePersonnel && !businessCode) {
    message.warning($t('page.business.businessCodeRequired'));
    return;
  }

  try {
    modalApi.setState({
      submitting: true,
    });

    let result: OrganizationUnitChangeResult;
    if (isUpdate) {
      const dto = await updateApi(input.id, {
        displayName: input.displayName,
        extraProperties: input.extraProperties,
      } as OrganizationUnitUpdateDto);
      result = { id: dto.id, parentId: dto.parentId };
    } else if (initializePersonnel) {
      const workshop = await initializeWorkshopApi({
        businessCode,
        displayName: input.displayName,
        initialPassword: input.initialPassword,
        parentOrganizationUnitId: input.parentId,
      });
      result = {
        id: workshop.organizationUnitId,
        parentId: workshop.parentOrganizationUnitId ?? input.parentId,
      };
    } else {
      const dto = await createApi({
        displayName: input.displayName,
        extraProperties: input.extraProperties,
        parentId: input.parentId,
      } as OrganizationUnitCreateDto);
      result = { id: dto.id, parentId: dto.parentId };
    }

    emits('change', result);
    modalApi.close();
  } finally {
    modalApi.setState({
      submitting: false,
    });
  }
}
</script>

<template>
  <Modal>
    <Form />
  </Modal>
</template>

<style scoped></style>
