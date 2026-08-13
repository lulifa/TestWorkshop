<script setup lang="ts">
import type { VbenFormSchema } from '#/adapter/form';

import { computed } from 'vue';

import { ProfilePasswordSetting, z } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useUsersApi } from '@abp/core';
import { message } from 'ant-design-vue';

const { changeCurrentUserPasswordApi } = useUsersApi();

const formSchema = computed((): VbenFormSchema[] => {
  return [
    {
      component: 'VbenInputPassword',
      componentProps: {
        autocomplete: 'current-password',
      },
      fieldName: 'currentPassword',
      label: $t('abp.account.settings.security.currentPassword'),
      rules: 'required',
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        autocomplete: 'new-password',
        passwordStrength: true,
      },
      fieldName: 'newPassword',
      label: $t('abp.account.settings.security.newPassword'),
      dependencies: {
        rules: () =>
          z
            .string({
              required_error: $t('abp.account.settings.security.newPassword'),
            })
            .min(6, {
              message: $t('abp.account.settings.security.passwordMinLength'),
            }),
        triggerFields: ['newPassword'],
      },
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        autocomplete: 'new-password',
        passwordStrength: true,
      },
      fieldName: 'confirmPassword',
      label: $t('abp.account.settings.security.confirmPassword'),
      rules: 'required',
    },
  ];
});

async function handleSubmit(values: Record<string, any>) {
  if (values.newPassword !== values.confirmPassword) {
    message.warning($t('abp.account.settings.security.passwordMismatch'));
    return;
  }
  await changeCurrentUserPasswordApi({
    currentPassword: values.currentPassword,
    newPassword: values.newPassword,
  });
  message.success($t('AbpUi.SavedSuccessfully'));
}
</script>
<template>
  <ProfilePasswordSetting :form-schema="formSchema" @submit="handleSubmit" />
</template>
