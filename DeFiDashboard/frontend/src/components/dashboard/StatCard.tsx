/**
 * StatCard component for displaying key metrics
 * Modern design with gradient accents and dark mode support
 */

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: React.ReactNode;
  isLoading?: boolean;
  variant?: 'default' | 'primary' | 'success' | 'warning';
}

export function StatCard({
  title,
  value,
  subtitle,
  icon,
  isLoading,
  variant = 'default'
}: StatCardProps) {
  // Gradient variants for modern look
  const gradients = {
    default: 'from-blue-500/10 to-purple-500/10 dark:from-blue-500/20 dark:to-purple-500/20',
    primary: 'from-blue-500/10 to-blue-600/10 dark:from-blue-500/20 dark:to-blue-600/20',
    success: 'from-green-500/10 to-emerald-500/10 dark:from-green-500/20 dark:to-emerald-500/20',
    warning: 'from-orange-500/10 to-yellow-500/10 dark:from-orange-500/20 dark:to-yellow-500/20',
  };

  const iconColors = {
    default: 'text-blue-500 dark:text-blue-400',
    primary: 'text-blue-600 dark:text-blue-400',
    success: 'text-green-600 dark:text-green-400',
    warning: 'text-orange-600 dark:text-orange-400',
  };

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 animate-pulse transition-colors duration-200">
        <div className="flex items-center justify-between">
          <div className="flex-1">
            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-3"></div>
            <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
          </div>
          {icon && <div className="w-12 h-12 bg-gray-200 dark:bg-gray-700 rounded-lg"></div>}
        </div>
      </div>
    );
  }

  return (
    <div className="group relative bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 hover:shadow-card-hover dark:hover:shadow-card-dark-hover transition-all duration-300 overflow-hidden">
      {/* Gradient background - subtle on hover */}
      <div className={`absolute inset-0 bg-gradient-to-br ${gradients[variant]} opacity-0 group-hover:opacity-100 transition-opacity duration-300`}></div>

      {/* Content */}
      <div className="relative flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600 dark:text-gray-400 uppercase tracking-wide">
            {title}
          </p>
          <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2 tracking-tight">
            {value}
          </p>
          {subtitle && (
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-2 flex items-center">
              {subtitle}
            </p>
          )}
        </div>
        {icon && (
          <div className={`${iconColors[variant]} opacity-80 group-hover:opacity-100 transition-opacity duration-300`}>
            {icon}
          </div>
        )}
      </div>
    </div>
  );
}
