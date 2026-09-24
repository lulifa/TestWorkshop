import type { EntityDto, PagedAndSortedResultRequestDto } from '../..';

enum WorkshopTelemetryStatus {
  Failed = 3,
  Pending = 0,
  Processing = 1,
  Success = 2,
}

interface WorkshopTelemetryTaskDto extends EntityDto<number> {
  channelType?: string;
  createdAt: string;
  deviceCode?: string;
  endTime?: string;
  endTime2?: string;
  error?: string;
  expiresAt: string;
  extraProperties?: Record<string, any>;
  fileName: string;
  fileObjectId: string;
  fileSize: number;
  fileTime?: string;
  nextRetryTime?: string;
  pilotSN?: string;
  processedAt?: string;
  recordCount?: number;
  retryCount: number;
  shipName?: string;
  startTime?: string;
  startTime2?: string;
  status: WorkshopTelemetryStatus;
  statusName: string;
  testBy?: string;
  testDate?: string;
  testedDeviceCode?: string;
  testedDeviceName?: string;
}

interface WorkshopTelemetryTaskListInput extends PagedAndSortedResultRequestDto {
  endTime?: string;
  fileName?: string;
  isPaged?: boolean;
  startTime?: string;
  status?: number;
}

interface WorkshopTelemetryStatisticsDto {
  failedCount: number;
  pendingCount: number;
  processingCount: number;
  successCount: number;
  totalFiles: number;
  totalRecords: number;
  totalSize: number;
  totalSizeMB: number;
}

interface WorkshopTelemetryFileInput {
  channelType: string;
  currentFile?: string;
  deviceCode: string;
  fileCount?: number;
  fileTime: string;
  group?: string;
  ifSource?: string;
  source?: string;
  testInfo: WorkshopTelemetryTestInfoInput;
}

interface WorkshopTelemetryTestInfoInput {
  endTime?: string;
  endTime2?: string;
  pilotSN?: string;
  shipName?: string;
  startTime?: string;
  startTime2?: string;
  testBy?: string;
  testDate?: string;
  testedDeviceCode?: string;
  testedDeviceName?: string;
}

export { WorkshopTelemetryStatus };

export type {
  WorkshopTelemetryFileInput,
  WorkshopTelemetryStatisticsDto,
  WorkshopTelemetryTaskDto,
  WorkshopTelemetryTaskListInput,
  WorkshopTelemetryTestInfoInput,
};
