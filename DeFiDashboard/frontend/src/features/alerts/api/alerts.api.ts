import { apiClient } from '@/shared/lib/api-client';
import type { AlertDto, AlertsSummary, AlertFilters } from '../types/alert.types';

/**
 * Alerts API Service
 * Note: These endpoints may not exist in the backend yet.
 * This is a placeholder for future implementation.
 */
export const alertsApi = {
  /**
   * Get all alerts with optional filtering
   */
  getAll: async (filters?: AlertFilters): Promise<AlertDto[]> => {
    const response = await apiClient.get<AlertDto[]>('/api/alerts', {
      params: {
        status: filters?.status,
        severity: filters?.severity,
        alertType: filters?.alertType,
        fromDate: filters?.fromDate,
        toDate: filters?.toDate,
      },
    });
    return response.data;
  },

  /**
   * Get alert by ID
   */
  getById: async (id: string): Promise<AlertDto> => {
    const response = await apiClient.get<AlertDto>(`/api/alerts/${id}`);
    return response.data;
  },

  /**
   * Get alerts summary for dashboard
   */
  getSummary: async (): Promise<AlertsSummary> => {
    const response = await apiClient.get<AlertsSummary>('/api/alerts/summary');
    return response.data;
  },

  /**
   * Acknowledge an alert
   */
  acknowledge: async (id: string): Promise<void> => {
    await apiClient.post(`/api/alerts/${id}/acknowledge`);
  },

  /**
   * Resolve an alert
   */
  resolve: async (id: string, notes?: string): Promise<void> => {
    await apiClient.post(`/api/alerts/${id}/resolve`, { notes });
  },

  /**
   * Dismiss an alert
   */
  dismiss: async (id: string): Promise<void> => {
    await apiClient.post(`/api/alerts/${id}/dismiss`);
  },
};
