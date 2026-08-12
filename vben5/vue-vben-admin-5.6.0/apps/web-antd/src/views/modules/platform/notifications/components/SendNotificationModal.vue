<script lang="ts" setup>
import { useVbenForm, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  NotificationMessageLevel,
  useNotificationsApi,
  useUsersApi,
} from '@abp/core';
import { message } from 'ant-design-vue';

const props = defineProps<{
  mode: 'broadcast' | 'message';
}>();

const emits = defineEmits<{
  (event: 'change'): void;
}>();

const isMessage = props.mode === 'message';
const { sendBroadCastMessageApi, sendCommonMessageApi } = useNotificationsApi();
const { getApi: getUserApi, getPagedListApi } = useUsersApi();

const levelOptions = [
  {
    label: $t('TestWorkshop.NotificationLevel:Warning'),
    value: NotificationMessageLevel.Warning,
  },
  {
    label: $t('TestWorkshop.NotificationLevel:Information'),
    value: NotificationMessageLevel.Information,
  },
  {
    label: $t('TestWorkshop.NotificationLevel:Error'),
    value: NotificationMessageLevel.Error,
  },
];

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
      componentProps: {
        autocomplete: 'off',
        maxlength: 128,
      },
      fieldName: 'title',
      label: $t('TestWorkshop.DisplayName:Subject'),
      rules: 'required',
    },
    {
      component: 'Textarea',
      componentProps: {
        autoSize: {
          maxRows: 8,
          minRows: 4,
        },
        maxlength: 1024,
        showCount: true,
      },
      fieldName: 'content',
      label: $t('TestWorkshop.DisplayName:Content'),
      rules: 'required',
    },
    {
      component: 'Select',
      componentProps: {
        options: levelOptions,
      },
      defaultValue: NotificationMessageLevel.Information,
      fieldName: 'messageLevel',
      label: $t('TestWorkshop.Notification:Level'),
      rules: 'selectRequired',
    },
    ...(isMessage
      ? [
          {
            component: 'ApiSelect',
            componentProps: {
              afterFetch: (result: {
                items: Array<{ id: string; name?: string; userName: string }>;
              }) => {
                return result.items.map((user) => ({
                  label: user.name
                    ? `${user.name} (${user.userName})`
                    : user.userName,
                  userName: user.userName,
                  value: user.id,
                }));
              },
              api: getPagedListApi,
              labelField: 'label',
              onChange: onReceiverChange,
              optionFilterProp: 'label',
              params: {
                maxResultCount: 100,
              },
              showSearch: true,
              valueField: 'value',
            },
            fieldName: 'receiveUserId',
            label: $t('TestWorkshop.Notification:Receiver'),
            rules: 'selectRequired',
          },
          {
            component: 'Input',
            componentProps: {
              style: {
                display: 'none',
              },
            },
            fieldName: 'receiveUserName',
          },
        ]
      : []),
  ],
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  onConfirm: async () => {
    await formApi.validateAndSubmitForm();
  },
  onOpenChange: (isOpen: boolean) => {
    if (!isOpen) {
      return;
    }
    formApi.resetForm();
    formApi.setFieldValue('messageLevel', NotificationMessageLevel.Information);
    modalApi.setState({
      title: isMessage
        ? $t('TestWorkshop.Notification:SendMessage')
        : $t('TestWorkshop.Notification:SendBroadcast'),
    });
  },
});

function onReceiverChange(_value: string, option: any) {
  formApi.setFieldValue('receiveUserName', option?.userName ?? '');
}

async function onSubmit(values: Record<string, any>) {
  try {
    modalApi.setState({ submitting: true });
    const commonInput = {
      content: values.content,
      messageLevel: values.messageLevel,
      title: values.title,
    };
    if (isMessage) {
      const user = await getUserApi(values.receiveUserId);
      await sendCommonMessageApi({
        ...commonInput,
        receiveUserId: values.receiveUserId,
        receiveUserName: user.userName,
      });
    } else {
      await sendBroadCastMessageApi(commonInput);
    }
    message.success($t('TestWorkshop.Notification:SendSuccess'));
    emits('change');
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
