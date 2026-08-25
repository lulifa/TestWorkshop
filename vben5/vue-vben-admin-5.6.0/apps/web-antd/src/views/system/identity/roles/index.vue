<script lang="ts" setup>
import type { IdentityRoleDto } from '@abp/core';
import type { MenuInfo } from 'ant-design-vue/es/menu/src/interface';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { defineAsyncComponent, h } from 'vue';

import { useAccess } from '@vben/access';
import { Page, useVbenModal } from '@vben/common-ui';
import { createIconifyIcon } from '@vben/icons';
import { $t } from '@vben/locales';

import { IdentityRolePermissions, useAbpStore, useRolesApi } from '@abp/core';
import {
  DeleteOutlined,
  EditOutlined,
  EllipsisOutlined,
} from '@ant-design/icons-vue';
import {
  Button,
  Dropdown,
  Menu,
  message,
  Modal,
  Space,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import MenuAllotModal from '#/views/modules/platform/menus/MenuAllotModal.vue';
import PermissionModal from '#/views/system/permissions/PermissionModal.vue';

defineOptions({
  name: 'RoleTable',
});

const MenuItem = Menu.Item;
const MenuOutlined = createIconifyIcon('heroicons-outline:menu-alt-3');
const PermissionsOutlined = createIconifyIcon('icon-park-outline:permissions');

const RoleModal = defineAsyncComponent(() => import('./RoleModal.vue'));

const abpStore = useAbpStore();
const { hasAccessByCodes } = useAccess();
const { cancel, deleteApi, getPagedListApi } = useRolesApi();

const [RolePermissionModal, permissionModalApi] = useVbenModal({
  connectedComponent: PermissionModal,
});
const [RoleMenuModal, menuModalApi] = useVbenModal({
  connectedComponent: MenuAllotModal,
});

const formOptions: VbenFormProps = {
  // 默认展开
  collapsed: false,
  schema: [
    {
      component: 'Input',
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

const gridOptions: VxeGridProps<IdentityRoleDto> = {
  columns: [
    {
      align: 'left',
      field: 'name',
      slots: { default: 'name' },
      sortable: true,
      title: $t('AbpIdentity.DisplayName:RoleName'),
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
  },
};

const gridEvents: VxeGridListeners<IdentityRoleDto> = {
  cellClick: () => {},
  sortChange: () => {
    gridApi.query();
  },
};
const [RoleEditModal, roleModalApi] = useVbenModal({
  connectedComponent: RoleModal,
});
const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const handleAdd = () => {
  roleModalApi.setData({});
  roleModalApi.open();
};

const handleEdit = (row: IdentityRoleDto) => {
  roleModalApi.setData(row);
  roleModalApi.open();
};

const handleDelete = (row: IdentityRoleDto) => {
  Modal.confirm({
    centered: true,
    content: $t('AbpIdentity.RoleDeletionConfirmationMessage', [row.name]),
    onCancel: () => {
      cancel('User closed cancel delete modal.');
    },
    onOk: async () => {
      await deleteApi(row.id);
      message.success($t('AbpUi.DeletedSuccessfully'));
      gridApi.query();
    },
    title: $t('AbpUi.AreYouSure'),
  });
};

const handleMenuClick = async (row: IdentityRoleDto, info: MenuInfo) => {
  switch (info.key) {
    case 'menus': {
      menuModalApi.setData({
        identity: row.name,
      });
      menuModalApi.open();
      break;
    }
    case 'permissions': {
      permissionModalApi.setData({
        displayName: row.name,
        providerKey: row.name,
        providerName: 'R',
      });
      permissionModalApi.open();
      break;
    }
  }
};

function onPermissionChange(_name: string, key: string) {
  const roles = abpStore.application?.currentUser.roles ?? [];
  if (roles.includes(key)) {
    // publish(Events.PermissionChange);
  }
}
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('AbpIdentity.Roles')">
      <template #toolbar-tools>
        <Button
          v-if="hasAccessByCodes([IdentityRolePermissions.Create])"
          type="primary"
          @click="handleAdd"
        >
          {{ $t('AbpIdentity.NewRole') }}
        </Button>
      </template>
      <template #name="{ row }">
        <Tag v-if="row.isStatic" color="#8baac4" style="margin-right: 5px">
          {{ $t('AbpIdentity.Static') }}
        </Tag>
        <Tag v-if="row.isDefault" color="#108ee9" style="margin-right: 5px">
          {{ $t('AbpIdentity.DisplayName:IsDefault') }}
        </Tag>
        <Tag v-if="row.isPublic" color="#87d068" style="margin-right: 5px">
          {{ $t('AbpIdentity.Public') }}
        </Tag>
        <span>{{ row.name }}</span>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row justify-center">
          <Space>
            <Button
              v-if="hasAccessByCodes([IdentityRolePermissions.Update])"
              :icon="h(EditOutlined)"
              block
              type="link"
              @click="handleEdit(row)"
            >
              {{ $t('AbpUi.Edit') }}
            </Button>
            <Button
              v-if="
                row.isStatic === false &&
                hasAccessByCodes([IdentityRolePermissions.Delete])
              "
              :icon="h(DeleteOutlined)"
              block
              danger
              type="link"
              @click="handleDelete(row)"
            >
              {{ $t('AbpUi.Delete') }}
            </Button>
            <Dropdown>
              <template #overlay>
                <Menu @click="(info) => handleMenuClick(row, info)">
                  <MenuItem
                    v-if="
                      hasAccessByCodes([
                        IdentityRolePermissions.ManagePermissions,
                      ])
                    "
                    key="permissions"
                    :icon="h(PermissionsOutlined)"
                  >
                    {{ $t('AbpPermissionManagement.Permissions') }}
                  </MenuItem>
                  <MenuItem
                    v-if="hasAccessByCodes(['TestWorkshop.Menu.ManageRoles'])"
                    key="menus"
                    :icon="h(MenuOutlined)"
                  >
                    {{ $t('TestWorkshop.Menu:Manage') }}
                  </MenuItem>
                </Menu>
              </template>
              <Button :icon="h(EllipsisOutlined)" type="link" />
            </Dropdown>
          </Space>
        </div>
      </template>
    </Grid>
    <RoleEditModal @change="() => gridApi.query()" />
    <RolePermissionModal @change="onPermissionChange" />
    <RoleMenuModal subject="role" />
  </Page>
</template>

<style lang="scss" scoped></style>
