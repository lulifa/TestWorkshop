import type { EntityDto, PagedAndSortedResultRequestDto } from '../..';

enum WorkshopTelemetryStatus {
  Failed = 3,
  Pending = 0,
  Processing = 1,
  Success = 2,
}

enum TelemetryMetricType {
  Flow = 3,
  Pressure = 1,
  Temp = 2,
  Vibration = 4,
}

interface WorkshopTelemetryTaskDto extends EntityDto<number> {
  createdAt: string;
  error?: string;
  expiresAt: string;
  fileName: string;
  fileObjectId: string;
  fileSize: number;
  nextRetryTime?: string;
  processedAt?: string;
  recordCount?: number;
  retryCount: number;
  status: WorkshopTelemetryStatus;
  statusName: string;
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

interface WorkshopTelemetryMetricTypeDto {
  displayName: string;
  name: string;
  value: TelemetryMetricType;
}

export { WorkshopTelemetryStatus };
export { TelemetryMetricType };

export type {
  WorkshopTelemetryMetricTypeDto,
  WorkshopTelemetryStatisticsDto,
  WorkshopTelemetryTaskDto,
  WorkshopTelemetryTaskListInput,
};
