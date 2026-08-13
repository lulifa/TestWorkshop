import type { PagedResultDto } from '@abp/core';

import type {
  NotificationCoreInput,
  NotificationDeleteInput,
  NotificationInput,
  NotificationOutput,
  NotificationSubscriptionInput,
  NotificationSubscriptionOutput,
  SendBroadCastMessageInput,
  SendCommonMessageInput,
  SetBatchReadInput,
} from '../../types';

import { useRequest } from '../../hooks';

export function useNotificationsApi() {
  const { cancel, request } = useRequest();

  function sendCommonMessageApi(input: SendCommonMessageInput): Promise<void> {
    return request('/api/platform/notification/send-common', {
      data: input,
      method: 'POST',
    });
  }

  function sendBroadCastMessageApi(
    input: SendBroadCastMessageInput,
  ): Promise<void> {
    return request('/api/platform/notification/send-broadcast', {
      data: input,
      method: 'POST',
    });
  }

  function setReadApi(input: NotificationCoreInput): Promise<void> {
    return request('/api/platform/notification/set-read', {
      data: input,
      method: 'PUT',
    });
  }

  function setBatchReadApi(input: SetBatchReadInput): Promise<void> {
    return request('/api/platform/notification/set-batchread', {
      data: input,
      method: 'PUT',
    });
  }

  function deleteApi(input: NotificationDeleteInput): Promise<void> {
    return request('/api/platform/notification/delete', {
      data: input,
      method: 'DELETE',
    });
  }

  function getMyNotificationListApi(
    input?: NotificationInput,
  ): Promise<PagedResultDto<NotificationOutput>> {
    return request<PagedResultDto<NotificationOutput>>(
      '/api/platform/notification/my-notification',
      {
        method: 'GET',
        params: input,
      },
    );
  }

  function getNotificationListApi(
    input?: NotificationInput,
  ): Promise<PagedResultDto<NotificationOutput>> {
    return request<PagedResultDto<NotificationOutput>>(
      '/api/platform/notification/notification',
      {
        method: 'GET',
        params: input,
      },
    );
  }

  function getSubscriptionListApi(
    input?: NotificationSubscriptionInput,
  ): Promise<PagedResultDto<NotificationSubscriptionOutput>> {
    return request<PagedResultDto<NotificationSubscriptionOutput>>(
      '/api/platform/notification/subscription',
      {
        method: 'GET',
        params: input,
      },
    );
  }

  function setSubscriptionReadApi(input: NotificationCoreInput): Promise<void> {
    return request('/api/platform/notification/subscription/set-read', {
      data: input,
      method: 'PUT',
    });
  }

  function setSubscriptionBatchReadApi(
    input: SetBatchReadInput,
  ): Promise<void> {
    return request('/api/platform/notification/subscription/set-batchread', {
      data: input,
      method: 'PUT',
    });
  }

  function deleteSubscriptionApi(input: NotificationCoreInput): Promise<void> {
    return request('/api/platform/notification/subscription/delete', {
      data: input,
      method: 'DELETE',
    });
  }

  return {
    cancel,
    deleteApi,
    deleteSubscriptionApi,
    getMyNotificationListApi,
    getNotificationListApi,
    getSubscriptionListApi,
    sendBroadCastMessageApi,
    sendCommonMessageApi,
    setBatchReadApi,
    setSubscriptionBatchReadApi,
    setSubscriptionReadApi,
    setReadApi,
  };
}
