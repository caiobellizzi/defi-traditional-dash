/**
 * Analytics Types
 */

export interface PerformanceMetrics {
  clientId?: string;
  totalValueUsd: number;
  periodStartValue: number;
  periodEndValue: number;
  absoluteReturn: number;
  percentReturn: number;
  period: string; // '1D', '1W', '1M', '3M', '6M', '1Y', 'ALL'
}

export interface AllocationDrift {
  clientId: string;
  clientName: string;
  targetAllocation: number;
  currentAllocation: number;
  drift: number;
  driftPercentage: number;
}
