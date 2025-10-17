import { formatCurrency, formatPercentage } from '@/shared/lib/formatters';
import type { PerformanceMetrics as PerformanceMetricsType } from '../types/analytics.types';

interface PerformanceMetricsProps {
  metrics: PerformanceMetricsType;
  isLoading?: boolean;
}

/**
 * Performance Metrics Display Component
 * Shows key performance indicators with color-coded changes
 */
export function PerformanceMetrics({ metrics, isLoading }: PerformanceMetricsProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {[1, 2, 3, 4].map((i) => (
          <div
            key={i}
            className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 animate-pulse"
          >
            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-24 mb-2"></div>
            <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
          </div>
        ))}
      </div>
    );
  }

  const isPositiveReturn = metrics.percentageReturn >= 0;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {/* Current Value */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
        <p className="text-sm text-gray-500 dark:text-gray-400">Current Value</p>
        <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2">
          {formatCurrency(metrics.currentValueUsd)}
        </p>
        <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
          Initial: {formatCurrency(metrics.initialValueUsd)}
        </p>
      </div>

      {/* Absolute Return */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
        <p className="text-sm text-gray-500 dark:text-gray-400">Total Return</p>
        <p
          className={`text-3xl font-bold mt-2 ${
            isPositiveReturn
              ? 'text-green-600 dark:text-green-400'
              : 'text-red-600 dark:text-red-400'
          }`}
        >
          {isPositiveReturn ? '+' : ''}
          {formatCurrency(metrics.absoluteReturn)}
        </p>
        <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">Absolute return</p>
      </div>

      {/* Percentage Return */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
        <p className="text-sm text-gray-500 dark:text-gray-400">Return %</p>
        <p
          className={`text-3xl font-bold mt-2 ${
            isPositiveReturn
              ? 'text-green-600 dark:text-green-400'
              : 'text-red-600 dark:text-red-400'
          }`}
        >
          {isPositiveReturn ? '+' : ''}
          {formatPercentage(metrics.percentageReturn)}
        </p>
        <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">Percentage return</p>
      </div>

      {/* Volatility & Sharpe Ratio */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
        <p className="text-sm text-gray-500 dark:text-gray-400">Risk Metrics</p>
        <p className="text-xl font-bold text-gray-900 dark:text-white mt-2">
          {formatPercentage(metrics.volatility)}
        </p>
        <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">Volatility</p>
        {metrics.sharpeRatio !== null && (
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-2">
            Sharpe: {metrics.sharpeRatio.toFixed(2)}
          </p>
        )}
      </div>

      {/* High/Low */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors col-span-full lg:col-span-2">
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">Value Range</p>
        <div className="flex justify-between items-center">
          <div>
            <p className="text-xs text-gray-400 dark:text-gray-500">Highest</p>
            <p className="text-xl font-bold text-green-600 dark:text-green-400">
              {formatCurrency(metrics.highestValueUsd)}
            </p>
          </div>
          <div className="flex-1 mx-4">
            <div className="h-2 bg-gray-200 dark:bg-gray-700 rounded-full relative">
              <div
                className="h-2 bg-blue-500 rounded-full absolute left-0"
                style={{
                  width: `${
                    ((metrics.currentValueUsd - metrics.lowestValueUsd) /
                      (metrics.highestValueUsd - metrics.lowestValueUsd)) *
                    100
                  }%`,
                }}
              />
            </div>
          </div>
          <div className="text-right">
            <p className="text-xs text-gray-400 dark:text-gray-500">Lowest</p>
            <p className="text-xl font-bold text-red-600 dark:text-red-400">
              {formatCurrency(metrics.lowestValueUsd)}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
