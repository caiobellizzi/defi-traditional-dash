/**
 * Utility functions for formatting data
 */

/**
 * Format a number as USD currency
 */
export const formatCurrency = (value: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
};

/**
 * Format a date string to readable format
 */
export const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
};

/**
 * Format a date string to include time
 */
export const formatDateTime = (dateString: string): string => {
  return new Date(dateString).toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

/**
 * Truncate a wallet address for display
 */
export const truncateAddress = (
  address: string,
  start = 6,
  end = 4
): string => {
  if (address.length <= start + end) return address;
  return `${address.slice(0, start)}...${address.slice(-end)}`;
};

/**
 * Format a percentage value
 */
export const formatPercentage = (value: number): string => {
  return `${value.toFixed(2)}%`;
};
