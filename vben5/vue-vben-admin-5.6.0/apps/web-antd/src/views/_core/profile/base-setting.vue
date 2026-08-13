<script setup lang="ts">
import type { VbenFormSchema } from '#/adapter/form';

import { computed, onMounted, ref } from 'vue';

import { ProfileBaseSetting } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { preferences } from '@vben/preferences';
import { useUserStore } from '@vben/stores';

import { useSettings, useUsersApi } from '@abp/core';
import { message } from 'ant-design-vue';

const profileBaseSettingRef = ref<undefined | { getFormApi: () => any }>();
const { getCurrentUserProfileApi, updateCurrentUserProfileApi } = useUsersApi();
const { isTrue } = useSettings();
const userStore = useUserStore();

const formSchema = computed((): VbenFormSchema[] => {
  return [
    {
      component: 'Input',
      componentProps: {
        disabled: !isTrue('Abp.Identity.User.IsUserNameUpdateEnabled'),
      },
      fieldName: 'userName',
      label: $t('AbpIdentity.UserName'),
      rules: 'required',
    },
    {
      component: 'Input',
      componentProps: {
        disabled: !isTrue('Abp.Identity.User.IsEmailUpdateEnabled'),
      },
      fieldName: 'email',
      label: $t('AbpIdentity.DisplayName:Email'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'surname',
      label: $t('AbpIdentity.DisplayName:Surname'),
    },
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('AbpIdentity.DisplayName:Name'),
    },
  ];
});

onMounted(loadProfile);

async function loadProfile() {
  const data = await getCurrentUserProfileApi();
  profileBaseSettingRef.value?.getFormApi().setValues({
    email: data.email,
    name: data.name,
    surname: data.surname,
    userName: data.userName,
  });
}

async function handleSubmit(values: Record<string, any>) {
  const updated = await updateCurrentUserProfileApi({
    email: values.email,
    name: values.name,
    surname: values.surname,
  });
  const userInfo = userStore.userInfo;
  userStore.setUserInfo({
    ...userInfo,
    avatar: userInfo?.avatar ?? preferences.app.defaultAvatar,
    email: updated.email,
    realName:
      [updated.surname, updated.name].filter(Boolean).join(' ') ||
      updated.userName,
    roles: updated.roleNames,
    userId: updated.id,
    username: updated.userName,
  });
  message.success($t('AbpUi.SavedSuccessfully'));
}
</script>
<template>
  <ProfileBaseSetting
    ref="profileBaseSettingRef"
    :form-schema="formSchema"
    @submit="handleSubmit"
  />
</template>
