<script lang="ts" setup>
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';

import { useAuthStore } from '#/store/auth';

const router = useRouter();

const authStore = useAuthStore();

onMounted(async () => {
  try {
    await authStore.oidcCallback();
  } catch {
    setTimeout(async () => {
      await router.replace({ path: LOGIN_PATH });
    }, 2000);
  }
});
</script>

<template>
  <div>{{ $t('page.auth.processingLogin') }}</div>
</template>
