<script lang="ts" setup>
import type { NameValue, TenantConnectionStringDto } from '@abp/core';
import type { FormExpose } from 'ant-design-vue/es/form/Form';

import { ref, toValue, useTemplateRef } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { useTenantsApi } from '@abp/core';
import { Form, Input, Select, Textarea } from 'ant-design-vue';

const props = defineProps<{
  dataBaseOptions: { label: string; value: string }[];
  submit?: (data: TenantConnectionStringDto) => Promise<void>;
}>();

const FormItem = Form.Item;

interface TenantConnectionString extends NameValue<string> {
  provider: string;
}

const isEditModal = ref(false);
const form = useTemplateRef<FormExpose>('form');
const formModel = ref<TenantConnectionString>({
  name: '',
  provider: 'MySql',
  value: '',
});

const { checkConnectionString } = useTenantsApi();

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    await form.value?.validate();
    onSubmit();
  },
  onOpenChange(isOpen) {
    isEditModal.value = false;
    let title = $t('AbpSaas.ConnectionStrings');
    if (isOpen) {
      form.value?.resetFields();
      const dto = modalApi.getData<TenantConnectionStringDto>();
      formModel.value = {
        provider: formModel.value.provider,
        ...dto,
      };
      if (dto.name) {
        isEditModal.value = true;
        title = `${$t('AbpSaas.ConnectionStrings')} - ${dto.name}`;
      }
    }
    modalApi.setState({ title });
  },
  title: $t('AbpSaas.ConnectionStrings'),
});
async function onSubmit() {
  modalApi.setState({ submitting: true });
  try {
    const input = toValue(formModel);
    await checkConnectionString({
      connectionString: input.value,
      name: input.name,
      provider: input.provider,
    });
    props.submit && (await props.submit(input));
    modalApi.close();
  } finally {
    modalApi.setState({ submitting: false });
  }
}
</script>
<template>
  <Modal>
    <Form
      ref="form"
      :label-col="{ span: 4 }"
      :wapper-col="{ span: 20 }"
      :model="formModel"
    >
      <FormItem
        name="provider"
        :label="$t('AbpSaas.DisplayName:DataBaseProvider')"
        required
      >
        <Select :options="dataBaseOptions" v-model:value="formModel.provider" />
      </FormItem>
      <FormItem name="name" :label="$t('AbpSaas.DisplayName:Name')" required>
        <Input
          :disabled="isEditModal"
          autocomplete="off"
          v-model:value="formModel.name"
        />
      </FormItem>
      <FormItem name="value" :label="$t('AbpSaas.DisplayName:Value')" required>
        <Textarea :auto-size="{ minRows: 3 }" v-model:value="formModel.value" />
      </FormItem>
    </Form>
  </Modal>
</template>
<style scoped></style>
