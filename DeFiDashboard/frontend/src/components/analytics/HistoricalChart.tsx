import { LineChart } from '../charts';
import { formatCurrency, formatDate } from '../../lib/formatters';
import type { HistoricalPerformance } from '../../types/analytics';

interface HistoricalChartProps {
  data: HistoricalPerformance;
  isLoading?: boolean;
}

/**
 * Historical Performance Chart Component
 * Shows portfolio value over time using a line chart
 */
export function HistoricalChart({ data, isLoading }: HistoricalChartProps) {
  if (isLoading) {
    return (
      <div className="h-[400px] flex items-center justify-center">
        <div className="animate-pulse text-gray-400 dark:text-gray-500">
          Loading chart data...
        </div>
      </div>
    );
  }

  if (!data || data.dataPoints.length === 0) {
    return (
      <div className="h-[400px] flex items-center justify-center">
        <p className="text-gray-500 dark:text-gray-400">
          No historical data available for the selected period.
        </p>
      </div>
    );
  }

  // Transform data for the chart
  const chartData = data.dataPoints.map((point) => ({
    date: point.date,
    value: point.valueUsd,
    change: point.percentChange,
  }));

  return (
    <div>
      {data.clientName && (
        <div className="mb-4">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
            {data.clientName} - Portfolio Performance
          </h3>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {data.period.charAt(0).toUpperCase() + data.period.slice(1)}ly view
          </p>
        </div>
      )}

      <LineChart
        data={chartData}
        lines={[
          {
            dataKey: 'value',
            name: 'Portfolio Value',
            color: '#3b82f6',
            strokeWidth: 2,
          },
        ]}
        xAxisKey="date"
        height={400}
        valueFormatter={(value) => formatCurrency(value)}
        xAxisFormatter={(value) => {
          const date = new Date(value);
          if (data.period === 'day') {
            return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
          } else if (data.period === 'week' || data.period === 'month') {
            return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
          } else {
            return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
          }
        }}
      />

      {/* Summary Stats */}
      <div className="grid grid-cols-3 gap-4 mt-6">
        <div className="text-center">
          <p className="text-xs text-gray-500 dark:text-gray-400">Data Points</p>
          <p className="text-lg font-semibold text-gray-900 dark:text-white">
            {data.dataPoints.length}
          </p>
        </div>
        <div className="text-center">
          <p className="text-xs text-gray-500 dark:text-gray-400">Period</p>
          <p className="text-lg font-semibold text-gray-900 dark:text-white">
            {data.period.charAt(0).toUpperCase() + data.period.slice(1)}
          </p>
        </div>
        <div className="text-center">
          <p className="text-xs text-gray-500 dark:text-gray-400">Total Change</p>
          <p
            className={`text-lg font-semibold ${
              data.dataPoints[data.dataPoints.length - 1]?.percentChange >= 0
                ? 'text-green-600 dark:text-green-400'
                : 'text-red-600 dark:text-red-400'
            }`}
          >
            {data.dataPoints[data.dataPoints.length - 1]?.percentChange >= 0 ? '+' : ''}
            {data.dataPoints[data.dataPoints.length - 1]?.percentChange.toFixed(2)}%
          </p>
        </div>
      </div>
    </div>
  );
}
