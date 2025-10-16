import { Link } from 'react-router-dom';
import { useTransactions } from '../../hooks/useTransactions';
import { formatCurrency, formatDateTime } from '../../lib/formatters';

/**
 * Recent Transactions Component
 * Shows the latest transactions in the dashboard
 */
export function RecentTransactions() {
  const { data, isLoading, error } = useTransactions({ pageNumber: 1, pageSize: 10 });

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3, 4, 5].map((i) => (
          <div key={i} className="animate-pulse">
            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
            <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
          </div>
        ))}
      </div>
    );
  }

  if (error || !data || data.items.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {error ? 'Unable to load transactions' : 'No recent transactions'}
        </p>
        <Link
          to="/transactions"
          className="text-sm text-blue-600 dark:text-blue-400 hover:underline mt-2 inline-block"
        >
          View all transactions
        </Link>
      </div>
    );
  }

  const transactions = data.items.slice(0, 5);

  return (
    <div className="space-y-3">
      {transactions.map((tx) => (
        <div
          key={tx.id}
          className="flex items-start justify-between pb-3 border-b border-gray-200 dark:border-gray-700 last:border-0 last:pb-0"
        >
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <span
                className={`px-2 py-0.5 rounded text-xs font-medium ${
                  tx.direction === 'IN'
                    ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                    : tx.direction === 'OUT'
                    ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'
                    : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                }`}
              >
                {tx.direction}
              </span>
              <span className="text-sm font-medium text-gray-900 dark:text-white">
                {tx.tokenSymbol || 'Unknown'}
              </span>
            </div>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              {formatDateTime(tx.transactionDate)}
            </p>
          </div>
          <div className="text-right ml-4">
            <p className="text-sm font-semibold text-gray-900 dark:text-white">
              {tx.amountUsd ? formatCurrency(tx.amountUsd) : 'N/A'}
            </p>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              {tx.amount.toFixed(4)} {tx.tokenSymbol}
            </p>
          </div>
        </div>
      ))}

      <Link
        to="/transactions"
        className="block text-center text-sm text-blue-600 dark:text-blue-400 hover:underline mt-4"
      >
        View all transactions →
      </Link>
    </div>
  );
}
