import { useState } from 'react';
import { PageLayout } from '../components/layout/PageLayout';
import { AlertList } from '../components/alerts/AlertList';
import { useAlerts } from '../hooks/useAlerts';
import type { AlertFilters, AlertStatus, AlertSeverity } from '../types/alerts';

/**
 * Alerts Page - View and manage system alerts
 */
export default function AlertsPage() {
  const [filters, setFilters] = useState<AlertFilters>({
    status: 'Active',
  });

  const { data: alerts, isLoading, error } = useAlerts(filters);

  const handleFilterChange = (key: keyof AlertFilters, value: string | undefined) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value || undefined,
    }));
  };

  const clearFilters = () => {
    setFilters({ status: 'Active' });
  };

  const hasActiveFilters = Object.keys(filters).some(
    (key) => key !== 'status' && filters[key as keyof AlertFilters]
  );

  // Calculate alert counts by severity
  const alertCounts = alerts
    ? alerts.reduce(
        (acc, alert) => {
          acc[alert.severity] = (acc[alert.severity] || 0) + 1;
          return acc;
        },
        {} as Record<string, number>
      )
    : {};

  return (
    <PageLayout>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Alerts</h1>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          Monitor and manage system alerts and notifications
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-gray-500 dark:text-gray-400">Total Alerts</p>
          <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
            {alerts?.length || 0}
          </p>
        </div>
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-red-200 dark:border-red-700 p-4 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-red-600 dark:text-red-400">Critical</p>
          <p className="text-2xl font-bold text-red-600 dark:text-red-400 mt-1">
            {alertCounts['Critical'] || 0}
          </p>
        </div>
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-orange-200 dark:border-orange-700 p-4 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-orange-600 dark:text-orange-400">High</p>
          <p className="text-2xl font-bold text-orange-600 dark:text-orange-400 mt-1">
            {alertCounts['High'] || 0}
          </p>
        </div>
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-amber-200 dark:border-amber-700 p-4 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-amber-600 dark:text-amber-400">Medium</p>
          <p className="text-2xl font-bold text-amber-600 dark:text-amber-400 mt-1">
            {alertCounts['Medium'] || 0}
          </p>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark mb-6 transition-colors">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Filters</h2>
          {hasActiveFilters && (
            <button
              onClick={clearFilters}
              className="text-sm text-blue-600 dark:text-blue-400 hover:underline"
            >
              Clear All
            </button>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Status */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Status
            </label>
            <select
              value={filters.status || ''}
              onChange={(e) => handleFilterChange('status', e.target.value as AlertStatus)}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
            >
              <option value="">All</option>
              <option value="Active">Active</option>
              <option value="Acknowledged">Acknowledged</option>
              <option value="Resolved">Resolved</option>
              <option value="Dismissed">Dismissed</option>
            </select>
          </div>

          {/* Severity */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Severity
            </label>
            <select
              value={filters.severity || ''}
              onChange={(e) => handleFilterChange('severity', e.target.value as AlertSeverity)}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
            >
              <option value="">All</option>
              <option value="Critical">Critical</option>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>
          </div>

          {/* From Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              From Date
            </label>
            <input
              type="date"
              value={filters.fromDate || ''}
              onChange={(e) => handleFilterChange('fromDate', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
            />
          </div>

          {/* To Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              To Date
            </label>
            <input
              type="date"
              value={filters.toDate || ''}
              onChange={(e) => handleFilterChange('toDate', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
            />
          </div>
        </div>
      </div>

      {/* Alerts List */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-card dark:shadow-card-dark overflow-hidden transition-colors">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
            Alerts
            {alerts && (
              <span className="ml-2 text-sm text-gray-500 dark:text-gray-400 font-normal">
                ({alerts.length} total)
              </span>
            )}
          </h2>
        </div>

        {error ? (
          <div className="p-12 text-center">
            <p className="text-amber-600 dark:text-amber-400">
              Alerts feature is not available yet. This requires backend implementation.
            </p>
          </div>
        ) : (
          <AlertList alerts={alerts || []} isLoading={isLoading} showActions={true} />
        )}
      </div>
    </PageLayout>
  );
}
