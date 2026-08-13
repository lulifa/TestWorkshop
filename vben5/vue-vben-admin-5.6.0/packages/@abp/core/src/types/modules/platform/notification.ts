import type { PagedAndSortedResultRequestDto } from '../..';

enum NotificationMessageType {
  BroadCast = 10,
  Common = 20,
}

enum NotificationMessageLevel {
  Error = 30,
  Information = 20,
  Warning = 10,
}

interface NotificationCoreInput {
  id: string;
}

interface NotificationDeleteInput extends NotificationCoreInput {
  receiverUserId?: string;
}

interface NotificationInput extends PagedAndSortedResultRequestDto {
  content?: string;
  endReadTime?: string;
  isPaged?: boolean;
  messageLevel?: NotificationMessageLevel;
  messageType?: NotificationMessageType;
  read?: boolean;
  receiverUserId?: string;
  receiverUserName?: string;
  senderUserId?: string;
  senderUserName?: string;
  startReadTime?: string;
  title?: string;
}

interface NotificationOutput {
  content: string;
  creationTime: string;
  id: string;
  messageLevel: NotificationMessageLevel;
  messageType: NotificationMessageType;
  read: boolean;
  readTime?: string;
  receiveUserId?: string;
  receiveUserName?: string;
  senderUserId: string;
  senderUserName: string;
  tenantId?: string;
  title: string;
}

interface NotificationSubscriptionInput extends PagedAndSortedResultRequestDto {
  endReadTime?: string;
  isPaged?: boolean;
  notificationId?: string;
  read?: boolean;
  receiverUserId?: string;
  receiverUserName?: string;
  startReadTime?: string;
}

interface NotificationSubscriptionOutput {
  content: string;
  creationTime?: string;
  id: string;
  messageLevel: NotificationMessageLevel;
  messageType: NotificationMessageType;
  notificationId: string;
  read: boolean;
  readTime: string;
  receiveUserId: string;
  receiveUserName: string;
  senderUserId: string;
  senderUserName: string;
  tenantId?: string;
  title: string;
}

interface SendBroadCastMessageInput {
  content: string;
  messageLevel: NotificationMessageLevel;
  title: string;
}

interface SendCommonMessageInput {
  content: string;
  messageLevel: NotificationMessageLevel;
  receiveUserId: string;
  receiveUserName: string;
  title: string;
}

interface SetBatchReadInput {
  ids: string[];
}

export { NotificationMessageLevel, NotificationMessageType };

export type {
  NotificationCoreInput,
  NotificationDeleteInput,
  NotificationInput,
  NotificationOutput,
  NotificationSubscriptionInput,
  NotificationSubscriptionOutput,
  SendBroadCastMessageInput,
  SendCommonMessageInput,
  SetBatchReadInput,
};
