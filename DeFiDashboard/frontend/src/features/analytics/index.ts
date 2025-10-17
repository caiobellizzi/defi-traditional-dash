/**
 * Analytics Feature - Barrel Export
 */

// Types
export * from './types/analytics.types';

// API
export { analyticsApi } from './api/analytics.api';

// Hooks
export {
  usePerformance,
  useHistoricalPerformance,
  useAllocationDrift,
  useAssetBreakdown,
  useTopAssets,
} from './hooks/useAnalytics';

// Components
export { PerformanceMetrics } from './components/PerformanceMetrics';
export { AllocationDriftTable } from './components/AllocationDriftTable';
export { HistoricalChart } from './components/HistoricalChart';

// Pages
export { default as AnalyticsPage } from './pages/AnalyticsPage';
