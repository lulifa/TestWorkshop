<script lang="ts" setup>
import type {
  TenantConnectionStringDto,
  TenantCreateDto,
  TenantDto,
  TenantUpdateDto,
} from '@abp/core';
import type { FormExpose } from 'ant-design-vue/es/form/Form';

import { ref, useTemplateRef } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useTenantsApi } from '@abp/core';
import {
  Checkbox,
  Form,
  Input,
  InputPassword,
  message,
  Select,
  Textarea,
} from 'ant-design-vue';

import ConnectionStringTable from './ConnectionStringTable.vue';

defineProps<{
  dataBaseOptions: { label: string; value: string }[];
}>();
const emits = defineEmits<{
  (event: 'change', val: TenantDto): void;
}>();
const FormItem = Form.Item;

const defaultModel = {
  connectionStrings: [],
  useSharedDatabase: true,
} as unknown as TenantDto;

const form = useTemplateRef<FormExpose>('form');
const tenant = ref({ ...defaultModel });
const activeTabKey = ref('basic');

const { cancel, checkConnectionString, createApi, getApi, updateApi } =
  useTenantsApi();

const [Modal, modalApi] = useVbenModal({
  class: 'w-[600px]',
  onClosed: cancel,
  async onConfirm() {
    await form.value?.validate();
    await onSubmit();
  },
  async onOpenChange(isOpen) {
    activeTabKey.value = 'basic';
    if (isOpen) {
      await onGet();
    }
  },
  title: $t('AbpSaas.Tenants'),
});

async function onGet() {
  const { id } = modalApi.getData<TenantDto>();
  if (!id) {
    tenant.value = { ...defaultModel };
    modalApi.setState({ title: $t('AbpSaas.NewTenant') });
    return;
  }
  try {
    modalApi.setState({ loading: true });
    const editionDto = await getApi(id);
    modalApi.setState({
      title: `${$t('AbpSaas.Tenants')} - ${editionDto.name}`,
    });
    tenant.value = editionDto;
  } finally {
    modalApi.setState({ loading: false });
  }
}

async function onSubmit() {
  try {
    modalApi.setState({ submitting: true });
    if (!tenant.value.id && !tenant.value.useSharedDatabase) {
      await checkConnectionString({
        connectionString: tenant.value.defaultConnectionString,
        provider: tenant.value.provider,
      });
    }
    const api = tenant.value.id
      ? updateApi(tenant.value.id, tenant.value as TenantUpdateDto)
      : createApi(tenant.value as unknown as TenantCreateDto);
    const dto = await api;
    message.success($t('AbpUi.SavedSuccessfully'));
    emits('change', dto);
    modalApi.close();
  } finally {
    modalApi.setState({ submitting: false });
  }
}

function onNameChange(name?: string) {
  if (
    !tenant.value.id &&
    (!tenant.value.adminEmailAddress ||
      !tenant.value.adminEmailAddress?.endsWith(`@${name}.com`))
  ) {
    tenant.value.adminEmailAddress = `admin@${name}.com`;
  }
  form.value?.clearValidate('adminEmailAddress');
}

function onConnectionChange(data: TenantConnectionStringDto) {
  return new Promise<void>((resolve) => {
    tenant.value.connectionStrings ??= [];
    let connectionString = tenant.value.connectionStrings.find(
      (x: TenantConnectionStringDto) => x.name === data.name,
    );
    if (connectionString) {
      connectionString.value = data.value;
    } else {
      connectionString = data;
      tenant.value.connectionStrings = [
        ...tenant.value.connectionStrings,
        data,
      ];
    }
    resolve();
  });
}

function onConnectionDelete(data: TenantConnectionStringDto) {
  return new Promise<void>((resolve) => {
    tenant.value.connectionStrings ??= [];
    tenant.value.connectionStrings = tenant.value.connectionStrings.filter(
      (x: TenantConnectionStringDto) => x.name !== data.name,
    );
    resolve();
  });
}
</script>
<template>
  <Modal>
    <Form
      ref="form"
      :model="tenant"
      :label-col="{ span: 6 }"
      :wrapper-col="{ span: 18 }"
    >
      <FormItem
        name="name"
        :label="$t('AbpSaas.DisplayName:TenantName')"
        required
      >
        <Input
          v-model:value="tenant.name"
          @change="(e) => onNameChange(e.target.value)"
          autocomplete="off"
        />
      </FormItem>
      <FormItem
        v-if="!tenant.id"
        name="adminEmailAddress"
        :label="$t('AbpSaas.DisplayName:AdminEmailAddress')"
        required
      >
        <Input
          type="email"
          v-model:value="tenant.adminEmailAddress"
          autocomplete="off"
        />
      </FormItem>
      <FormItem
        v-if="!tenant.id"
        name="adminPassword"
        :label="$t('AbpSaas.DisplayName:AdminPassword')"
        required
      >
        <InputPassword
          v-model:value="tenant.adminPassword"
          autocomplete="off"
        />
      </FormItem>
      <FormItem
        v-if="!tenant.id"
        name="useSharedDatabase"
        :label="$t('AbpSaas.DisplayName:UseSharedDatabase')"
      >
        <Checkbox v-model:checked="tenant.useSharedDatabase">
          {{ $t('AbpSaas.DisplayName:UseSharedDatabase') }}
        </Checkbox>
      </FormItem>
      <template v-if="!tenant.id && !tenant.useSharedDatabase">
        <FormItem
          name="provider"
          :label="$t('AbpSaas.DisplayName:DataBaseProvider')"
          required
        >
          <Select :options="dataBaseOptions" v-model:value="tenant.provider" />
        </FormItem>
        <FormItem
          name="defaultConnectionString"
          :label="$t('AbpSaas.DisplayName:DefaultConnectionString')"
          required
        >
          <Textarea
            :auto-size="{ minRows: 2 }"
            v-model:value="tenant.defaultConnectionString"
          />
        </FormItem>
        <ConnectionStringTable
          :data-base-options="dataBaseOptions"
          :connection-strings="tenant.connectionStrings"
          :submit="onConnectionChange"
          :delete="onConnectionDelete"
        />
      </template>
    </Form>
  </Modal>
</template>
<style scoped></style>
