import { PieChart, type PieChartDataPoint } from '../charts';
import { formatCurrency } from '../../lib/formatters';
import type { ClientPortfolioDto } from '../../types/portfolio';

interface AssetBreakdownChartProps {
  portfolios: ClientPortfolioDto[];
  isLoading?: boolean;
}

/**
 * Asset Breakdown Chart Component
 * Shows distribution of assets across crypto and traditional finance
 */
export function AssetBreakdownChart({ portfolios, isLoading }: AssetBreakdownChartProps) {
  if (isLoading) {
    return (
      <div className="h-[300px] flex items-center justify-center">
        <div className="animate-pulse text-gray-400 dark:text-gray-500">Loading chart...</div>
      </div>
    );
  }

  if (!portfolios || portfolios.length === 0) {
    return (
      <div className="h-[300px] flex items-center justify-center">
        <p className="text-gray-500 dark:text-gray-400">No portfolio data available</p>
      </div>
    );
  }

  // Calculate totals
  const totalCrypto = portfolios.reduce((sum, p) => sum + p.cryptoValueUsd, 0);
  const totalTradFi = portfolios.reduce((sum, p) => sum + p.traditionalValueUsd, 0);

  const data: PieChartDataPoint[] = [
    {
      name: 'Crypto Assets',
      value: totalCrypto,
      color: '#3b82f6', // blue-500
    },
    {
      name: 'Traditional Assets',
      value: totalTradFi,
      color: '#10b981', // green-500
    },
  ].filter((item) => item.value > 0);

  if (data.length === 0) {
    return (
      <div className="h-[300px] flex items-center justify-center">
        <p className="text-gray-500 dark:text-gray-400">No assets to display</p>
      </div>
    );
  }

  return (
    <div>
      <PieChart data={data} height={300} valueFormatter={formatCurrency} showLegend={true} />

      {/* Summary Stats */}
      <div className="grid grid-cols-2 gap-4 mt-6">
        <div className="text-center p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
          <p className="text-sm text-blue-600 dark:text-blue-400 font-medium">Crypto</p>
          <p className="text-lg font-bold text-blue-700 dark:text-blue-300 mt-1">
            {formatCurrency(totalCrypto)}
          </p>
          <p className="text-xs text-blue-600 dark:text-blue-400 mt-1">
            {totalCrypto + totalTradFi > 0
              ? `${((totalCrypto / (totalCrypto + totalTradFi)) * 100).toFixed(1)}%`
              : '0%'}
          </p>
        </div>
        <div className="text-center p-4 bg-green-50 dark:bg-green-900/20 rounded-lg">
          <p className="text-sm text-green-600 dark:text-green-400 font-medium">Traditional</p>
          <p className="text-lg font-bold text-green-700 dark:text-green-300 mt-1">
            {formatCurrency(totalTradFi)}
          </p>
          <p className="text-xs text-green-600 dark:text-green-400 mt-1">
            {totalCrypto + totalTradFi > 0
              ? `${((totalTradFi / (totalCrypto + totalTradFi)) * 100).toFixed(1)}%`
              : '0%'}
          </p>
        </div>
      </div>
    </div>
  );
}
