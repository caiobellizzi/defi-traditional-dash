import type { AlertSeverity } from '../types/alert.types';

interface AlertBadgeProps {
  severity: AlertSeverity;
  className?: string;
}

/**
 * Alert Severity Badge Component
 * Color-coded badge based on alert severity
 */
export function AlertBadge({ severity, className = '' }: AlertBadgeProps) {
  const getBadgeClasses = () => {
    switch (severity) {
      case 'Critical':
        return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400 border-red-300 dark:border-red-700';
      case 'High':
        return 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400 border-orange-300 dark:border-orange-700';
      case 'Medium':
        return 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400 border-amber-300 dark:border-amber-700';
      case 'Low':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400 border-blue-300 dark:border-blue-700';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300 border-gray-300 dark:border-gray-600';
    }
  };

  return (
    <span
      className={`px-2 py-1 rounded-full text-xs font-medium border ${getBadgeClasses()} ${className}`}
    >
      {severity}
    </span>
  );
}
