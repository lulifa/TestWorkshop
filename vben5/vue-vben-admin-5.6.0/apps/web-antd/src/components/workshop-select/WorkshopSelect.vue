<script lang="ts" setup>
import type { OrganizationUnitDto } from '@abp/core';

import { computed, onMounted, ref } from 'vue';

import { listToTree, useWorkshopDeviceApi } from '@abp/core';
import { Select, TreeSelect } from 'ant-design-vue';

interface WorkshopTreeNode {
  children: WorkshopTreeNode[];
  disabled: boolean;
  label: string;
  value: string;
}

type OrganizationUnitNode = OrganizationUnitDto & {
  children: OrganizationUnitNode[];
};

withDefaults(
  defineProps<{
    allowClear?: boolean;
    disabled?: boolean;
    mode?: 'flat' | 'tree';
    modelValue?: string;
    placeholder?: string;
  }>(),
  {
    allowClear: true,
    disabled: false,
    mode: 'flat',
    modelValue: undefined,
    placeholder: '全部车间',
  },
);

const emit = defineEmits<{
  change: [value?: string];
  'update:modelValue': [value?: string];
}>();

const { getOrganizationUnitsApi } = useWorkshopDeviceApi();
const items = ref<OrganizationUnitDto[]>([]);
const loading = ref(false);

function getLabel(item: OrganizationUnitDto) {
  const businessCode = item.extraProperties?.BusinessCode;
  return businessCode
    ? `${item.displayName} (${businessCode})`
    : item.displayName;
}

const flatOptions = computed(() =>
  items.value
    .filter((item) => item.extraProperties?.BusinessCode)
    .map((item) => ({
      label: getLabel(item),
      value: item.id,
    })),
);

function toTreeNode(node: OrganizationUnitNode): WorkshopTreeNode {
  const children = node.children.map((child) => toTreeNode(child));
  return {
    children,
    disabled: children.length > 0,
    label: getLabel(node),
    value: node.id,
  };
}

const treeData = computed<WorkshopTreeNode[]>(() => {
  const tree = listToTree<OrganizationUnitNode>(items.value, {
    id: 'id',
    pid: 'parentId',
  });
  return tree.map((node) => toTreeNode(node));
});

onMounted(async () => {
  loading.value = true;
  try {
    const { items: organizationUnits } = await getOrganizationUnitsApi();
    items.value = organizationUnits;
  } finally {
    loading.value = false;
  }
});

function onUpdate(value: unknown) {
  const normalized = Array.isArray(value) ? value[0] : value;
  const result =
    typeof normalized === 'number' || typeof normalized === 'string'
      ? String(normalized)
      : undefined;
  emit('update:modelValue', result);
  emit('change', result);
}
</script>

<template>
  <TreeSelect
    v-if="mode === 'tree'"
    :allow-clear="allowClear"
    :disabled="disabled"
    :loading="loading"
    :placeholder="placeholder"
    :show-search="true"
    :tree-data="treeData"
    :tree-default-expand-all="true"
    tree-node-filter-prop="label"
    :value="modelValue"
    class="w-full"
    @update:value="onUpdate"
  />
  <Select
    v-else
    :allow-clear="allowClear"
    :disabled="disabled"
    :loading="loading"
    :options="flatOptions"
    :placeholder="placeholder"
    :show-search="true"
    option-filter-prop="label"
    :value="modelValue"
    class="w-full"
    @update:value="onUpdate"
  />
</template>
