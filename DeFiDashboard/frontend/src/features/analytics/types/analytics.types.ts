/**
 * Analytics Types
 */

/**
 * Performance metrics for a client or overall portfolio
 */
export interface PerformanceMetrics {
  clientId?: string;
  clientName?: string;
  startDate: string;
  endDate: string;
  initialValueUsd: number;
  currentValueUsd: number;
  absoluteReturn: number;
  percentageReturn: number;
  highestValueUsd: number;
  lowestValueUsd: number;
  volatility: number;
  sharpeRatio: number | null;
}

/**
 * Time-series performance data point
 */
export interface PerformanceDataPoint {
  date: string;
  valueUsd: number;
  percentChange: number;
}

/**
 * Historical performance over time
 */
export interface HistoricalPerformance {
  clientId?: string;
  clientName?: string;
  dataPoints: PerformanceDataPoint[];
  period: 'day' | 'week' | 'month' | 'quarter' | 'year';
}

/**
 * Allocation drift for a specific allocation
 */
export interface AllocationDrift {
  allocationId: string;
  clientId: string;
  clientName: string;
  assetType: string;
  assetName: string;
  targetAllocationValue: number;
  targetAllocationType: string;
  currentValueUsd: number;
  targetValueUsd: number;
  driftAmount: number;
  driftPercentage: number;
  requiresRebalancing: boolean;
  rebalancingThreshold: number;
}

/**
 * Asset breakdown by type
 */
export interface AssetTypeBreakdown {
  assetType: string;
  totalValueUsd: number;
  percentage: number;
  assetCount: number;
}

/**
 * Top performing assets
 */
export interface TopAsset {
  assetId: string;
  assetName: string;
  assetType: string;
  currentValueUsd: number;
  returnPercentage: number;
  allocationCount: number;
}

/**
 * Filters for performance analytics
 */
export interface AnalyticsFilters {
  clientId?: string;
  startDate?: string;
  endDate?: string;
  period?: 'day' | 'week' | 'month' | 'quarter' | 'year';
}
