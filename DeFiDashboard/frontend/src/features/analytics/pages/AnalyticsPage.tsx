import { useState } from 'react';
import { PageLayout } from '@/shared/components/layout/PageLayout';
import { PerformanceMetrics } from '../components/PerformanceMetrics';
import { AllocationDriftTable } from '../components/AllocationDriftTable';
import { HistoricalChart } from '../components/HistoricalChart';
import {
  usePerformance,
  useHistoricalPerformance,
  useAllocationDrift,
} from '../hooks/useAnalytics';
import type { AnalyticsFilters } from '../types/analytics.types';

/**
 * Analytics Page - Performance analysis and allocation drift
 */
export default function AnalyticsPage() {
  const [filters, setFilters] = useState<AnalyticsFilters>({
    period: 'month',
  });

  const { data: performanceData, isLoading: perfLoading, error: perfError } = usePerformance(filters);
  const {
    data: historicalData,
    isLoading: histLoading,
    error: histError,
  } = useHistoricalPerformance(filters);
  const { data: driftData, isLoading: driftLoading, error: driftError } = useAllocationDrift();

  const handlePeriodChange = (period: 'day' | 'week' | 'month' | 'quarter' | 'year') => {
    setFilters((prev) => ({ ...prev, period }));
  };

  return (
    <PageLayout>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Analytics</h1>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          Performance metrics and portfolio analysis
        </p>
      </div>

      {/* Period Filter */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-card dark:shadow-card-dark mb-6 transition-colors">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Period:</span>
          <div className="flex gap-2">
            {(['day', 'week', 'month', 'quarter', 'year'] as const).map((period) => (
              <button
                key={period}
                onClick={() => handlePeriodChange(period)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                  filters.period === period
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
                }`}
              >
                {period.charAt(0).toUpperCase() + period.slice(1)}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Performance Metrics */}
      <div className="mb-8">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
          Performance Overview
        </h2>
        {perfError ? (
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-12 text-center shadow-card dark:shadow-card-dark transition-colors">
            <p className="text-amber-600 dark:text-amber-400">
              Performance metrics are not available yet. This feature requires backend implementation.
            </p>
          </div>
        ) : performanceData ? (
          <PerformanceMetrics metrics={performanceData} isLoading={perfLoading} />
        ) : (
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-12 text-center shadow-card dark:shadow-card-dark transition-colors">
            <div className="animate-pulse text-gray-400 dark:text-gray-500">
              Loading performance metrics...
            </div>
          </div>
        )}
      </div>

      {/* Historical Performance Chart */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-card dark:shadow-card-dark overflow-hidden mb-8 transition-colors">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
            Historical Performance
          </h2>
        </div>
        <div className="p-6">
          {histError ? (
            <div className="p-12 text-center">
              <p className="text-amber-600 dark:text-amber-400">
                Historical performance data is not available yet. This feature requires backend
                implementation.
              </p>
            </div>
          ) : historicalData ? (
            <HistoricalChart data={historicalData} isLoading={histLoading} />
          ) : (
            <div className="h-[400px] flex items-center justify-center">
              <div className="animate-pulse text-gray-400 dark:text-gray-500">
                Loading chart data...
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Allocation Drift Analysis */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-card dark:shadow-card-dark overflow-hidden transition-colors">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
            Allocation Drift Analysis
          </h2>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Allocations that have drifted from their target values
          </p>
        </div>
        {driftError ? (
          <div className="p-12 text-center">
            <p className="text-amber-600 dark:text-amber-400">
              Allocation drift analysis is not available yet. This feature requires backend
              implementation.
            </p>
          </div>
        ) : driftData ? (
          <AllocationDriftTable drifts={driftData} isLoading={driftLoading} />
        ) : (
          <div className="p-12 text-center">
            <div className="animate-pulse text-gray-400 dark:text-gray-500">
              Loading drift analysis...
            </div>
          </div>
        )}
      </div>
    </PageLayout>
  );
}
