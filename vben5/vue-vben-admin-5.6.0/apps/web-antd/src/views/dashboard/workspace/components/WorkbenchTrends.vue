<script setup lang="ts">
import type { WorkbenchTrendItem } from '@vben/common-ui';

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  VbenIcon,
} from '@vben-core/shadcn-ui';

interface Props {
  items?: WorkbenchTrendItem[];
  title: string;
}

defineOptions({
  name: 'WorkbenchTrends',
});

withDefaults(defineProps<Props>(), {
  items: () => [],
});
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
          :key="item.title"
          class="flex justify-between gap-x-6 py-5"
        >
          <div class="flex min-w-0 items-center gap-x-4">
            <VbenIcon
              :icon="item.avatar"
              alt=""
              class="size-10 flex-none rounded-full"
            />
            <div class="min-w-0 flex-auto">
              <p
                class="text-sm font-semibold leading-6 text-foreground"
                :title="item.title"
              >
                {{ item.title }}
              </p>
              <!-- eslint-disable vue/no-v-html -->
              <p
                class="mt-1 truncate text-xs leading-5 text-foreground/80 *:text-primary"
                :title="item.content"
                v-html="item.content"
              ></p>
            </div>
          </div>
          <div class="hidden h-full shrink-0 sm:flex sm:flex-col sm:items-end">
            <span
              class="mt-6 text-xs leading-6 text-foreground/80"
              :title="item.date"
            >
              {{ item.date }}
            </span>
          </div>
        </li>
      </ul>
    </CardContent>
  </Card>
</template>
