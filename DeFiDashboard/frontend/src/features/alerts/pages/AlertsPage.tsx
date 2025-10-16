import { PageLayout } from '@/shared/components/layout/PageLayout';
import { useAlertsRealtime } from '@/hooks/useAlertsRealtime';

export default function AlertsPage() {
  // Enable real-time alert notifications
  useAlertsRealtime();

  return (
    <PageLayout
      title="Alerts"
      subtitle="System alerts and notifications"
    >
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 transition-colors">
        <div className="text-center py-12 text-gray-500 dark:text-gray-400">
          <h3 className="text-lg font-semibold mb-2">Coming Soon</h3>
          <p>Alert management and notifications will be available here.</p>
          <p className="text-xs mt-4 text-gray-400 dark:text-gray-500">
            Real-time alerts are enabled - you will receive toast notifications when new alerts arrive
          </p>
        </div>
      </div>
    </PageLayout>
  );
}
