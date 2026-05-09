<script lang="ts" setup>
import type { ExtendedFormApi, VbenFormSchema } from '@vben/common-ui';
import type { Recordable } from '@vben/types';

import { computed, markRaw, nextTick, onMounted, useTemplateRef } from 'vue';

import { AuthenticationLogin, z } from '@vben/common-ui';
import { useAppConfig } from '@vben/hooks';
import { $t } from '@vben/locales';

import {
  useAbpStore,
  useApplicationConfigurationApi,
  useSettings,
} from '@abp/core';
import { Modal } from 'ant-design-vue';

import { useAuthStore } from '#/store';
import TenantSelect from '#/views/system/tenants/TenantSelect.vue';

import ThirdPartyLogin from './third-party-login.vue';

interface LoginInstance {
  getFormApi(): ExtendedFormApi | undefined;
}

defineOptions({ name: 'Login' });

const { onlyOidc } = useAppConfig(import.meta.env, import.meta.env.PROD);

const abpStore = useAbpStore();

const authStore = useAuthStore();

const { isTrue } = useSettings();

const { getConfigApi } = useApplicationConfigurationApi();

const login = useTemplateRef<LoginInstance>('login');

const formSchema = computed((): VbenFormSchema[] => {
  if (onlyOidc) {
    return [];
  }
  let schemas: VbenFormSchema[] = [
    {
      component: 'Input',
      componentProps: {
        placeholder: $t('authentication.usernameTip'),
      },
      fieldName: 'username',
      label: $t('authentication.username'),
      rules: z.string().min(1, { message: $t('authentication.usernameTip') }),
    },
    {
      component: 'InputPassword',
      componentProps: {
        placeholder: $t('authentication.password'),
        autocomplete: 'off',
      },
      fieldName: 'password',
      label: $t('authentication.password'),
      rules: z.string().min(1, { message: $t('authentication.passwordTip') }),
    },
  ];
  if (abpStore.application?.multiTenancy?.isEnabled) {
    schemas = [
      {
        component: markRaw(TenantSelect),
        componentProps: {
          onChange: onInit,
        },
        fieldName: 'tenant',
      },
      ...schemas,
    ];
  }
  return schemas;
});

async function onInit() {
  if (onlyOidc === true) {
    setTimeout(() => {
      Modal.confirm({
        centered: true,
        title: $t('page.auth.oidcLogin'),
        content: $t('page.auth.oidcLoginMessage'),
        maskClosable: false,
        closable: false,
        cancelButtonProps: {
          disabled: true,
        },
        async onOk() {
          await authStore.oidcLogin();
        },
      });
    }, 300);
    return;
  }
  const abpConfig = await getConfigApi();
  abpStore.setApplication(abpConfig);
  nextTick(() => {
    const formApi = login.value?.getFormApi();
    formApi?.setFieldValue('tenant', abpConfig.currentTenant.name);
  });
}
async function onLogin(params: Recordable<any>) {
  if (onlyOidc === true) {
    await authStore.oidcLogin();
    return;
  }

  try {
    await authStore.authLogin(params);
  } catch {}
}

onMounted(onInit);
</script>

<template>
  <div v-if="!onlyOidc">
    <AuthenticationLogin
      ref="login"
      :form-schema="formSchema"
      :loading="authStore.loginLoading"
      :show-register="isTrue('Abp.Account.IsSelfRegistrationEnabled')"
      @submit="onLogin"
    >
      <!-- 第三方登录 -->
      <template #third-party-login>
        <ThirdPartyLogin />
      </template>
    </AuthenticationLogin>
  </div>
</template>
