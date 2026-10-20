import type { EntityDto, PagedAndSortedResultRequestDto } from '../..';

enum WorkshopOrderStatus {
  Cancelled = 3,
  Draft = 0,
  PendingReview = 2,
  ReviewApproved = 4,
  ReviewRejected = 5,
  Testing = 1,
}

enum WorkshopTestReportStatus {
  Failed = 3,
  Fetching = 1,
  NotReady = 0,
  Ready = 2,
}

enum WorkshopOrderAuditResult {
  Approved = 1,
  Rejected = 0,
}

interface WorkshopOrderDto extends EntityDto<string> {
  creationTime?: string;
  deviceCode?: string;
  deviceId: string;
  deviceName?: string;
  hasTestReport: boolean;
  lastModificationTime?: string;
  orderNo: string;
  projectSupervisorName?: string;
  projectSupervisorUserId: string;
  requiredCompletionTime: string;
  status: WorkshopOrderStatus;
  statusName?: string;
  testEndedAt?: string;
  testReportFetchedAt?: string;
  testReportFileCount: number;
  testReportStatus: WorkshopTestReportStatus;
  testStartedAt?: string;
}

interface WorkshopOrderCreateOrUpdateDto {
  deviceId: string;
  orderNo: string;
  projectSupervisorUserId: string;
  requiredCompletionTime: string;
}

interface WorkshopOrderProjectSupervisorDto {
  name: string;
  userId: string;
  userName: string;
}

type WorkshopOrderCreateDto = WorkshopOrderCreateOrUpdateDto;

type WorkshopOrderUpdateDto = WorkshopOrderCreateOrUpdateDto;

interface GetWorkshopOrderListInput extends PagedAndSortedResultRequestDto {
  deviceId?: string;
  endTime?: string;
  isPaged?: boolean;
  orderNo?: string;
  startTime?: string;
  status?: WorkshopOrderStatus;
}

interface WorkshopTelemetryGraphChannelDto {
  channelType: string;
  sampleCount: number;
  values: number[];
}

interface WorkshopTelemetryGraphGroupDto {
  channels: WorkshopTelemetryGraphChannelDto[];
  timestamp: string;
}

interface WorkshopOrderTelemetryGraphDto {
  deviceCode?: string;
  deviceName?: string;
  graphs: WorkshopTelemetryGraphGroupDto[];
  orderId: string;
  orderNo: string;
  testedDeviceCode?: string;
  testedDeviceName?: string;
}

interface WorkshopOrderAuditInput {
  maintenanceSuggestion?: string;
  result: WorkshopOrderAuditResult;
}

interface WorkshopOrderAuditRecordDto extends EntityDto<string> {
  auditedAt: string;
  auditorName?: string;
  auditorUserId: string;
  maintenanceSuggestion?: string;
  orderId: string;
  reportFileCount: number;
  result: WorkshopOrderAuditResult;
  resultName?: string;
}

interface WorkshopOrderReportFileDto {
  contentType?: string;
  creationTime: string;
  fileName: string;
  fileSize: number;
  hasPreview: boolean;
  key: string;
}

export {
  WorkshopOrderAuditResult,
  WorkshopOrderStatus,
  WorkshopTestReportStatus,
};

export type {
  GetWorkshopOrderListInput,
  WorkshopOrderAuditInput,
  WorkshopOrderAuditRecordDto,
  WorkshopOrderCreateDto,
  WorkshopOrderDto,
  WorkshopOrderProjectSupervisorDto,
  WorkshopOrderReportFileDto,
  WorkshopOrderTelemetryGraphDto,
  WorkshopOrderUpdateDto,
  WorkshopTelemetryGraphChannelDto,
  WorkshopTelemetryGraphGroupDto,
};
