import { BarChart, type BarChartDataPoint } from '../charts';
import { formatCurrency } from '../../lib/formatters';
import type { ClientPortfolioDto } from '../../types/portfolio';

interface TopClientsChartProps {
  portfolios: ClientPortfolioDto[];
  limit?: number;
  isLoading?: boolean;
}

/**
 * Top Clients Chart Component
 * Shows top clients by portfolio value using a bar chart
 */
export function TopClientsChart({ portfolios, limit = 10, isLoading }: TopClientsChartProps) {
  if (isLoading) {
    return (
      <div className="h-[400px] flex items-center justify-center">
        <div className="animate-pulse text-gray-400 dark:text-gray-500">Loading chart...</div>
      </div>
    );
  }

  if (!portfolios || portfolios.length === 0) {
    return (
      <div className="h-[400px] flex items-center justify-center">
        <p className="text-gray-500 dark:text-gray-400">No client data available</p>
      </div>
    );
  }

  // Sort by total value and take top N
  const topClients = [...portfolios]
    .sort((a, b) => b.totalValueUsd - a.totalValueUsd)
    .slice(0, limit);

  const data: BarChartDataPoint[] = topClients.map((portfolio) => ({
    name: portfolio.clientName || 'Unknown',
    crypto: portfolio.cryptoValueUsd,
    traditional: portfolio.traditionalValueUsd,
    total: portfolio.totalValueUsd,
  }));

  return (
    <div>
      <BarChart
        data={data}
        bars={[
          { dataKey: 'crypto', name: 'Crypto', color: '#3b82f6' },
          { dataKey: 'traditional', name: 'Traditional', color: '#10b981' },
        ]}
        xAxisKey="name"
        height={400}
        stacked={true}
        valueFormatter={formatCurrency}
        showLegend={true}
      />

      {/* Summary */}
      <div className="mt-4 text-center">
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Showing top {topClients.length} clients by portfolio value
        </p>
      </div>
    </div>
  );
}
