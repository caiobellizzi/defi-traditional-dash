import { useSignalR } from '@/shared/hooks/useSignalR';

/**
 * Connection Status Indicator
 * Displays real-time SignalR connection status in the UI
 */
export const ConnectionStatus = () => {
  const { connectionState, isConnected } = useSignalR();

  // Auto-hide when connected after 3 seconds
  if (isConnected) {
    return null;
  }

  const statusConfig = {
    connected: {
      text: 'Connected',
      color: 'bg-green-500',
      icon: '✓',
    },
    reconnecting: {
      text: 'Reconnecting...',
      color: 'bg-yellow-500',
      icon: '⟳',
    },
    disconnected: {
      text: 'Disconnected',
      color: 'bg-red-500',
      icon: '✕',
    },
    error: {
      text: 'Connection Error',
      color: 'bg-red-600',
      icon: '!',
    },
  };

  const config = statusConfig[connectionState] || statusConfig.disconnected;

  return (
    <div className="fixed bottom-4 right-4 z-50 animate-in slide-in-from-bottom-2 fade-in duration-300">
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 px-4 py-2 flex items-center gap-2 transition-colors">
        <div className={`w-2 h-2 rounded-full ${config.color} animate-pulse`} />
        <span className="text-sm text-gray-700 dark:text-gray-300">
          {config.text}
        </span>
        {connectionState === 'reconnecting' && (
          <div className="animate-spin w-3 h-3 border border-gray-300 border-t-gray-600 dark:border-gray-600 dark:border-t-gray-300 rounded-full" />
        )}
      </div>
    </div>
  );
};
