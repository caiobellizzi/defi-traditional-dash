import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useClients } from '../hooks/useClients';
import { useWallets } from '../hooks/useWallets';
import { useAllocations } from '../hooks/useAllocations';
import { StatCard } from '../components/dashboard/StatCard';
import { QuickActionCard } from '../components/dashboard/QuickActionCard';
import { PageLayout } from '../components/layout/PageLayout';
import { formatDate } from '../lib/formatters';
import {
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';

/**
 * Color palette for charts
 */
const CHART_COLORS = {
  primary: ['#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981'],
  status: {
    Active: '#10b981',
    Inactive: '#6b7280',
    Suspended: '#ef4444',
  },
};

/**
 * Dashboard page - Central hub for custody management overview
 */
export default function DashboardPage() {
  const { data: clientsData, isLoading: clientsLoading } = useClients();
  const { data: walletsData, isLoading: walletsLoading } = useWallets();
  const { data: allocationsData, isLoading: allocationsLoading } = useAllocations();

  // Calculate statistics
  const stats = useMemo(() => {
    const totalClients = clientsData?.totalCount || 0;
    const totalWallets = walletsData?.length || 0;
    const activeAllocations = allocationsData?.filter((a) => !a.endDate).length || 0;

    // Count unique networks from wallets
    const uniqueNetworks = new Set<string>();
    walletsData?.forEach((wallet) => {
      wallet.supportedChains.forEach((chain: string) => uniqueNetworks.add(chain));
    });
    const totalNetworks = uniqueNetworks.size;

    return {
      totalClients,
      totalWallets,
      activeAllocations,
      totalNetworks,
    };
  }, [clientsData, walletsData, allocationsData]);

  // Prepare client status data for pie chart
  const clientStatusData = useMemo(() => {
    if (!clientsData?.items) return [];

    const statusCounts: Record<string, number> = {};
    clientsData.items.forEach((client) => {
      const status = client.status || 'Active';
      statusCounts[status] = (statusCounts[status] || 0) + 1;
    });

    return Object.entries(statusCounts).map(([name, value]) => ({
      name,
      value,
      color: CHART_COLORS.status[name as keyof typeof CHART_COLORS.status] || '#6b7280',
    }));
  }, [clientsData]);

  // Prepare network distribution data for bar chart
  const networkData = useMemo(() => {
    if (!walletsData) return [];

    const networkCounts: Record<string, number> = {};
    walletsData.forEach((wallet) => {
      wallet.supportedChains.forEach((chain: string) => {
        networkCounts[chain] = (networkCounts[chain] || 0) + 1;
      });
    });

    return Object.entries(networkCounts)
      .map(([network, count]) => ({ network, count }))
      .sort((a, b) => b.count - a.count);
  }, [walletsData]);

  // Get recent clients and wallets
  const recentClients = useMemo(() => {
    if (!clientsData?.items) return [];
    return clientsData.items.slice(0, 5);
  }, [clientsData]);

  const recentWallets = useMemo(() => {
    if (!walletsData) return [];
    return walletsData.slice(0, 5);
  }, [walletsData]);

  const isLoading = clientsLoading || walletsLoading || allocationsLoading;

  return (
    <PageLayout>
      <div className="space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1">
            Custody asset management overview
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <StatCard
            title="Total Clients"
            value={stats.totalClients}
            subtitle={`${clientStatusData.find((s) => s.name === 'Active')?.value || 0} active`}
            isLoading={clientsLoading}
            icon={
              <svg
                className="w-12 h-12"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                />
              </svg>
            }
          />

          <StatCard
            title="Total Wallets"
            value={stats.totalWallets}
            subtitle={`${walletsData?.filter((w) => w.status === 'Active').length || 0} active`}
            isLoading={walletsLoading}
            icon={
              <svg
                className="w-12 h-12"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z"
                />
              </svg>
            }
          />

          <StatCard
            title="Active Allocations"
            value={stats.activeAllocations}
            subtitle="Current assignments"
            isLoading={allocationsLoading}
            icon={
              <svg
                className="w-12 h-12"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                />
              </svg>
            }
          />

          <StatCard
            title="Supported Networks"
            value={stats.totalNetworks}
            subtitle="Blockchain networks"
            isLoading={walletsLoading}
            icon={
              <svg
                className="w-12 h-12"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"
                />
              </svg>
            }
          />
        </div>

        {/* Charts Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Client Status Distribution */}
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors duration-200">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Client Status Distribution
            </h2>
            {isLoading ? (
              <div className="h-80 flex items-center justify-center">
                <div className="animate-pulse text-gray-400 dark:text-gray-500">Loading chart...</div>
              </div>
            ) : clientStatusData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={clientStatusData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={(props: any) =>
                      `${props.name}: ${(props.percent * 100).toFixed(0)}%`
                    }
                    outerRadius={100}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {clientStatusData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip contentStyle={{ backgroundColor: 'var(--tooltip-bg)', border: '1px solid var(--tooltip-border)' }} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 flex items-center justify-center text-gray-400 dark:text-gray-500">
                No client data available
              </div>
            )}
          </div>

          {/* Wallets by Network */}
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors duration-200">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Wallets by Network
            </h2>
            {isLoading ? (
              <div className="h-80 flex items-center justify-center">
                <div className="animate-pulse text-gray-400 dark:text-gray-500">Loading chart...</div>
              </div>
            ) : networkData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={networkData}>
                  <XAxis dataKey="network" stroke="currentColor" className="text-gray-600 dark:text-gray-400" />
                  <YAxis stroke="currentColor" className="text-gray-600 dark:text-gray-400" />
                  <Tooltip contentStyle={{ backgroundColor: 'var(--tooltip-bg)', border: '1px solid var(--tooltip-border)' }} />
                  <Bar dataKey="count" fill="#3b82f6" radius={[8, 8, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 flex items-center justify-center text-gray-400 dark:text-gray-500">
                No wallet data available
              </div>
            )}
          </div>
        </div>

        {/* Recent Activity */}
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden shadow-card dark:shadow-card-dark transition-colors duration-200">
          <div className="p-6 border-b border-gray-200 dark:border-gray-700">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
              Recent Activity
            </h2>
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-2 divide-y lg:divide-y-0 lg:divide-x divide-gray-200 dark:divide-gray-700">
            {/* Recent Clients */}
            <div className="p-6">
              <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-4 uppercase tracking-wide">
                Recently Added Clients
              </h3>
              {isLoading ? (
                <div className="space-y-3">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="animate-pulse">
                      <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                      <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
                    </div>
                  ))}
                </div>
              ) : recentClients.length > 0 ? (
                <div className="space-y-3">
                  {recentClients.map((client) => (
                    <div key={client.id} className="flex items-center justify-between group/item">
                      <div className="min-w-0 flex-1">
                        <Link
                          to="/clients"
                          className="text-sm font-medium text-gray-900 dark:text-gray-100 hover:text-blue-600 dark:hover:text-blue-400 truncate block transition-colors"
                        >
                          {client.name}
                        </Link>
                        <p className="text-xs text-gray-500 dark:text-gray-400">
                          {client.email}
                        </p>
                      </div>
                      <span className="text-xs text-gray-400 dark:text-gray-500 ml-2">
                        {formatDate(client.createdAt)}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-gray-400 dark:text-gray-500">No clients yet</p>
              )}
            </div>

            {/* Recent Wallets */}
            <div className="p-6">
              <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-4 uppercase tracking-wide">
                Recently Added Wallets
              </h3>
              {isLoading ? (
                <div className="space-y-3">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="animate-pulse">
                      <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                      <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
                    </div>
                  ))}
                </div>
              ) : recentWallets.length > 0 ? (
                <div className="space-y-3">
                  {recentWallets.map((wallet: any) => (
                    <div key={wallet.id} className="flex items-center justify-between group/item">
                      <div className="min-w-0 flex-1">
                        <Link
                          to="/wallets"
                          className="text-sm font-medium text-gray-900 dark:text-gray-100 hover:text-blue-600 dark:hover:text-blue-400 truncate block transition-colors"
                        >
                          {wallet.label}
                        </Link>
                        <p className="text-xs text-gray-500 dark:text-gray-400">
                          {wallet.supportedChains.join(', ')}
                        </p>
                      </div>
                      <span className="text-xs text-gray-400 dark:text-gray-500 ml-2">
                        {formatDate(wallet.createdAt)}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-gray-400 dark:text-gray-500">No wallets yet</p>
              )}
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div>
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Quick Actions
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <QuickActionCard
              title="Add New Client"
              description="Register a new client to the custody platform"
              linkTo="/clients"
              linkText="Go to Clients"
              icon={
                <svg
                  className="w-8 h-8"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"
                  />
                </svg>
              }
            />

            <QuickActionCard
              title="Add New Wallet"
              description="Connect a custody wallet to the platform"
              linkTo="/wallets"
              linkText="Go to Wallets"
              icon={
                <svg
                  className="w-8 h-8"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                  />
                </svg>
              }
            />

            <QuickActionCard
              title="Create Allocation"
              description="Assign wallet shares to client portfolios"
              linkTo="/allocations"
              linkText="Go to Allocations"
              icon={
                <svg
                  className="w-8 h-8"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"
                  />
                </svg>
              }
            />
          </div>
        </div>
      </div>
    </PageLayout>
  );
}
