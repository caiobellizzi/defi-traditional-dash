import { apiClient } from '@/shared/lib/api-client';
import type {
  PerformanceMetrics,
  HistoricalPerformance,
  AllocationDrift,
  AssetTypeBreakdown,
  TopAsset,
  AnalyticsFilters,
} from '../types/analytics.types';

/**
 * Analytics API Service
 * Note: These endpoints may not exist in the backend yet.
 * This is a placeholder for future implementation.
 */
export const analyticsApi = {
  /**
   * Get performance metrics for a client or overall portfolio
   */
  getPerformance: async (filters?: AnalyticsFilters): Promise<PerformanceMetrics> => {
    const response = await apiClient.get<PerformanceMetrics>('/api/analytics/performance', {
      params: {
        clientId: filters?.clientId,
        startDate: filters?.startDate,
        endDate: filters?.endDate,
      },
    });
    return response.data;
  },

  /**
   * Get historical performance data over time
   */
  getHistoricalPerformance: async (
    filters?: AnalyticsFilters
  ): Promise<HistoricalPerformance> => {
    const response = await apiClient.get<HistoricalPerformance>(
      '/api/analytics/performance/historical',
      {
        params: {
          clientId: filters?.clientId,
          startDate: filters?.startDate,
          endDate: filters?.endDate,
          period: filters?.period || 'day',
        },
      }
    );
    return response.data;
  },

  /**
   * Get allocation drift analysis
   */
  getAllocationDrift: async (clientId?: string): Promise<AllocationDrift[]> => {
    const response = await apiClient.get<AllocationDrift[]>('/api/analytics/allocation-drift', {
      params: { clientId },
    });
    return response.data;
  },

  /**
   * Get asset type breakdown
   */
  getAssetBreakdown: async (clientId?: string): Promise<AssetTypeBreakdown[]> => {
    const response = await apiClient.get<AssetTypeBreakdown[]>('/api/analytics/asset-breakdown', {
      params: { clientId },
    });
    return response.data;
  },

  /**
   * Get top performing assets
   */
  getTopAssets: async (limit: number = 10, clientId?: string): Promise<TopAsset[]> => {
    const response = await apiClient.get<TopAsset[]>('/api/analytics/top-assets', {
      params: { limit, clientId },
    });
    return response.data;
  },
};
