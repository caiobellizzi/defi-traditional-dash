import { formatCurrency, formatPercentage } from '@/shared/lib/formatters';
import type { AllocationDrift } from '../types/analytics.types';

interface AllocationDriftTableProps {
  drifts: AllocationDrift[];
  isLoading?: boolean;
}

/**
 * Allocation Drift Table Component
 * Shows allocations that have drifted from their targets
 */
export function AllocationDriftTable({ drifts, isLoading }: AllocationDriftTableProps) {
  if (isLoading) {
    return (
      <div className="p-12 text-center">
        <div className="animate-pulse text-gray-400 dark:text-gray-500">
          Loading drift analysis...
        </div>
      </div>
    );
  }

  if (drifts.length === 0) {
    return (
      <div className="p-12 text-center">
        <p className="text-gray-500 dark:text-gray-400">
          No allocation drift detected. All allocations are within target ranges.
        </p>
      </div>
    );
  }

  // Sort by drift percentage (absolute value)
  const sortedDrifts = [...drifts].sort(
    (a, b) => Math.abs(b.driftPercentage) - Math.abs(a.driftPercentage)
  );

  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead className="bg-gray-50 dark:bg-gray-900/50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Client
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Asset
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Type
            </th>
            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Current Value
            </th>
            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Target Value
            </th>
            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Drift
            </th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Status
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
          {sortedDrifts.map((drift) => {
            const isOverAllocated = drift.driftPercentage > 0;
            return (
              <tr
                key={drift.allocationId}
                className="hover:bg-gray-50 dark:hover:bg-gray-900/30 transition-colors"
              >
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900 dark:text-white">
                    {drift.clientName}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-900 dark:text-white">{drift.assetName}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 py-1 rounded-full text-xs ${
                      drift.assetType === 'Wallet'
                        ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
                        : 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                    }`}
                  >
                    {drift.assetType}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-900 dark:text-white">
                  {formatCurrency(drift.currentValueUsd)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600 dark:text-gray-400">
                  {formatCurrency(drift.targetValueUsd)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right">
                  <div
                    className={`text-sm font-semibold ${
                      isOverAllocated
                        ? 'text-amber-600 dark:text-amber-400'
                        : 'text-blue-600 dark:text-blue-400'
                    }`}
                  >
                    {isOverAllocated ? '+' : ''}
                    {formatPercentage(drift.driftPercentage)}
                  </div>
                  <div className="text-xs text-gray-500 dark:text-gray-400">
                    {isOverAllocated ? '+' : ''}
                    {formatCurrency(drift.driftAmount)}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-center">
                  {drift.requiresRebalancing ? (
                    <span className="px-2 py-1 rounded-full text-xs bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400">
                      Rebalance
                    </span>
                  ) : (
                    <span className="px-2 py-1 rounded-full text-xs bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400">
                      OK
                    </span>
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
