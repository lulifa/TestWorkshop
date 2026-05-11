<script setup lang="ts">
import { useFeatures } from '@abp/core';

import { useAuthStore } from '#/store/auth';

defineOptions({
  name: 'ThirdPartyLogin',
});

const authStore = useAuthStore();

const { isEnabled } = useFeatures();

async function login() {
  await authStore.oidcLogin();
}
async function loginGitHub() {
  await authStore.oidcLogin('GitHub');
}
</script>

<template>
  <div class="w-full sm:mx-auto md:max-w-md">
    <!-- 注释掉 后期如果对接三方登录 可以放开 -->
    <!-- <div class="mt-4 flex items-center justify-between">
      <span class="w-[35%] border-b border-input dark:border-gray-600"></span>
      <span class="text-center text-xs uppercase text-muted-foreground">
        {{ $t('authentication.thirdPartyLogin') }}
      </span>
      <span class="w-[35%] border-b border-input dark:border-gray-600"></span>
    </div>

    <div class="mt-4 flex flex-wrap justify-center">
      <VbenIconButton
        :tooltip="$t('authentication.githubLogin')"
        tooltip-side="top"
        class="mb-3"
        @click="loginGitHub"
        v-if="isEnabled('Abp.Account.OAuth.GitHub.Enable')"
      >
        <SvgGithubIcon />
      </VbenIconButton>
    </div>
    <div class="mt-4 flex flex-wrap justify-center">
      <Button block type="primary" ghost @click="login">
        {{ $t('page.auth.oidcLogin') }}
      </Button>
    </div> -->
  </div>
</template>
