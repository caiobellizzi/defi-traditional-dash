import { useQuery } from '@tanstack/react-query';
import { analyticsApi } from '../api/analytics';
import type { AnalyticsFilters } from '../types/analytics';

/**
 * Hook to fetch performance metrics
 */
export const usePerformance = (filters?: AnalyticsFilters) => {
  return useQuery({
    queryKey: ['analytics', 'performance', filters],
    queryFn: () => analyticsApi.getPerformance(filters),
    retry: false, // Don't retry if endpoint doesn't exist yet
  });
};

/**
 * Hook to fetch historical performance data
 */
export const useHistoricalPerformance = (filters?: AnalyticsFilters) => {
  return useQuery({
    queryKey: ['analytics', 'historical-performance', filters],
    queryFn: () => analyticsApi.getHistoricalPerformance(filters),
    retry: false,
  });
};

/**
 * Hook to fetch allocation drift analysis
 */
export const useAllocationDrift = (clientId?: string) => {
  return useQuery({
    queryKey: ['analytics', 'allocation-drift', clientId],
    queryFn: () => analyticsApi.getAllocationDrift(clientId),
    retry: false,
  });
};

/**
 * Hook to fetch asset type breakdown
 */
export const useAssetBreakdown = (clientId?: string) => {
  return useQuery({
    queryKey: ['analytics', 'asset-breakdown', clientId],
    queryFn: () => analyticsApi.getAssetBreakdown(clientId),
    retry: false,
  });
};

/**
 * Hook to fetch top performing assets
 */
export const useTopAssets = (limit: number = 10, clientId?: string) => {
  return useQuery({
    queryKey: ['analytics', 'top-assets', limit, clientId],
    queryFn: () => analyticsApi.getTopAssets(limit, clientId),
    retry: false,
  });
};
