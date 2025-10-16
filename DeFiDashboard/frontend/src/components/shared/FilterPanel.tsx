import { ReactNode } from 'react';

interface FilterOption {
  label: string;
  value: string;
}

interface FilterConfig {
  name: string;
  label: string;
  type: 'select' | 'text' | 'date' | 'number';
  options?: FilterOption[];
  placeholder?: string;
}

interface FilterPanelProps {
  filters: Record<string, any>;
  config: FilterConfig[];
  onFilterChange: (name: string, value: string) => void;
  onClear: () => void;
  hasActiveFilters?: boolean;
  children?: ReactNode;
}

/**
 * Reusable Filter Panel Component
 * Provides a flexible filter interface with different input types
 */
export function FilterPanel({
  filters,
  config,
  onFilterChange,
  onClear,
  hasActiveFilters,
  children,
}: FilterPanelProps) {
  const renderFilterInput = (filterConfig: FilterConfig) => {
    const value = filters[filterConfig.name] || '';

    switch (filterConfig.type) {
      case 'select':
        return (
          <select
            value={value}
            onChange={(e) => onFilterChange(filterConfig.name, e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
          >
            <option value="">All</option>
            {filterConfig.options?.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        );

      case 'text':
        return (
          <input
            type="text"
            value={value}
            onChange={(e) => onFilterChange(filterConfig.name, e.target.value)}
            placeholder={filterConfig.placeholder}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
          />
        );

      case 'date':
        return (
          <input
            type="date"
            value={value}
            onChange={(e) => onFilterChange(filterConfig.name, e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
          />
        );

      case 'number':
        return (
          <input
            type="number"
            value={value}
            onChange={(e) => onFilterChange(filterConfig.name, e.target.value)}
            placeholder={filterConfig.placeholder}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors"
          />
        );

      default:
        return null;
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Filters</h2>
        {hasActiveFilters && (
          <button
            onClick={onClear}
            className="text-sm text-blue-600 dark:text-blue-400 hover:underline"
          >
            Clear All
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {config.map((filterConfig) => (
          <div key={filterConfig.name}>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              {filterConfig.label}
            </label>
            {renderFilterInput(filterConfig)}
          </div>
        ))}
        {children}
      </div>
    </div>
  );
}
