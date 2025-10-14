/**
 * StatCard component for displaying key metrics
 */

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: React.ReactNode;
  isLoading?: boolean;
}

export function StatCard({ title, value, subtitle, icon, isLoading }: StatCardProps) {
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg border p-6 animate-pulse">
        <div className="flex items-center justify-between">
          <div className="flex-1">
            <div className="h-4 bg-gray-200 rounded w-1/2 mb-3"></div>
            <div className="h-8 bg-gray-200 rounded w-3/4"></div>
          </div>
          {icon && <div className="w-12 h-12 bg-gray-200 rounded"></div>}
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg border p-6 hover:shadow-lg transition-shadow">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-500">{title}</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">{value}</p>
          {subtitle && (
            <p className="text-sm text-gray-400 mt-1">{subtitle}</p>
          )}
        </div>
        {icon && (
          <div className="text-gray-400 opacity-75">{icon}</div>
        )}
      </div>
    </div>
  );
}
