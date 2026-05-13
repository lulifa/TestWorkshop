<script lang="ts" setup>
import type { IdentityUserDto } from '@abp/core';
import type { MenuInfo } from 'ant-design-vue/es/menu/src/interface';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h } from 'vue';

import { useAccess } from '@vben/access';
import { Page, useVbenModal } from '@vben/common-ui';
import { createIconifyIcon } from '@vben/icons';
import { $t } from '@vben/locales';

import {
  formatToDateTime,
  IdentityUserPermissions,
  useAbpStore,
  useUsersApi,
} from '@abp/core';
import {
  DeleteOutlined,
  EditOutlined,
  EllipsisOutlined,
  LockOutlined,
  PlusOutlined,
  UnlockOutlined,
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
  name: 'UserTable',
});

const UserModal = defineAsyncComponent(() => import('./UserModal.vue'));
const LockModal = defineAsyncComponent(() => import('./UserLockModal.vue'));
const PasswordModal = defineAsyncComponent(
  () => import('./UserPasswordModal.vue'),
);

const MenuItem = Menu.Item;
const CheckIcon = createIconifyIcon('ant-design:check-outlined');
const CloseIcon = createIconifyIcon('ant-design:close-outlined');
const PasswordIcon = createIconifyIcon('carbon:password');
const MenuOutlined = createIconifyIcon('heroicons-outline:menu-alt-3');
const PermissionsOutlined = createIconifyIcon('icon-park-outline:permissions');

const getLockEnd = computed(() => {
  return (row: IdentityUserDto) => {
    if (row.lockoutEnd) {
      const lockTime = new Date(row.lockoutEnd);
      if (lockTime) {
        // 锁定时间高于当前时间不显示
        const nowTime = new Date();
        return lockTime < nowTime;
      }
    }
    return true;
  };
});

const abpStore = useAbpStore();
const { hasAccessByCodes } = useAccess();
const { cancel, deleteApi, getPagedListApi, unLockApi } = useUsersApi();

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

const gridOptions: VxeGridProps<IdentityUserDto> = {
  columns: [
    {
      align: 'center',
      type: 'seq',
      width: 50,
    },
    {
      field: 'isActive',
      slots: { default: 'active' },
      sortable: true,
      title: $t('AbpIdentity.DisplayName:IsActive'),
    },
    {
      field: 'userName',
      minWidth: '100px',
      sortable: true,
      title: $t('AbpIdentity.DisplayName:UserName'),
    },
    {
      align: 'left',
      field: 'email',
      minWidth: '120px',
      slots: { default: 'email' },
      sortable: true,
      title: $t('AbpIdentity.DisplayName:Email'),
    },
    {
      field: 'surname',
      sortable: true,
      title: $t('AbpIdentity.DisplayName:Surname'),
    },
    {
      field: 'name',
      sortable: true,
      title: $t('AbpIdentity.DisplayName:Name'),
    },
    {
      align: 'left',
      field: 'phoneNumber',
      slots: { default: 'phoneNumber' },
      sortable: true,
      title: $t('AbpIdentity.DisplayName:PhoneNumber'),
    },
    {
      field: 'lockoutEnd',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : '';
      },
      sortable: true,
      title: $t('AbpIdentity.LockoutEnd'),
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

const gridEvents: VxeGridListeners<IdentityUserDto> = {
  cellClick: () => {},
  sortChange: () => {
    gridApi.query();
  },
};
const [UserEditModal, userModalApi] = useVbenModal({
  connectedComponent: UserModal,
});
const [UserLockModal, lockModalApi] = useVbenModal({
  connectedComponent: LockModal,
});
const [UserPasswordModal, pwdModalApi] = useVbenModal({
  connectedComponent: PasswordModal,
});

const [UserPermissionModal, permissionModalApi] = useVbenModal({
  connectedComponent: PermissionModal,
});

const [UserMenuModal, menuModalApi] = useVbenModal({
  connectedComponent: MenuAllotModal,
});
const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,
  gridEvents,
  gridOptions,
});

const handleAdd = () => {
  userModalApi.setData({});
  userModalApi.open();
};

const handleEdit = (row: IdentityUserDto) => {
  userModalApi.setData(row);
  userModalApi.open();
};

const handleDelete = (row: IdentityUserDto) => {
  Modal.confirm({
    centered: true,
    content: $t('AbpIdentity.UserDeletionConfirmationMessage', [row.userName]),
    onCancel: () => {
      cancel('User closed cancel delete modal.');
    },
    onOk: async () => {
      await deleteApi(row.id);
      message.success($t('AbpUi.DeletedSuccessfully'));
      await gridApi.query();
    },
    title: $t('AbpUi.AreYouSure'),
  });
};

const handleUnlock = async (row: IdentityUserDto) => {
  await unLockApi(row.id);
  await gridApi.query();
};

const handleMenuClick = async (row: IdentityUserDto, info: MenuInfo) => {
  switch (info.key) {
    case 'lock': {
      lockModalApi.setData(row);
      lockModalApi.open();
      break;
    }
    case 'menus': {
      menuModalApi.setData({
        identity: row.id,
      });
      menuModalApi.open();
      break;
    }
    case 'password': {
      pwdModalApi.setData(row);
      pwdModalApi.open();
      break;
    }
    case 'permissions': {
      const userId = abpStore.application?.currentUser.id;
      permissionModalApi.setData({
        displayName: row.userName,
        providerKey: row.id,
        providerName: 'U',
        readonly: userId === row.id,
      });
      permissionModalApi.open();
      break;
    }
    case 'unlock': {
      handleUnlock(row);
      break;
    }
  }
};
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('AbpIdentity.Users')">
      <template #toolbar-tools>
        <Button
          :icon="h(PlusOutlined)"
          type="primary"
          v-access:code="[IdentityUserPermissions.Create]"
          @click="handleAdd"
        >
          {{ $t('AbpIdentity.NewUser') }}
        </Button>
      </template>
      <template #active="{ row }">
        <div class="flex flex-row justify-center">
          <div :class="row.isActive ? 'text-green-600' : 'text-red-600'">
            <CheckIcon v-if="row.isActive" />
            <CloseIcon v-else />
          </div>
        </div>
      </template>
      <template #email="{ row }">
        <div class="flex flex-row">
          <Tag v-if="row.emailConfirmed" color="success">
            {{ $t('abp.account.settings.security.verified') }}
          </Tag>
          <Tag v-else color="warning">
            {{ $t('abp.account.settings.security.unVerified') }}
          </Tag>
          <span>{{ row.email }}</span>
        </div>
      </template>
      <template #phoneNumber="{ row }">
        <div class="flex flex-row">
          <div v-if="row.phoneNumber">
            <Tag v-if="row.phoneNumberConfirmed" color="success">
              {{ $t('abp.account.settings.security.verified') }}
            </Tag>
            <Tag v-else color="warning">
              {{ $t('abp.account.settings.security.unVerified') }}
            </Tag>
          </div>
          <span>{{ row.phoneNumber }}</span>
        </div>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row justify-center">
          <Space>
            <Button
              :icon="h(EditOutlined)"
              block
              type="link"
              v-access:code="[IdentityUserPermissions.Update]"
              @click="handleEdit(row)"
            >
              {{ $t('AbpUi.Edit') }}
            </Button>

            <Button
              :icon="h(DeleteOutlined)"
              block
              danger
              type="link"
              v-access:code="[IdentityUserPermissions.Delete]"
              @click="handleDelete(row)"
            >
              {{ $t('AbpUi.Delete') }}
            </Button>

            <Dropdown>
              <template #overlay>
                <Menu @click="(info) => handleMenuClick(row, info)">
                  <MenuItem
                    v-if="
                      hasAccessByCodes([IdentityUserPermissions.Update]) &&
                      row.isActive &&
                      getLockEnd(row)
                    "
                    key="lock"
                    :icon="h(LockOutlined)"
                  >
                    {{ $t('AbpIdentity.Lock') }}
                  </MenuItem>
                  <MenuItem
                    v-if="
                      hasAccessByCodes([IdentityUserPermissions.Update]) &&
                      row.isActive &&
                      !getLockEnd(row)
                    "
                    key="unlock"
                    :icon="h(UnlockOutlined)"
                  >
                    {{ $t('AbpIdentity.UnLock') }}
                  </MenuItem>
                  <MenuItem
                    v-if="
                      hasAccessByCodes([
                        IdentityUserPermissions.ManagePermissions,
                      ])
                    "
                    key="permissions"
                    :icon="h(PermissionsOutlined)"
                  >
                    {{ $t('AbpPermissionManagement.Permissions') }}
                  </MenuItem>
                  <MenuItem
                    v-if="hasAccessByCodes([IdentityUserPermissions.Update])"
                    key="password"
                    :icon="h(PasswordIcon)"
                  >
                    {{ $t('AbpIdentity.SetPassword') }}
                  </MenuItem>
                  <MenuItem
                    v-if="hasAccessByCodes(['TestWorkshop.Menu.ManageUsers'])"
                    key="menus"
                    :icon="h(MenuOutlined)"
                  >
                    {{ $t('TestWorkshop.Menu:Manage') }}
                  </MenuItem>
                </Menu>
              </template>
              <Button :icon="h(EllipsisOutlined)" type="link" class="ml-2" />
            </Dropdown>
          </Space>
        </div>
      </template>
    </Grid>
    <UserLockModal @change="() => gridApi.query()" />
    <UserEditModal @change="() => gridApi.query()" />
    <UserPasswordModal @change="() => gridApi.query()" />
    <UserPermissionModal />
    <UserMenuModal subject="user" />
  </Page>
</template>

<style lang="scss" scoped></style>
