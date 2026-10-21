<script setup lang="ts">
import type { OrganizationUnitDto } from '@abp/core';
import type {
  AntTreeNodeDropEvent,
  DataNode,
  EventDataNode,
} from 'ant-design-vue/es/tree';
import type { Key } from 'ant-design-vue/es/vc-table/interface';

import { defineAsyncComponent, h, onMounted, ref, watchEffect } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { createIconifyIcon } from '@vben/icons';
import { $t } from '@vben/locales';

import { useOrganizationUnitsApi } from '@abp/core';
import {
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
  RedoOutlined,
} from '@ant-design/icons-vue';
import {
  Button,
  Card,
  Dropdown,
  Menu,
  message,
  Modal,
  Tree,
} from 'ant-design-vue';

import PermissionModal from '#/views/system/permissions/PermissionModal.vue';

defineOptions({
  name: 'OrganizationUnitTree',
});

const emits = defineEmits<{
  (event: 'selected', id?: string): void;
}>();

const { deleteApi, getApi, getChildrenApi, getRootListApi, moveTo } =
  useOrganizationUnitsApi();

const MenuItem = Menu.Item;
const PermissionsOutlined = createIconifyIcon('icon-park-outline:permissions');
const OrganizationUnitModal = defineAsyncComponent(
  () => import('./OrganizationUnitModal.vue'),
);
interface ContextMenuActionMap {
  [key: string]: (id: string) => Promise<void> | void;
}
const actionsMap: ContextMenuActionMap = {
  create: onCreate,
  delete: onDelete,
  permissions: onPermissions,
  refresh: onRefresh,
  update: onUpdate,
};

const organizationUnits = ref<DataNode[]>([]);
const loadedKeys = ref<string[]>([]);
const expandedKeys = ref<string[]>([]);
const selectedKey = ref<string>();

const [OrganizationUnitEditModal, editModalApi] = useVbenModal({
  connectedComponent: OrganizationUnitModal,
});
const [OrganizationUnitPermissionModal, permissionModalApi] = useVbenModal({
  connectedComponent: PermissionModal,
});

/** 刷新组织机构树 */
async function onRefresh() {
  loadedKeys.value = [];
  const { items } = await getRootListApi();
  organizationUnits.value = items.map((item) =>
    toOrganizationUnitTreeNode(item),
  );
}

/** 加载组织机构树节点 */
async function onLoad(node: EventDataNode) {
  await loadChildren(node.dataRef!);
}

async function loadChildren(node: DataNode) {
  const nodeKey = String(node.key);
  const { items } = await getChildrenApi({ id: nodeKey });
  node.isLeaf = items.length === 0;
  node.children = items.map((item) => toOrganizationUnitTreeNode(item));
  organizationUnits.value = [...organizationUnits.value];
  if (!loadedKeys.value.includes(nodeKey)) {
    loadedKeys.value.push(nodeKey);
  }
}

function toOrganizationUnitTreeNode(item: OrganizationUnitDto): DataNode {
  return {
    children: [],
    isLeaf: false,
    key: item.id,
    title:
      item.displayName +
      (item.extraProperties?.BusinessCode
        ? ` (${item.extraProperties.BusinessCode})`
        : ''),
  };
}

/** 右键点击事件 */
function onRightClick() {
  // 阻止默认事件
}

/** 创建组织机构树 */
function onCreate(parentId?: string) {
  editModalApi.setData({ parentId });
  editModalApi.open();
}

async function onOrganizationUnitChanged(result: {
  id: string;
  parentId?: string;
}) {
  const parentId = result.parentId;
  if (!parentId) {
    await onRefresh();
    return;
  }

  const parentNode = findOrganizationUnitNode(
    organizationUnits.value,
    parentId,
  );
  if (!parentNode) {
    await onRefresh();
    return;
  }

  await loadChildren(parentNode);
  selectedKey.value = result.id;
  expandedKeys.value = [...new Set([parentId, ...expandedKeys.value])];
}

function findOrganizationUnitNode(
  nodes: DataNode[],
  id: string,
): DataNode | undefined {
  for (const node of nodes) {
    if (String(node.key) === id) {
      return node;
    }

    const childNode = findOrganizationUnitNode(node.children ?? [], id);
    if (childNode) {
      return childNode;
    }
  }
}

/** 编辑组织机构树 */
function onUpdate(id: string) {
  editModalApi.setData({ id });
  editModalApi.open();
}

/** 编辑组织机构树权限 */
async function onPermissions(id: string) {
  const dto = await getApi(id);
  permissionModalApi.setData({
    displayName: dto.displayName,
    providerKey: id,
    providerName: 'O',
  });
  permissionModalApi.open();
}

/** 删除组织机构 */
function onDelete(id: string) {
  Modal.confirm({
    centered: true,
    content: $t('AbpUi.ItemWillBeDeletedMessage'),
    maskClosable: false,
    onOk: async () => {
      await deleteApi(id);
      message.success($t('AbpUi.DeletedSuccessfully'));
      onRefresh();
    },
    title: $t('AbpUi.AreYouSure'),
  });
}

/** 右键菜单点击事件 */
function onMenuClick(id: string, eventKey: Key) {
  actionsMap[eventKey]!(id);
}

/** 组织机构选择变化事件 */
function onSelectChange(selectedKeys: Key[]) {
  if (selectedKeys.length === 0) {
    selectedKey.value = undefined;
    return;
  }
  selectedKey.value = String(selectedKeys[0]);
}

function onExpand(keys: Key[]) {
  expandedKeys.value = keys.map(String);
}

/** 组织机构节点拖动事件 */
function onDrop(info: AntTreeNodeDropEvent) {
  if (!info.dragNode.eventKey) {
    return;
  }
  const eventKey = String(info.dragNode.eventKey);
  const api =
    info.dropPosition === -1
      ? moveTo(eventKey, undefined) // parent
      : moveTo(eventKey, String(info.node.eventKey)); // children
  api.then(onRefresh);
}

onMounted(onRefresh);

watchEffect(() => {
  emits('selected', selectedKey.value);
});
</script>

<template>
  <Card :title="$t('AbpIdentity.OrganizationUnit:Tree')">
    <template #extra>
      <Button :icon="h(PlusOutlined)" type="primary" @click="() => onCreate()">
        {{ $t('AbpIdentity.OrganizationUnit:AddRoot') }}
      </Button>
    </template>
    <Tree
      :expanded-keys="expandedKeys"
      :load-data="onLoad"
      :loaded-keys="loadedKeys"
      :selected-keys="selectedKey ? [selectedKey] : []"
      :tree-data="organizationUnits"
      block-node
      draggable
      @drop="onDrop"
      @expand="onExpand"
      @right-click="onRightClick"
      @select="onSelectChange"
    >
      <template #title="{ key: treeKey, title }">
        <Dropdown :trigger="['contextmenu']">
          <span> {{ title }}</span>
          <template #overlay>
            <Menu @click="({ key: menuKey }) => onMenuClick(treeKey, menuKey)">
              <MenuItem key="update" :icon="h(EditOutlined)">
                {{ $t('AbpUi.Edit') }}
              </MenuItem>
              <MenuItem key="create" :icon="h(PlusOutlined)">
                {{ $t('AbpIdentity.OrganizationUnit:AddChildren') }}
              </MenuItem>
              <MenuItem key="delete" :icon="h(DeleteOutlined)">
                {{ $t('AbpUi.Delete') }}
              </MenuItem>
              <MenuItem key="permissions" :icon="h(PermissionsOutlined)">
                {{ $t('AbpIdentity.Permissions') }}
              </MenuItem>
              <MenuItem key="refresh" :icon="h(RedoOutlined)">
                {{ $t('AbpIdentity.OrganizationUnit:RefreshRoot') }}
              </MenuItem>
            </Menu>
          </template>
        </Dropdown>
      </template>
    </Tree>
  </Card>
  <OrganizationUnitEditModal @change="onOrganizationUnitChanged" />
  <OrganizationUnitPermissionModal />
</template>

<style scoped></style>
