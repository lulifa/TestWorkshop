<script lang="ts" setup>
import type {
  OrganizationUnitDto,
  WorkshopDeviceDto,
  WorkshopDeviceTypeDto,
} from '@abp/core';
import type { DataNode } from 'ant-design-vue/es/tree';
import type { Key } from 'ant-design-vue/es/vc-table/interface';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h, onMounted, ref } from 'vue';

import { Page, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  DeviceType,
  formatToDateTime,
  listToTree,
  useAuthorization,
  useWorkshopDeviceApi,
  WorkshopDevicePermissions,
} from '@abp/core';
import {
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
} from '@ant-design/icons-vue';
import { Button, Card, message, Modal, Space, Tag, Tree } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'WorkshopDeviceManagement',
});

const { isGranted } = useAuthorization();
const { deleteApi, getListApi, getOrganizationUnitsApi, getTypesApi } =
  useWorkshopDeviceApi();

const organizationUnits = ref<OrganizationUnitDto[]>([]);
const deviceTypes = ref<WorkshopDeviceTypeDto[]>([]);
const expandedKeys = ref<string[]>([]);
const selectedOrganizationUnitId = ref<string>();
const treeData = ref<DataNode[]>([]);

const organizationUnitNames = computed(
  () =>
    new Map(organizationUnits.value.map((item) => [item.id, item.displayName])),
);

const typeLabelMap = computed(
  () =>
    new Map(deviceTypes.value.map((item) => [item.value, item.displayName])),
);

function toOrganizationTree(items: OrganizationUnitDto[]) {
  const tree = listToTree(items, {
    id: 'id',
    pid: 'parentId',
  });
  return tree.map((node) => toTreeNode(node));
}

function toTreeNode(node: any): any {
  const children = (node.children ?? []).map((child: any) => toTreeNode(child));
  return {
    children,
    disabled: children.length > 0,
    label: node.displayName,
    value: node.id,
  };
}

const formOptions: VbenFormProps = {
  collapsed: false,
  commonConfig: {
    colon: true,
    componentProps: {
      class: 'w-full',
    },
  },
  schema: [
    {
      component: 'Input',
      fieldName: 'filter',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.DisplayName:CodeOrName'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        afterFetch: (result: { items: WorkshopDeviceTypeDto[] }) => {
          return result.items.map((item) => ({
            label: item.displayName || item.name,
            value: item.value,
          }));
        },
        allowClear: true,
        api: getTypesApi,
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'type',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.DisplayName:Type'),
    },
    {
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: true,
        api: async () => {
          const { items } = await getOrganizationUnitsApi();
          return toOrganizationTree(items);
        },
        childrenField: 'children',
        labelField: 'label',
        onChange: (value: string) => {
          selectedOrganizationUnitId.value = value;
          gridApi.query();
        },
        valueField: 'value',
      },
      fieldName: 'organizationUnitId',
      formItemClass: 'col-span-1 items-baseline',
      label: $t('TestWorkshop.DisplayName:OrganizationUnit'),
    },
  ],
  showCollapseButton: false,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<WorkshopDeviceDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      field: 'code',
      minWidth: 140,
      title: $t('TestWorkshop.DisplayName:Code'),
    },
    {
      field: 'name',
      minWidth: 180,
      title: $t('TestWorkshop.DisplayName:Name'),
    },
    {
      align: 'center',
      field: 'type',
      minWidth: 100,
      slots: { default: 'type' },
      title: $t('TestWorkshop.DisplayName:Type'),
    },
    {
      field: 'organizationUnitId',
      minWidth: 140,
      slots: { default: 'organizationUnit' },
      title: $t('TestWorkshop.DisplayName:OrganizationUnit'),
    },
    {
      field: 'creationTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '';
      },
      minWidth: 170,
      title: $t('TestWorkshop.DisplayName:CreationTime'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      width: 220,
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        const values = formValues as Record<string, any>;
        return await getListApi({
          filter: values?.filter,
          maxResultCount: page.pageSize,
          organizationUnitId:
            values?.organizationUnitId ?? selectedOrganizationUnitId.value,
          skipCount: (page.currentPage - 1) * page.pageSize,
          sorting,
          type: values?.type,
        });
      },
      queryAll: async (params) => {
        const { sort } = params;
        const formValues = await gridApi.formApi.getValues();
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        const values = formValues as Record<string, any>;
        return await getListApi({
          filter: values?.filter,
          isPaged: false,
          organizationUnitId:
            values?.organizationUnitId ?? selectedOrganizationUnitId.value,
          sorting,
          type: values?.type,
        });
      },
    },
    response: {
      total: 'totalCount',
      list: 'items',
    },
  },
  toolbarConfig: {
    custom: true,
    export: true,
    refresh: true,
    refreshOptions: {
      code: 'query',
    },
    zoom: true,
  },
};

const gridEvents: VxeGridListeners<WorkshopDeviceDto> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const [WorkshopDeviceModal, workshopDeviceModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./WorkshopDeviceModal.vue'),
  ),
});

function buildTree(
  items: OrganizationUnitDto[],
  parentId: null | string = null,
): DataNode[] {
  return items
    .filter((item) => (item.parentId ?? null) === parentId)
    .map((item) => ({
      children: buildTree(items, item.id),
      key: item.id,
      title: item.displayName,
    }));
}

function collectKeys(nodes: DataNode[]): string[] {
  return nodes.flatMap((node) => [
    String(node.key),
    ...collectKeys(node.children ?? []),
  ]);
}

async function loadOrganizationUnits() {
  const { items } = await getOrganizationUnitsApi();
  organizationUnits.value = items;
  treeData.value = buildTree(items);
  expandedKeys.value = collectKeys(treeData.value);
  const rootItem = items.find((item) => !item.parentId);
  selectedOrganizationUnitId.value = rootItem?.id ?? items[0]?.id;
}

async function loadDeviceTypes() {
  const { items } = await getTypesApi();
  deviceTypes.value = items;
}

function onSelect(selectedKeys: Key[]) {
  selectedOrganizationUnitId.value =
    selectedKeys.length > 0 ? String(selectedKeys[0]) : undefined;
  gridApi.formApi.setFieldValue(
    'organizationUnitId',
    selectedOrganizationUnitId.value,
  );
  gridApi.query();
}

function onExpand(keys: Key[]) {
  expandedKeys.value = keys.map(String);
}

function onCreate() {
  workshopDeviceModalApi.setData({});
  workshopDeviceModalApi.open();
}

function onUpdate(row: WorkshopDeviceDto) {
  workshopDeviceModalApi.setData(row);
  workshopDeviceModalApi.open();
}

function onDelete(row: WorkshopDeviceDto) {
  Modal.confirm({
    centered: true,
    content: $t('AbpUi.ItemWillBeDeletedMessage'),
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await deleteApi(row.id);
        message.success($t('AbpUi.DeletedSuccessfully'));
        await gridApi.query();
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('AbpUi.AreYouSure'),
  });
}

function typeLabel(type: DeviceType, typeName?: string) {
  return (
    typeLabelMap.value.get(type) ?? typeName ?? DeviceType[type] ?? String(type)
  );
}

onMounted(async () => {
  await Promise.all([loadOrganizationUnits(), loadDeviceTypes()]);
  await gridApi.query();
});
</script>

<template>
  <Page auto-content-height>
    <div class="flex h-full flex-row gap-2">
      <Card
        class="h-full w-[280px] shrink-0"
        :title="$t('TestWorkshop.DisplayName:OrganizationUnit')"
      >
        <Tree
          :expanded-keys="expandedKeys"
          :selected-keys="
            selectedOrganizationUnitId ? [selectedOrganizationUnitId] : []
          "
          :tree-data="treeData"
          block-node
          @expand="onExpand"
          @select="onSelect"
        />
      </Card>
      <div class="min-w-0 flex-1">
        <Grid :table-title="$t('page.business.workshopdevices')">
          <template #toolbar-tools>
            <Button
              v-if="isGranted([WorkshopDevicePermissions.Create])"
              :icon="h(PlusOutlined)"
              type="primary"
              @click="onCreate"
            >
              {{ $t('TestWorkshop.WorkshopDevice:AddNew') }}
            </Button>
          </template>
          <template #type="{ row }">
            <Tag color="blue">{{ typeLabel(row.type, row.typeName) }}</Tag>
          </template>
          <template #organizationUnit="{ row }">
            {{
              organizationUnitNames.get(row.organizationUnitId) ??
              row.organizationUnitId
            }}
          </template>
          <template #action="{ row }">
            <div class="flex flex-row justify-center">
              <Space>
                <Button
                  v-if="isGranted([WorkshopDevicePermissions.Update])"
                  :icon="h(EditOutlined)"
                  type="link"
                  @click="onUpdate(row)"
                >
                  {{ $t('AbpUi.Edit') }}
                </Button>
                <Button
                  v-if="isGranted([WorkshopDevicePermissions.Delete])"
                  :icon="h(DeleteOutlined)"
                  danger
                  type="link"
                  @click="onDelete(row)"
                >
                  {{ $t('AbpUi.Delete') }}
                </Button>
              </Space>
            </div>
          </template>
        </Grid>
      </div>
    </div>

    <WorkshopDeviceModal @change="() => gridApi.query()" />
  </Page>
</template>

<style scoped></style>
