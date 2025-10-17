import { useState } from 'react';
import { useAcknowledgeAlert, useResolveAlert, useDismissAlert } from '../hooks/useAlerts';

interface AlertActionsProps {
  alertId: string;
}

/**
 * Alert Actions Component
 * Provides actions to acknowledge, resolve, or dismiss alerts
 */
export function AlertActions({ alertId }: AlertActionsProps) {
  const [showResolveModal, setShowResolveModal] = useState(false);
  const [notes, setNotes] = useState('');

  const acknowledgeAlert = useAcknowledgeAlert();
  const resolveAlert = useResolveAlert();
  const dismissAlert = useDismissAlert();

  const handleAcknowledge = () => {
    acknowledgeAlert.mutate(alertId);
  };

  const handleResolve = () => {
    resolveAlert.mutate({ id: alertId, notes });
    setShowResolveModal(false);
    setNotes('');
  };

  const handleDismiss = () => {
    if (confirm('Are you sure you want to dismiss this alert?')) {
      dismissAlert.mutate(alertId);
    }
  };

  return (
    <>
      <div className="flex items-center gap-2">
        <button
          onClick={handleAcknowledge}
          disabled={acknowledgeAlert.isPending}
          className="px-3 py-1 text-xs font-medium text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded-lg transition-colors disabled:opacity-50"
          title="Acknowledge alert"
        >
          Acknowledge
        </button>
        <button
          onClick={() => setShowResolveModal(true)}
          disabled={resolveAlert.isPending}
          className="px-3 py-1 text-xs font-medium text-green-600 dark:text-green-400 hover:bg-green-50 dark:hover:bg-green-900/30 rounded-lg transition-colors disabled:opacity-50"
          title="Resolve alert"
        >
          Resolve
        </button>
        <button
          onClick={handleDismiss}
          disabled={dismissAlert.isPending}
          className="px-3 py-1 text-xs font-medium text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-900/30 rounded-lg transition-colors disabled:opacity-50"
          title="Dismiss alert"
        >
          Dismiss
        </button>
      </div>

      {/* Resolve Modal */}
      {showResolveModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-xl p-6 max-w-md w-full mx-4 shadow-xl">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Resolve Alert
            </h3>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Add resolution notes (optional)"
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-colors mb-4"
              rows={4}
            />
            <div className="flex gap-2 justify-end">
              <button
                onClick={() => setShowResolveModal(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleResolve}
                disabled={resolveAlert.isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 dark:bg-green-500 dark:hover:bg-green-600 rounded-lg transition-colors disabled:opacity-50"
              >
                {resolveAlert.isPending ? 'Resolving...' : 'Resolve'}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
