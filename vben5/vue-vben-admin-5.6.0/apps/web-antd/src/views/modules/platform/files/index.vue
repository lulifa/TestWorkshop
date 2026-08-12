<script lang="ts" setup>
import type { FileObjectDto } from '@abp/core';

import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';

import { h } from 'vue';

import { Page } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { formatToDateTime, useFileApi } from '@abp/core';
import {
  CloudUploadOutlined,
  DeleteOutlined,
  EyeOutlined,
  FileAddOutlined,
} from '@ant-design/icons-vue';
import { Button, message, Modal, Space } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useFileManager } from '#/components/file-manager';

defineOptions({
  name: 'FileTable',
});

const { deleteApi, getPagedListApi } = useFileApi();

const formOptions: VbenFormProps = {
  collapsed: true,
  collapsedRows: 2,
  commonConfig: {
    colon: true,
    componentProps: {
      class: 'w-full',
    },
  },
  fieldMappingTime: [
    [
      'uploadTime',
      ['startTime', 'endTime'],
      (value) => formatToDateTime(value),
    ],
  ],
  schema: [
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'keyword',
      formItemClass: 'col-span-2 items-baseline',
      label: '关键字',
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'ownerType',
      label: '业务类型',
    },
    {
      component: 'Input',
      componentProps: {
        allowClear: true,
      },
      fieldName: 'ownerId',
      label: '业务ID',
    },
    {
      component: 'RangePicker',
      componentProps: {
        showTime: true,
      },
      fieldName: 'uploadTime',
      formItemClass: 'col-span-2 items-baseline',
      label: '上传时间',
    },
  ],
  showCollapseButton: true,
  submitOnEnter: true,
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps<FileObjectDto> = {
  columns: [
    {
      align: 'center',
      fixed: 'left',
      type: 'seq',
      width: 80,
    },
    {
      align: 'left',
      field: 'fileName',
      fixed: 'left',
      minWidth: 220,
      title: '文件名',
    },
    {
      align: 'left',
      field: 'contentType',
      minWidth: 160,
      title: '文件类型',
    },
    {
      align: 'left',
      field: 'fileSizeText',
      minWidth: 100,
      title: '文件大小',
    },
    {
      align: 'left',
      field: 'ownerType',
      minWidth: 110,
      title: '业务类型',
    },
    {
      align: 'left',
      field: 'ownerId',
      minWidth: 280,
      title: '业务ID',
    },
    {
      align: 'left',
      field: 'creationTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : cellValue;
      },
      minWidth: 180,
      title: '上传时间',
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
      query: async ({ page }, formValues) => {
        return await getPagedListApi({
          isPaged: true,
          maxResultCount: page.pageSize,
          skipCount: (page.currentPage - 1) * page.pageSize,
          ...formValues,
        });
      },
      queryAll: async () => {
        const formValues = await gridApi.formApi.getValues();
        return await getPagedListApi({
          isPaged: false,
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

const gridEvents: VxeGridListeners<FileObjectDto> = {};

const [Grid, gridApi] = useVbenVxeGrid<FileObjectDto>({
  formOptions,
  gridEvents,
  gridOptions,
});

const { PreviewModal, UploadModal, openFilePreview, openFileUpload } =
  useFileManager();

async function onCreate(isMultiple: boolean) {
  const { ownerId, ownerType } = await gridApi.formApi.getValues();
  openFileUpload({ multiple: isMultiple, ownerId, ownerType });
}

function onPreview(row: FileObjectDto) {
  openFilePreview(row);
}

function onDelete(row: FileObjectDto) {
  Modal.confirm({
    afterClose: () => {
      gridApi.setLoading(false);
    },
    centered: true,
    content: `${$t('AbpUi.ItemWillBeDeletedMessage')}`,
    onOk: async () => {
      try {
        gridApi.setLoading(true);
        await deleteApi(row.id);
        message.success($t('AbpUi.DeletedSuccessfully'));
        gridApi.query();
      } finally {
        gridApi.setLoading(false);
      }
    },
    title: $t('AbpUi.AreYouSure'),
  });
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('TestWorkshop.DisplayName:File')">
      <template #toolbar-tools>
        <Space :size="8">
          <Button
            :icon="h(FileAddOutlined)"
            type="primary"
            @click="onCreate(false)"
          >
            单文件上传
          </Button>
          <Button
            :icon="h(CloudUploadOutlined)"
            type="primary"
            @click="onCreate(true)"
          >
            多文件上传
          </Button>
        </Space>
      </template>
      <template #action="{ row }">
        <div class="flex flex-row justify-center">
          <Space class="whitespace-nowrap" :size="4">
            <Button
              :icon="h(EyeOutlined)"
              size="small"
              type="link"
              @click="onPreview(row)"
            >
              预览
            </Button>
            <Button
              :icon="h(DeleteOutlined)"
              danger
              size="small"
              type="link"
              @click="onDelete(row)"
            >
              {{ $t('AbpUi.Delete') }}
            </Button>
          </Space>
        </div>
      </template>
    </Grid>

    <UploadModal @change="() => gridApi.query()" />
    <PreviewModal />
  </Page>
</template>

<style scoped></style>
