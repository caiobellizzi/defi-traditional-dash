import { Link } from 'react-router-dom';
import { useAlertsSummary } from '../../hooks/useAlerts';
import { AlertBadge } from '../alerts/AlertBadge';
import { formatDateTime } from '../../lib/formatters';

/**
 * Recent Alerts Component
 * Shows unresolved alerts in the dashboard
 */
export function RecentAlerts() {
  const { data: summary, isLoading, error } = useAlertsSummary();

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div key={i} className="animate-pulse">
            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
            <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
          </div>
        ))}
      </div>
    );
  }

  if (error || !summary || summary.recentAlerts.length === 0) {
    return (
      <div className="text-center py-8">
        <div className="text-green-600 dark:text-green-400 mb-2">
          <svg
            className="mx-auto h-12 w-12"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {error ? 'Unable to load alerts' : 'No active alerts - All systems operational'}
        </p>
        <Link
          to="/alerts"
          className="text-sm text-blue-600 dark:text-blue-400 hover:underline mt-2 inline-block"
        >
          View alert history
        </Link>
      </div>
    );
  }

  const alerts = summary.recentAlerts.slice(0, 5);

  return (
    <div className="space-y-4">
      {/* Summary Stats */}
      <div className="grid grid-cols-4 gap-2 pb-4 border-b border-gray-200 dark:border-gray-700">
        <div className="text-center">
          <p className="text-lg font-bold text-red-600 dark:text-red-400">
            {summary.criticalCount}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-400">Critical</p>
        </div>
        <div className="text-center">
          <p className="text-lg font-bold text-orange-600 dark:text-orange-400">
            {summary.highCount}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-400">High</p>
        </div>
        <div className="text-center">
          <p className="text-lg font-bold text-amber-600 dark:text-amber-400">
            {summary.mediumCount}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-400">Medium</p>
        </div>
        <div className="text-center">
          <p className="text-lg font-bold text-blue-600 dark:text-blue-400">{summary.lowCount}</p>
          <p className="text-xs text-gray-500 dark:text-gray-400">Low</p>
        </div>
      </div>

      {/* Recent Alerts */}
      <div className="space-y-3">
        {alerts.map((alert) => (
          <div
            key={alert.id}
            className="flex items-start gap-3 pb-3 border-b border-gray-200 dark:border-gray-700 last:border-0 last:pb-0"
          >
            <div className="flex-shrink-0 mt-0.5">
              {alert.severity === 'Critical' || alert.severity === 'High' ? (
                <svg
                  className="h-5 w-5 text-red-500 dark:text-red-400"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                  />
                </svg>
              ) : (
                <svg
                  className="h-5 w-5 text-blue-500 dark:text-blue-400"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              )}
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                  {alert.title}
                </p>
                <AlertBadge severity={alert.severity} />
              </div>
              <p className="text-xs text-gray-500 dark:text-gray-400">
                {formatDateTime(alert.createdAt)}
              </p>
            </div>
          </div>
        ))}
      </div>

      <Link
        to="/alerts"
        className="block text-center text-sm text-blue-600 dark:text-blue-400 hover:underline mt-4"
      >
        View all alerts →
      </Link>
    </div>
  );
}
