<script lang="ts" setup>
import type { TenantDto } from '@abp/core';
import type { MenuInfo } from 'ant-design-vue/es/menu/src/interface';

import type { VbenFormProps } from '#/adapter/form';
import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { defineAsyncComponent, h, reactive } from 'vue';

import { Page, useVbenModal } from '@vben/common-ui';
import { createIconifyIcon } from '@vben/icons';
import { $t } from '@vben/locales';

import { TenantsPermissions, useAuthorization, useTenantsApi } from '@abp/core';
import {
  DeleteOutlined,
  EditOutlined,
  EllipsisOutlined,
} from '@ant-design/icons-vue';
import { Button, Dropdown, Menu, message, Modal, Space } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'TenantTable',
});

const { isGranted } = useAuthorization();
const { cancel, deleteApi, getPagedListApi } = useTenantsApi();

const MenuItem = Menu.Item;
const AuditLogIcon = createIconifyIcon('fluent-mdl2:compliance-audit');
const ConnectionIcon = createIconifyIcon('mdi:connection');
const FeatureIcon = createIconifyIcon('pajamas:feature-flag');

const formOptions: VbenFormProps = {
  // 默认展开
  collapsed: false,
  schema: [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
        autocomplete: 'off',
      },
      fieldName: 'filter',
      formItemClass: 'col-span-2 items-baseline',
      label: $t('AbpUi.Search'),
    },
  ],
  // 控制表单是否显示折叠按钮
  showCollapseButton: true,
  // 按下回车时是否提交表单
  submitOnEnter: true,
};

const gridOptions: VxeGridProps<TenantDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      align: 'center',
      field: 'name',
      sortable: true,
      title: $t('AbpSaas.DisplayName:Name'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'action' },
      title: $t('AbpUi.Actions'),
      visible: isGranted(
        [
          TenantsPermissions.Default,
          TenantsPermissions.Update,
          TenantsPermissions.Delete,
          TenantsPermissions.ManageConnectionStrings,
          TenantsPermissions.ManageFeatures,
        ],
        false,
      ),
      width: 240,
    },
  ],
  exportConfig: {},
  keepSource: true,
  height: 'auto',
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getPagedListApi({
          sorting,
          maxResultCount: page.pageSize,
          skipCount: (page.currentPage - 1) * page.pageSize,
          ...formValues,
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
    zoomOptions: {},
  },
};

const gridEvents: VxeGridListeners<TenantDto> = {
  cellClick: () => {},
  sortChange: () => {
    gridApi.query();
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const [TenantModal, modalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(() => import('./TenantModal.vue')),
});

const [TenantConnectionStringsModal, connectionStringsModalApi] = useVbenModal({
  connectedComponent: defineAsyncComponent(
    () => import('./ConnectionStringsModal.vue'),
  ),
});

const dataBaseOptions = reactive([
  { label: 'MySql', value: 'MySql' },
  { label: 'Oracle', value: 'Oracle' },
  { label: 'Postgres', value: 'Postgres' },
  { label: 'Sqlite', value: 'Sqlite' },
  { label: 'SqlServer', value: 'SqlServer' },
]);

const onCreate = () => {
  modalApi.setData({});
  modalApi.open();
};

const onUpdate = (row: TenantDto) => {
  modalApi.setData(row);
  modalApi.open();
};

const onDelete = (row: TenantDto) => {
  Modal.confirm({
    centered: true,
    content: $t('AbpSaas.TenantDeletionConfirmationMessage', [row.name]),
    onCancel: () => {
      cancel();
    },
    onOk: async () => {
      await deleteApi(row.id);
      message.success($t('AbpUi.DeletedSuccessfully'));
      await gridApi.query();
    },
    title: $t('AbpUi.AreYouSure'),
  });
};

const onMenuClick = (row: TenantDto, info: MenuInfo) => {
  switch (info.key) {
    case 'connection-strings': {
      connectionStringsModalApi.setData(row);
      connectionStringsModalApi.open();
      break;
    }
    case 'entity-changes': {
      message.info(`点击了实体变更 ${row.name}`);
      break;
    }
    case 'features': {
      message.info(`点击了管理功能 ${row.name}`);
      break;
    }
  }
};
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('AbpSaas.Tenants')">
      <template #toolbar-tools>
        <Button
          v-if="isGranted([TenantsPermissions.Create])"
          type="primary"
          @click="onCreate"
        >
          {{ $t('AbpSaas.NewTenant') }}
        </Button>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row justify-center">
          <Space>
            <Button
              v-if="isGranted([TenantsPermissions.Update])"
              :icon="h(EditOutlined)"
              block
              type="link"
              @click="onUpdate(row)"
            >
              {{ $t('AbpUi.Edit') }}
            </Button>
            <Button
              v-if="isGranted([TenantsPermissions.Delete])"
              :icon="h(DeleteOutlined)"
              block
              danger
              type="link"
              @click="onDelete(row)"
            >
              {{ $t('AbpUi.Delete') }}
            </Button>

            <Dropdown
              v-if="
                isGranted([
                  TenantsPermissions.ManageConnectionStrings,
                  TenantsPermissions.ManageFeatures,
                ])
              "
            >
              <template #overlay>
                <Menu @click="(info) => onMenuClick(row, info)">
                  <MenuItem
                    v-if="
                      isGranted([TenantsPermissions.ManageConnectionStrings])
                    "
                    key="connection-strings"
                    :icon="h(ConnectionIcon)"
                  >
                    {{ $t('AbpSaas.ConnectionStrings') }}
                  </MenuItem>
                  <MenuItem
                    v-if="isGranted([TenantsPermissions.ManageFeatures])"
                    key="features"
                    :icon="h(FeatureIcon)"
                  >
                    {{ $t('AbpSaas.ManageFeatures') }}
                  </MenuItem>
                  <MenuItem key="entity-changes" :icon="h(AuditLogIcon)">
                    {{ $t('AbpAuditLogging.EntitiesChanged') }}
                  </MenuItem>
                </Menu>
              </template>
              <Button :icon="h(EllipsisOutlined)" type="link" class="ml-2" />
            </Dropdown>
          </Space>
        </div>
      </template>
    </Grid>

    <TenantModal
      :data-base-options="dataBaseOptions"
      @change="() => gridApi.query()"
    />
    <TenantConnectionStringsModal :data-base-options="dataBaseOptions" />
  </Page>
</template>
<style scoped></style>
