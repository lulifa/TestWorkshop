import type { WorkshopRealtimeDashboardDto } from '../../types';

import { useRequest } from '../../hooks';

export function useWorkshopRealtimeDataApi() {
  const { cancel, request } = useRequest();

  function getDashboardApi(): Promise<WorkshopRealtimeDashboardDto> {
    return request<WorkshopRealtimeDashboardDto>(
      '/api/workshop/realtime-data/dashboard',
      {
        method: 'GET',
      },
    );
  }

  return {
    cancel,
    getDashboardApi,
  };
}
