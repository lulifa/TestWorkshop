<script setup lang="ts">
import type { IdentityRoleDto } from '@abp/core';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { computed, defineAsyncComponent, h, nextTick, watch } from 'vue';

import { useAccess } from '@vben/access';
import { Page, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  OrganizationUnitPermissions,
  useOrganizationUnitsApi,
  useRolesApi,
} from '@abp/core';
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons-vue';
import { Button, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

defineOptions({
  name: 'OrganizationUnitRoleTable',
});

const props = defineProps<{
  selectedKey?: string;
}>();

const SelectRoleModal = defineAsyncComponent(
  () => import('./SelectRoleModal.vue'),
);

const { hasAccessByCodes } = useAccess();
const { addRoles, getRoleListApi } = useOrganizationUnitsApi();
const { cancel, removeOrganizationUnitApi } = useRolesApi();

const getAddRoleEnabled = computed(() => {
  return (
    props.selectedKey &&
    hasAccessByCodes([OrganizationUnitPermissions.ManageRoles])
  );
});

const gridOptions: VxeGridProps<IdentityRoleDto> = {
  columns: [
    {
      field: 'name',
      minWidth: '100px',
      sortable: true,
      title: $t('AbpIdentity.DisplayName:RoleName'),
    },
    {
      field: 'action',
      fixed: 'right',
      slots: { default: 'actions' },
      title: $t('AbpUi.Actions'),
      width: 180,
    },
  ],
  exportConfig: {},
  keepSource: true,
  height: 'auto',
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        if (!props.selectedKey) {
          return {
            totalCount: 0,
            items: [],
          };
        }
        const sorting = sort.order ? `${sort.field} ${sort.order}` : undefined;
        return await getRoleListApi(props.selectedKey!, {
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
const [Grid, gridApi] = useVbenVxeGrid({
  gridEvents,
  gridOptions,
});

const [RoleModal, roleModalApi] = useVbenModal({
  connectedComponent: SelectRoleModal,
});

const onRefresh = () => {
  return nextTick(gridApi.query);
};

const onDelete = (row: IdentityRoleDto) => {
  Modal.confirm({
    centered: true,
    content: $t('AbpIdentity.OrganizationUnit:AreYouSureRemoveRole', [
      row.name,
    ]),
    onCancel: () => {
      cancel('User closed cancel delete modal.');
    },
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await removeOrganizationUnitApi(row.id, props.selectedKey!);
        await onRefresh();
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('AbpUi.AreYouSure'),
  });
};

const onShowRole = () => {
  roleModalApi.setData({
    id: props.selectedKey,
  });
  roleModalApi.open();
};

const onCreateRole = async (roles: IdentityRoleDto[]) => {
  try {
    roleModalApi.setState({
      submitting: true,
    });
    await addRoles(props.selectedKey!, {
      roleIds: roles.map((item) => item.id),
    });
    roleModalApi.close();
    await gridApi.query();
  } finally {
    roleModalApi.setState({
      submitting: false,
    });
  }
};
watch(() => props.selectedKey, onRefresh);
</script>

<template>
  <Page class="vp-raw h-[300px] w-full">
    <Grid :table-title="$t('AbpIdentity.Roles')">
      <template #toolbar-tools>
        <Button
          v-if="getAddRoleEnabled"
          :icon="h(PlusOutlined)"
          type="primary"
          @click="onShowRole"
        >
          {{ $t('AbpIdentity.OrganizationUnit:AddRole') }}
        </Button>
      </template>
      <template #actions="{ row }">
        <Button
          :icon="h(DeleteOutlined)"
          danger
          type="link"
          v-access:code="[OrganizationUnitPermissions.ManageRoles]"
          @click="onDelete(row)"
        >
          {{ $t('AbpUi.Delete') }}
        </Button>
      </template>
    </Grid>
    <RoleModal @confirm="onCreateRole" />
  </Page>
</template>

<style scoped></style>
