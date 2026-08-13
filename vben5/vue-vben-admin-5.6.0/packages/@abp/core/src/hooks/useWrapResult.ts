import type { AxiosResponse } from 'axios';

import { useErrorFormat } from './useErrorFormat';

// abp结果包装处理
export function useWrapperResult(response: AxiosResponse) {
  // abp组装的话 请求头会包含这个特殊key
  const _defaultWrapperHeaderKey = '_abpwrapresult';
  const { hasAbpError, throwIfAbpError } = useErrorFormat(response);
  const { data, headers } = response;

  // 是否已包装结果
  function hasWrapResult(): boolean {
    return headers[_defaultWrapperHeaderKey] === 'true' || hasAbpError();
  }

  // 获取包装结果
  function getData(): any {
    throwIfError();
    return data.result;
  }

  /** 如果请求错误,抛出异常 */
  function throwIfError() {
    // 如果是abp标准错误，抛出
    throwIfAbpError();
    const { code, details, message } = data;
    // 如果业务失败，抛出异常
    const hasSuccess = data && Reflect.has(data, 'code') && code === '0';
    if (!hasSuccess) {
      const content = details || message;
      throw Object.assign({}, response, {
        response: {
          ...response,
          data: {
            ...response.data,
            message: content,
          },
        },
      });
    }
  }

  return {
    getData,
    hasWrapResult,
  };
}
