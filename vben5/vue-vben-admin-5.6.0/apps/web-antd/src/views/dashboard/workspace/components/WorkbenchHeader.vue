<script lang="ts" setup>
import { useRouter } from 'vue-router';

import { VbenAvatar } from '@vben-core/shadcn-ui';

interface Props {
  avatar?: string;
  broadcastCount?: number;
  notifierCount?: number;
  text?: string;
}

defineOptions({
  name: 'WorkbenchHeader',
});

withDefaults(defineProps<Props>(), {
  avatar: '',
  broadcastCount: 0,
  text: '',
  notifierCount: 0,
});

const router = useRouter();

function goToMessages() {
  router.push({
    path: '/business/notifications/my-message',
    query: { read: 'false' },
  });
}

function goToBroadcasts() {
  router.push({
    path: '/business/notifications/my-broadcast',
    query: { read: 'false' },
  });
}
</script>
<template>
  <div class="card-box p-4 py-6 lg:flex">
    <VbenAvatar :alt="text" :src="avatar" class="size-20" />
    <div
      v-if="$slots.title || $slots.description"
      class="flex flex-col justify-center md:ml-6 md:mt-0"
    >
      <h1 v-if="$slots.title" class="text-md font-semibold md:text-xl">
        <slot name="title"></slot>
      </h1>
      <span v-if="$slots.description" class="mt-1 text-foreground/80">
        <slot name="description"></slot>
      </span>
    </div>
    <div class="mt-4 flex flex-1 justify-end md:mt-0">
      <div class="flex items-center gap-6">
        <div class="flex flex-col justify-center text-right">
          <span class="text-foreground/80">
            {{ $t('page.business.notifications.message') }}
          </span>
          <a
            class="cursor-pointer text-2xl transition hover:text-primary"
            href="/business/notifications/my-message"
            @click.prevent="goToMessages"
          >
            {{ $t('workbench.header.notifier.count', [notifierCount]) }}
          </a>
        </div>
        <div class="flex flex-col justify-center text-right">
          <span class="text-foreground/80">
            {{ $t('page.business.notifications.broadcast') }}
          </span>
          <a
            class="cursor-pointer text-2xl transition hover:text-primary"
            href="/business/notifications/my-broadcast"
            @click.prevent="goToBroadcasts"
          >
            {{ $t('workbench.header.notifier.count', [broadcastCount]) }}
          </a>
        </div>
      </div>
    </div>
  </div>
</template>
