import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useSignalR } from '@/shared/hooks/useSignalR';

/**
 * Real-time updates hook for client-specific data
 * Automatically subscribes to client updates and invalidates queries
 */
export const useClientRealtime = (clientId: string | undefined) => {
  const queryClient = useQueryClient();
  const { subscribe, subscribeToClient, unsubscribeFromClient } = useSignalR();

  useEffect(() => {
    if (!clientId) return;

    // Subscribe to client-specific updates
    subscribeToClient(clientId);

    // Portfolio updates
    const unsubPortfolio = subscribe('PortfolioUpdated', (data: any) => {
      if (data.clientId === clientId) {
        queryClient.invalidateQueries({ queryKey: ['client', clientId, 'portfolio'] });
        queryClient.invalidateQueries({ queryKey: ['portfolio', clientId] });
        toast.success('Portfolio updated with latest data', {
          icon: '🔄',
        });
      }
    });

    // Wallet balance updates
    const unsubWalletBalance = subscribe('WalletBalanceUpdated', () => {
      // Invalidate client portfolio when any wallet balance changes
      queryClient.invalidateQueries({ queryKey: ['client', clientId, 'portfolio'] });
      queryClient.invalidateQueries({ queryKey: ['portfolio', clientId] });
    });

    // Account balance updates
    const unsubAccountBalance = subscribe('AccountBalanceUpdated', () => {
      // Invalidate client portfolio when any account balance changes
      queryClient.invalidateQueries({ queryKey: ['client', clientId, 'portfolio'] });
      queryClient.invalidateQueries({ queryKey: ['portfolio', clientId] });
    });

    // New transactions
    const unsubTransaction = subscribe('NewTransaction', (data: any) => {
      if (data.clientId === clientId) {
        queryClient.invalidateQueries({ queryKey: ['transactions'] });
        queryClient.invalidateQueries({ queryKey: ['client', clientId, 'transactions'] });
        toast.success(`New transaction: ${data.type}`, {
          icon: '💸',
        });
      }
    });

    // New alerts
    const unsubAlert = subscribe('NewAlert', (alert: any) => {
      if (alert.clientId === clientId) {
        queryClient.invalidateQueries({ queryKey: ['alerts'] });
        queryClient.invalidateQueries({ queryKey: ['client', clientId, 'alerts'] });

        // Show toast based on severity
        const severityMap: Record<string, { icon: string; duration: number }> = {
          High: { icon: '🚨', duration: 8000 },
          Medium: { icon: '⚠️', duration: 6000 },
          Low: { icon: 'ℹ️', duration: 4000 },
        };
        const toastConfig = severityMap[alert.severity] || { icon: 'ℹ️', duration: 4000 };

        toast(alert.message, {
          icon: toastConfig.icon,
          duration: toastConfig.duration,
          style: alert.severity === 'High' ? {
            background: '#fef2f2',
            color: '#dc2626',
            border: '1px solid #fca5a5',
          } : undefined,
        });
      }
    });

    // Allocation drift
    const unsubDrift = subscribe('AllocationDrift', (data: any) => {
      if (data.clientId === clientId) {
        queryClient.invalidateQueries({ queryKey: ['allocations'] });
        queryClient.invalidateQueries({ queryKey: ['client', clientId, 'allocations'] });
        toast.error(`Allocation drift detected: ${data.message}`, {
          icon: '⚖️',
          duration: 8000,
        });
      }
    });

    // Cleanup: unsubscribe from all events
    return () => {
      unsubscribeFromClient(clientId);
      unsubPortfolio();
      unsubWalletBalance();
      unsubAccountBalance();
      unsubTransaction();
      unsubAlert();
      unsubDrift();
    };
  }, [clientId, subscribe, subscribeToClient, unsubscribeFromClient, queryClient]);
};
