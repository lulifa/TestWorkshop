import type { DeviceType } from './workshopDevices';

interface WorkshopRealtimeDashboardDto {
  devices: WorkshopRealtimeDeviceDto[];
  generatedAt: string;
  onlineTimeoutSeconds: number;
  refreshAfterSeconds: number;
}

interface WorkshopRealtimeDeviceDto {
  dataTimestamp?: string;
  deviceCode: string;
  deviceId: string;
  deviceName: string;
  deviceType: DeviceType;
  deviceTypeName?: string;
  flow?: number;
  lastHeartbeatAt: string;
  onlineDurationSeconds: number;
  organizationUnitId: string;
  organizationUnitName?: string;
  pressure?: number;
  temperature?: number;
}

export type { WorkshopRealtimeDashboardDto, WorkshopRealtimeDeviceDto };
