/**
 * Dashboard Types
 */

export interface DashboardStats {
  totalAUM: number;
  totalClients: number;
  totalWallets: number;
  totalAccounts: number;
  recentTransactions: number;
  pendingAlerts: number;
}

export interface RecentActivity {
  id: string;
  type: string;
  description: string;
  timestamp: string;
  metadata?: Record<string, unknown>;
}
