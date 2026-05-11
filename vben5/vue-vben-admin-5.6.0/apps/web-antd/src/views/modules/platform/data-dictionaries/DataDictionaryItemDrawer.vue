<script lang="ts" setup>
import type { DataDto, DataItemDto } from '@abp/core';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { defineAsyncComponent, h, ref } from 'vue';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  DataValueType,
  isNullOrWhiteSpace,
  sortby,
  useDataDictionariesApi,
} from '@abp/core';
import {
  CheckOutlined,
  CloseOutlined,
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
} from '@ant-design/icons-vue';
import { Button, message, Modal, Space } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

const { deleteItemApi, getApi } = useDataDictionariesApi();

const dataItems = ref<DataItemDto[]>([]);
const valueTypeMaps: { [key: number]: string } = {
  [DataValueType.Array]: 'Array',
  [DataValueType.Boolean]: 'Boolean',
  [DataValueType.Date]: 'Date',
  [DataValueType.DateTime]: 'DateTime',
  [DataValueType.Numeic]: 'Number',
  [DataValueType.Object]: 'Object',
  [DataValueType.String]: 'String',
};

const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-2/3',
  async onOpenChange(isOpen) {
    if (isOpen) {
      const { name } = drawerApi.getData<DataDto>();
      drawerApi.setState({
        title: `${$t('TestWorkshop.DisplayName:DataDictionary')} - ${name}`,
      });
      await onGet();
    }
  },
  showConfirmButton: false,
  title: '字典项目',
});

const gridOptions: VxeGridProps<DataItemDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      align: 'left',
      field: 'name',
      minWidth: 150,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:Name'),
      treeNode: true,
    },
    {
      align: 'left',
      field: 'displayName',
      minWidth: 150,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:DisplayName'),
    },
    {
      align: 'left',
      field: 'description',
      minWidth: 250,
      title: $t('TestWorkshop.DisplayName:Description'),
    },
    {
      align: 'left',
      field: 'valueType',
      formatter: ({ row }) => {
        return valueTypeMaps[row.valueType] ?? row.valueType;
      },
      minWidth: 80,
      sortable: true,
      title: $t('TestWorkshop.DisplayName:ValueType'),
    },
    {
      align: 'left',
      field: 'defaultValue',
      minWidth: 120,
      title: $t('TestWorkshop.DisplayName:DefaultValue'),
    },
    {
      align: 'left',
      field: 'allowBeNull',
      minWidth: 60,
      slots: { default: 'allowBeNull' },
      sortable: true,
      title: $t('TestWorkshop.DisplayName:AllowBeNull'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      width: 240,
    },
  ],
  exportConfig: {},
  keepSource: true,
  height: 'auto',
  pagerConfig: {
    pageSize: 15,
    pageSizes: [10, 15, 30, 50, 100],
  },
  toolbarConfig: {
    custom: true,
    export: true,
    refresh: false,
    zoom: true,
  },
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }) => {
        let items = sortby(dataItems.value, sort.field);
        if (sort.order === 'desc') {
          items = items.toReversed();
        }
        const result = {
          totalCount: dataItems.value.length,
          items: items.slice(
            (page.currentPage - 1) * page.pageSize,
            page.currentPage * page.pageSize,
          ),
        };
        return new Promise((resolve) => {
          resolve(result);
        });
      },
    },
    response: {
      total: 'totalCount',
      list: 'items',
    },
  },
};

const gridEvents: VxeGridListeners<DataItemDto> = {
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  gridEvents,
  gridOptions,
});

const [DataDictionaryItemModal, itemModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./DataDictionaryItemModal.vue'),
  ),
});

async function onGet() {
  const { id } = drawerApi.getData<DataDto>();
  if (isNullOrWhiteSpace(id)) {
    return;
  }
  try {
    drawerApi.setState({ loading: true });
    const dto = await getApi(id);
    dataItems.value = dto.items;
    setTimeout(() => gridApi.reload(), 100);
  } finally {
    drawerApi.setState({
      loading: false,
    });
  }
}

function onCreate() {
  const data = drawerApi.getData<DataDto>();
  itemModalApi.setData({ data });
  itemModalApi.open();
}

function onUpdate(row: DataItemDto) {
  const data = drawerApi.getData<DataDto>();
  itemModalApi.setData({ data, item: row });
  itemModalApi.open();
}

function onDelete(row: DataItemDto) {
  Modal.confirm({
    afterClose: () => {
      gridApi.setLoading(false);
    },
    centered: true,
    content: `${$t('AbpUi.ItemWillBeDeletedMessage')}`,
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        const { id } = drawerApi.getData<DataDto>();
        await deleteItemApi(id, row.name);
        message.success($t('AbpUi.DeletedSuccessfully'));
        onGet();
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('AbpUi.AreYouSure'),
  });
}
</script>
<template>
  <div>
    <Page auto-content-height>
      <Drawer>
        <Grid :table-title="$t('TestWorkshop.Data:Items')">
          <template #toolbar-tools>
            <Button :icon="h(PlusOutlined)" type="primary" @click="onCreate">
              {{ $t('TestWorkshop.Data:AppendItem') }}
            </Button>
          </template>
          <template #allowBeNull="{ row }">
            <div class="flex flex-row justify-center">
              <CheckOutlined v-if="row.allowBeNull" class="text-green-500" />
              <CloseOutlined v-else class="text-red-500" />
            </div>
          </template>
          <template #action="{ row }">
            <div class="flex flex-row justify-center">
              <Space>
                <Button
                  :icon="h(EditOutlined)"
                  block
                  type="link"
                  @click="onUpdate(row)"
                >
                  {{ $t('AbpUi.Edit') }}
                </Button>
                <Button
                  v-if="!row.isStatic"
                  :icon="h(DeleteOutlined)"
                  block
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

        <DataDictionaryItemModal @change="onGet" />
      </Drawer>
    </Page>
  </div>
</template>

<style scoped></style>
