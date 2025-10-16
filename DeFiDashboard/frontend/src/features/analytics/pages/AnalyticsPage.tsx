import { PageLayout } from '@/shared/components/layout/PageLayout';

export default function AnalyticsPage() {
  return (
    <PageLayout
      title="Analytics"
      subtitle="Performance metrics and portfolio analytics"
    >
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <div className="text-center py-12 text-gray-500 dark:text-gray-400">
          <h3 className="text-lg font-semibold mb-2">Coming Soon</h3>
          <p>Advanced analytics and performance tracking will be available here.</p>
        </div>
      </div>
    </PageLayout>
  );
}
