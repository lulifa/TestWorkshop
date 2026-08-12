<script setup lang="ts">
import type { WorkbenchTrendItem } from '@vben/common-ui';

import { computed } from 'vue';
import { useRouter } from 'vue-router';

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  VbenIcon,
} from '@vben-core/shadcn-ui';

interface TrendItem extends WorkbenchTrendItem {
  kind: 'broadcast' | 'message';
}

interface Props {
  broadcasts?: WorkbenchTrendItem[];
  messages?: WorkbenchTrendItem[];
  title: string;
}

const props = withDefaults(defineProps<Props>(), {
  broadcasts: () => [],
  messages: () => [],
});

const router = useRouter();

const items = computed<TrendItem[]>(() =>
  [
    ...props.messages.map((item) => ({
      ...item,
      kind: 'message' as const,
    })),
    ...props.broadcasts.map((item) => ({
      ...item,
      kind: 'broadcast' as const,
    })),
  ].slice(0, 10),
);

function goTo(item: TrendItem) {
  router.push(
    item.kind === 'broadcast'
      ? {
          path: '/business/notifications/my-broadcast',
          query: { read: 'false' },
        }
      : {
          path: '/business/notifications/my-message',
          query: { read: 'false' },
        },
  );
}
</script>

<template>
  <Card>
    <CardHeader class="py-4">
      <CardTitle class="text-lg">{{ title }}</CardTitle>
    </CardHeader>
    <slot v-if="items.length === 0" name="empty"></slot>
    <CardContent v-else class="flex flex-wrap p-5 pt-0">
      <ul class="w-full divide-y divide-border" role="list">
        <li
          v-for="item in items"
          :key="`${item.kind}-${item.title}`"
          class="flex cursor-pointer justify-between gap-x-6 rounded-md px-2 py-5 transition hover:bg-accent/60"
          @click="goTo(item)"
        >
          <div class="flex min-w-0 items-center gap-x-4">
            <VbenIcon
              :icon="item.avatar"
              alt=""
              class="size-10 flex-none rounded-full"
            />
            <div class="min-w-0 flex-auto">
              <div class="flex items-center gap-2">
                <p
                  class="truncate text-sm font-semibold leading-6 text-foreground"
                  :title="item.title"
                >
                  {{ item.title }}
                </p>
                <span
                  class="shrink-0 rounded px-1.5 py-0.5 text-xs"
                  :class="
                    item.kind === 'broadcast'
                      ? 'bg-primary/10 text-primary'
                      : 'bg-foreground/5 text-foreground/60'
                  "
                >
                  {{ item.kind === 'broadcast' ? '通告' : '消息' }}
                </span>
              </div>
              <p
                class="mt-1 truncate text-xs leading-5 text-foreground/80"
                :title="item.content"
              >
                {{ item.content }}
              </p>
            </div>
          </div>
          <div class="hidden h-full shrink-0 sm:flex sm:flex-col sm:items-end">
            <span class="mt-6 text-xs leading-6 text-foreground/80">
              {{ item.date }}
            </span>
          </div>
        </li>
      </ul>
    </CardContent>
  </Card>
</template>
