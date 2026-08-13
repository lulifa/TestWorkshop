import type { AxiosResponse } from 'axios';

import type { RemoteServiceErrorInfo } from '../types';

// abp标准的报错处理
export function useErrorFormat(response: AxiosResponse) {
  // abp标准报错的话 请求头会包含这个特殊key
  const _defaultErrorHeaderKey: string = '_abperrorformat';
  const { data, headers } = response;

  // 是否请求错误
  function hasAbpError(): boolean {
    return headers[_defaultErrorHeaderKey] === 'true';
  }

  // 请求错误时抛出异常
  function throwIfAbpError(): void {
    if (!hasAbpError()) return;

    const errorJson = data.error as RemoteServiceErrorInfo;

    let message = errorJson.message;

    const errors = errorJson.validationErrors;

    message += errors?.map((v) => v.message).join('\n') ?? '';

    throw Object.assign({}, response, { message, response });
  }

  return { hasAbpError, throwIfAbpError };
}
