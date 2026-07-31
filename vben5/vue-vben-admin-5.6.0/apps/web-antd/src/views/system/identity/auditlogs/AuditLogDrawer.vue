<script setup lang="ts">
import type { Action, AuditLogDto } from '@abp/core';
import type { VxeGridProps } from 'vxe-table';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';

import {
  CodeEditor,
  formatToDateTime,
  MODE,
  useAuditlogs,
  useAuditLogsApi,
} from '@abp/core';
import { Descriptions, Tabs, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

import EntityChangeTable from './EntityChangeTable.vue';

defineOptions({
  name: 'AuditLogDrawer',
});

const TabPane = Tabs.TabPane;
const DescriptionsItem = Descriptions.Item;

const activedTab = ref('basic');
const auditLogModel = ref<AuditLogDto>({} as AuditLogDto);

const { getApi } = useAuditLogsApi();
const { getHttpMethodColor, getHttpStatusCodeColor } = useAuditlogs();
const [Drawer, drawerApi] = useVbenDrawer({
  class: 'w-auto',
  onCancel() {
    drawerApi.close();
  },
  onConfirm: async () => {
    drawerApi.close();
  },
  onOpenChange: async (isOpen: boolean) => {
    if (isOpen) {
      try {
        auditLogModel.value = {} as AuditLogDto;
        drawerApi.setState({ loading: true });
        const dto = drawerApi.getData<AuditLogDto>();
        await onGet(dto.id);
      } finally {
        drawerApi.setState({ loading: false });
      }
    }
  },
  title: $t('TestWorkshop.DisplayName:AuditLog'),
});
/** 调用方法表格配置 */
const actionsGridOptions: VxeGridProps<Action> = {
  border: true,
  columns: [
    {
      align: 'left',
      field: 'parameters',
      slots: {
        content: 'parameters',
      },
      type: 'expand',
    },
    {
      align: 'left',
      field: 'serviceName',
      sortable: true,
      title: $t('AbpAuditLogging.ServiceName'),
      width: 'auto',
    },
    {
      align: 'left',
      field: 'methodName',
      sortable: true,
      title: $t('AbpAuditLogging.MethodName'),
      width: 150,
    },
    {
      align: 'left',
      field: 'executionTime',
      formatter: ({ cellValue }) => {
        return cellValue ? formatToDateTime(cellValue) : cellValue;
      },
      sortable: true,
      title: $t('AbpAuditLogging.ExecutionTime'),
      width: 200,
    },
    {
      align: 'left',
      field: 'executionDuration',
      sortable: true,
      title: $t('AbpAuditLogging.ExecutionDuration'),
      width: 150,
    },
  ],
  expandConfig: {
    accordion: true,
    padding: true,
    trigger: 'row',
    height: 300,
  },
  exportConfig: {},
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    ajax: {
      query: () => {
        return Promise.resolve(auditLogModel.value.actions);
      },
    },
    response: {
      list: ({ data }) => {
        return data;
      },
    },
  },
  toolbarConfig: {
    enabled: false,
  },
};
/** 调用方法表格 */
const [ActionsGrid] = useVbenVxeGrid({
  gridOptions: actionsGridOptions,
});
/** 查询审计日志 */
async function onGet(id: string) {
  const dto = await getApi(id);
  auditLogModel.value = dto;
}
</script>

<template>
  <Drawer>
    <div style="width: 1000px">
      <Tabs v-model="activedTab">
        <TabPane key="basic" :tab="$t('TestWorkshop.DisplayName:Operation')">
          <Descriptions :colon="false" :column="2" bordered size="small">
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:ApplicationName')"
            >
              {{ auditLogModel.applicationName }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:ExecutionTime')"
            >
              {{ formatToDateTime(auditLogModel.executionTime) }}
            </DescriptionsItem>
            <DescriptionsItem :label="$t('TestWorkshop.DisplayName:UserName')">
              {{ auditLogModel.userName }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:TenantName')"
            >
              <span v-if="auditLogModel.tenantId">
                {{ auditLogModel.tenantId }}/{{ auditLogModel.tenantName }}
              </span>
            </DescriptionsItem>
            <template v-if="auditLogModel.impersonatorUserName">
              <DescriptionsItem
                :label="$t('TestWorkshop.DisplayName:ImpersonatorTenantId')"
              >
                <span v-if="auditLogModel.impersonatorTenantId">
                  {{ auditLogModel.impersonatorTenantId }}/{{
                    auditLogModel.impersonatorTenantName
                  }}
                </span>
              </DescriptionsItem>
              <DescriptionsItem
                :label="$t('TestWorkshop.DisplayName:ImpersonatorUserId')"
              >
                {{ auditLogModel.impersonatorUserId }}/{{
                  auditLogModel.impersonatorUserName
                }}
              </DescriptionsItem>
            </template>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:RequestUrl')"
              :span="2"
            >
              {{ auditLogModel.url }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:HttpMethod')"
              :span="2"
            >
              <Tag :color="getHttpMethodColor(auditLogModel.httpMethod)">
                {{ auditLogModel.httpMethod }}
              </Tag>
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:HttpStatusCode')"
            >
              <Tag
                :color="getHttpStatusCodeColor(auditLogModel.httpStatusCode)"
              >
                {{ auditLogModel.httpStatusCode }}
              </Tag>
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:ExecutionDuration')"
            >
              {{ auditLogModel.executionDuration }}
            </DescriptionsItem>
            <DescriptionsItem :label="$t('TestWorkshop.DisplayName:ClientId')">
              {{ auditLogModel.clientId }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:ClientIpAddress')"
            >
              {{ auditLogModel.clientIpAddress }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:ClientName')"
            >
              {{ auditLogModel.clientName }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:CorrelationId')"
            >
              {{ auditLogModel.correlationId }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:BrowserInfo')"
              :label-style="{ width: '110px' }"
              :span="2"
            >
              {{ auditLogModel.browserInfo }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:Comments')"
              :span="2"
            >
              {{ auditLogModel.comments }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:Exception')"
              :span="2"
            >
              {{ auditLogModel.exceptions }}
            </DescriptionsItem>
            <DescriptionsItem
              :label="$t('TestWorkshop.DisplayName:Additional')"
              :span="2"
            >
              {{ auditLogModel.extraProperties }}
            </DescriptionsItem>
          </Descriptions>
        </TabPane>
        <TabPane
          v-if="auditLogModel.actions?.length"
          key="opera"
          :tab="`${$t('TestWorkshop.DisplayName:InvokeMethod')}(${auditLogModel.actions?.length})`"
        >
          <ActionsGrid>
            <template #parameters="{ row }">
              <Descriptions :colon="false" :column="1" bordered size="small">
                <DescriptionsItem
                  :label="$t('TestWorkshop.DisplayName:Parameters')"
                >
                  <CodeEditor
                    :mode="MODE.JSON"
                    :value="row.parameters"
                    readonly
                  />
                </DescriptionsItem>
                <DescriptionsItem
                  :label="$t('TestWorkshop.DisplayName:Additional')"
                >
                  <CodeEditor
                    :mode="MODE.JSON"
                    :value="row.extraProperties"
                    readonly
                  />
                </DescriptionsItem>
              </Descriptions>
            </template>
          </ActionsGrid>
        </TabPane>
        <TabPane
          v-if="auditLogModel.entityChanges?.length"
          key="changes"
          :tab="`${$t('TestWorkshop.DisplayName:EntitiesChanged')}(${auditLogModel.entityChanges?.length})`"
        >
          <EntityChangeTable :data="auditLogModel.entityChanges" />
        </TabPane>
      </Tabs>
    </div>
  </Drawer>
</template>

<style scoped></style>
