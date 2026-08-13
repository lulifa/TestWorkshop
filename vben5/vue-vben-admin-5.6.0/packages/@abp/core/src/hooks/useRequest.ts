import type { AxiosRequestConfig } from 'axios';

import { requestClient } from '../index';

export function useRequest() {
  const controllers = new Set<AbortController>();

  function request<T>(url: string, config: AxiosRequestConfig): Promise<T> {
    const controller = new AbortController();
    controllers.add(controller);

    return requestClient
      .request<T>(url, {
        ...config,
        signal: controller.signal,
      })
      .finally(() => {
        controllers.delete(controller);
      });
  }

  function cancel(message?: string) {
    controllers.forEach((controller) => controller.abort(message));
    controllers.clear();
  }

  return { cancel, request };
}
