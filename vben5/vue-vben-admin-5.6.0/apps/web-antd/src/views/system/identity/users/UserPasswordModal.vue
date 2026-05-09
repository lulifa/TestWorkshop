<script lang="ts" setup>
import type { IdentityUserDto } from '@abp/core';

import { h } from 'vue';

import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useRandomPassword, useUsersApi } from '@abp/core';
import { Button, message } from 'ant-design-vue';

defineOptions({
  name: 'UserPasswordModal',
});
const emits = defineEmits<{
  (event: 'change'): void;
}>();

const { generatePassword } = useRandomPassword();
const { cancel, changePasswordApi } = useUsersApi();

const [Form, formApi] = useVbenForm({
  commonConfig: {
    // 所有表单项
    componentProps: {
      class: 'w-full',
    },
  },
  handleSubmit: onSubmit,
  schema: [
    {
      component: 'InputSearch',
      componentProps: (_, actions) => {
        return {
          allowClear: false,
          enterButton: h(
            Button,
            {
              type: 'primary',
            },
            () => $t('AbpIdentity.RandomPassword'),
          ),
          onSearch: () => {
            actions.resetForm();
            actions.setFieldValue('password', generatePassword());
          },
        };
      },
      fieldName: 'password',
      label: $t('AbpIdentity.Password'),
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
    cancel('User password modal has closed!');
  },
  onConfirm: async () => {
    try {
      modalApi.setState({ submitting: true });
      await formApi.validateAndSubmitForm();
    } finally {
      modalApi.setState({ submitting: false });
    }
  },
  onOpenChange(isOpen) {
    let title = $t('AbpIdentity.SetPassword');
    if (isOpen) {
      const { userName } = modalApi.getData<IdentityUserDto>();
      title += ` - ${userName}`;
    }
    modalApi.setState({ title });
  },
  title: $t('AbpIdentity.SetPassword'),
});

async function onSubmit(input: Record<string, any>) {
  const { id } = modalApi.getData<IdentityUserDto>();
  await changePasswordApi(id, { password: input.password });
  message.success($t('AbpUi.SavedSuccessfully'));
  emits('change');
  modalApi.close();
}
</script>
<template>
  <Modal>
    <Form />
  </Modal>
</template>

<style scoped></style>
