<script setup lang="ts">
import { ref } from 'vue';

import { Profile } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { useUserStore } from '@vben/stores';

import { VbenAvatar } from '@vben-core/shadcn-ui';

import { CameraOutlined } from '@ant-design/icons-vue';

import { getCurrentUserAvatarApi, USER_AVATAR_OWNER_TYPE } from '#/api';
import { useFileUpload } from '#/components/file-upload';

import ProfileBase from './base-setting.vue';
import ProfilePassword from './password-setting.vue';

const userStore = useUserStore();
const { UploadModal, openFileUpload } = useFileUpload();

const tabsValue = ref<string>('basic');

const tabs = ref([
  {
    label: $t('abp.account.settings.basic.title'),
    value: 'basic',
  },
  {
    label: $t('abp.account.settings.security.password'),
    value: 'password',
  },
]);

function openAvatarUpload() {
  const userId = userStore.userInfo?.userId;
  if (!userId) {
    return;
  }
  openFileUpload({
    accept: 'image/*',
    multiple: false,
    ownerId: userId,
    ownerType: USER_AVATAR_OWNER_TYPE,
    title: $t('abp.account.settings.changeAvatar'),
  });
}

async function refreshAvatar() {
  const current = userStore.userInfo;
  if (!current?.userId) {
    return;
  }
  const avatar = await getCurrentUserAvatarApi();
  if (!avatar) {
    return;
  }
  if (current.avatar?.startsWith('blob:')) {
    URL.revokeObjectURL(current.avatar);
  }
  userStore.setUserInfo({
    ...current,
    avatar: URL.createObjectURL(avatar),
  });
}
</script>
<template>
  <Profile
    v-model:model-value="tabsValue"
    :title="$t('abp.account.profile')"
    :user-info="userStore.userInfo"
    :tabs="tabs"
  >
    <template #avatar="{ src }">
      <div class="flex flex-col items-center gap-2">
        <button
          class="group relative cursor-pointer rounded-full outline-none"
          type="button"
          :title="$t('abp.account.settings.changeAvatar')"
          @click="openAvatarUpload"
        >
          <VbenAvatar :src="src" class="size-20" />
          <span
            class="absolute inset-0 flex items-center justify-center rounded-full bg-black/40 opacity-0 transition-opacity group-hover:opacity-100"
          >
            <CameraOutlined class="text-lg text-white" />
          </span>
        </button>
      </div>
    </template>
    <template #content>
      <ProfileBase v-if="tabsValue === 'basic'" />
      <ProfilePassword v-if="tabsValue === 'password'" />
      <UploadModal @change="refreshAvatar" />
    </template>
  </Profile>
</template>
