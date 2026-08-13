import type { SlotsType } from 'vue';

import type { ExtendedVxeGridApi, VxeGridProps } from './types';

import { defineComponent, h, onBeforeUnmount } from 'vue';

import { useStore } from '@vben-core/shared/store';

import { VxeGridApi } from './api';
import VxeGrid from './use-vxe-grid.vue';

// 使用轻量插槽参数，避免展开 vxe 内部递归的 GridConstructor 类型
type GridSlotParams<T> = {
  row: T;
  [key: string]: any;
};

type GridSlots<T> = {
  [key: string]: ((params: GridSlotParams<T>) => any) | undefined;
};

export function useVbenVxeGrid<T extends Record<string, any> = any>(
  options: VxeGridProps<T>,
) {
  // const IS_REACTIVE = isReactive(options);
  const api = new VxeGridApi(options);
  const extendedApi: ExtendedVxeGridApi<T> = api as ExtendedVxeGridApi<T>;
  extendedApi.useStore = (selector) => {
    return useStore(api.store, selector);
  };

  const Grid = defineComponent(
    (props: VxeGridProps<T>, { attrs, slots }) => {
      onBeforeUnmount(() => {
        api.unmount();
      });
      api.setState({ ...props, ...attrs });
      return () => h(VxeGrid, { ...props, ...attrs, api: extendedApi }, slots);
    },
    {
      name: 'VbenVxeGrid',
      inheritAttrs: false,
      slots: Object as SlotsType<
        {
          // 表格标题
          'table-title': undefined;
          // 工具栏左侧部分
          'toolbar-actions': GridSlotParams<T>;
          // 工具栏右侧部分
          'toolbar-tools': GridSlotParams<T>;
        } & GridSlots<T>
      >,
    },
  );
  // Add reactivity support
  // if (IS_REACTIVE) {
  //   watch(
  //     () => options,
  //     () => {
  //       api.setState(options);
  //     },
  //     { immediate: true },
  //   );
  // }

  return [Grid, extendedApi] as const;
}

export type UseVbenVxeGrid = typeof useVbenVxeGrid;
