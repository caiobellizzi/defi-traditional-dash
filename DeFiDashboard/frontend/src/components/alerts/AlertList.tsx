import { useState } from 'react';
import { AlertBadge } from './AlertBadge';
import { AlertActions } from './AlertActions';
import { formatDateTime } from '../../lib/formatters';
import type { AlertDto } from '../../types/alerts';

interface AlertListProps {
  alerts: AlertDto[];
  isLoading?: boolean;
  showActions?: boolean;
}

/**
 * Alert List Component
 * Displays a list of alerts with optional actions
 */
export function AlertList({ alerts, isLoading, showActions = true }: AlertListProps) {
  const [expandedAlert, setExpandedAlert] = useState<string | null>(null);

  if (isLoading) {
    return (
      <div className="p-12 text-center">
        <div className="animate-pulse text-gray-400 dark:text-gray-500">Loading alerts...</div>
      </div>
    );
  }

  if (alerts.length === 0) {
    return (
      <div className="p-12 text-center">
        <div className="text-gray-500 dark:text-gray-400">
          <svg
            className="mx-auto h-12 w-12 text-gray-400 dark:text-gray-600 mb-4"
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
          <p>No alerts found. Everything looks good!</p>
        </div>
      </div>
    );
  }

  const toggleExpand = (alertId: string) => {
    setExpandedAlert(expandedAlert === alertId ? null : alertId);
  };

  const getAlertIcon = (severity: string) => {
    if (severity === 'Critical' || severity === 'High') {
      return (
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
      );
    }
    return (
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
    );
  };

  return (
    <div className="divide-y divide-gray-200 dark:divide-gray-700">
      {alerts.map((alert) => (
        <div
          key={alert.id}
          className={`p-6 transition-colors ${
            alert.status === 'Active'
              ? 'bg-white dark:bg-gray-800'
              : 'bg-gray-50 dark:bg-gray-900/30 opacity-75'
          }`}
        >
          <div className="flex items-start gap-4">
            {/* Icon */}
            <div className="flex-shrink-0 mt-1">{getAlertIcon(alert.severity)}</div>

            {/* Content */}
            <div className="flex-1 min-w-0">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <h3 className="text-sm font-semibold text-gray-900 dark:text-white">
                      {alert.title}
                    </h3>
                    <AlertBadge severity={alert.severity} />
                    <span className="px-2 py-1 rounded-full text-xs bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300">
                      {alert.status}
                    </span>
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">{alert.message}</p>
                  <div className="flex items-center gap-4 mt-2">
                    <span className="text-xs text-gray-500 dark:text-gray-500">
                      {formatDateTime(alert.createdAt)}
                    </span>
                    <span className="text-xs text-gray-500 dark:text-gray-500">
                      Type: {alert.alertType}
                    </span>
                    {alert.metadata && Object.keys(alert.metadata).length > 0 && (
                      <button
                        onClick={() => toggleExpand(alert.id)}
                        className="text-xs text-blue-600 dark:text-blue-400 hover:underline"
                      >
                        {expandedAlert === alert.id ? 'Hide' : 'Show'} Details
                      </button>
                    )}
                  </div>

                  {/* Expanded Details */}
                  {expandedAlert === alert.id && alert.metadata && (
                    <div className="mt-4 p-4 bg-gray-50 dark:bg-gray-900/50 rounded-lg">
                      <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300 mb-2">
                        Additional Details
                      </h4>
                      <pre className="text-xs text-gray-600 dark:text-gray-400 overflow-x-auto">
                        {JSON.stringify(alert.metadata, null, 2)}
                      </pre>
                    </div>
                  )}
                </div>

                {/* Actions */}
                {showActions && alert.status === 'Active' && (
                  <AlertActions alertId={alert.id} />
                )}
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
