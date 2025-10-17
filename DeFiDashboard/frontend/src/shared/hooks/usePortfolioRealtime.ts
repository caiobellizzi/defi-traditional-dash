import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useSignalR } from '@/shared/hooks/useSignalR';

/**
 * Real-time updates hook for portfolio-wide data
 */
export const usePortfolioRealtime = () => {
  const queryClient = useQueryClient();
  const { subscribe } = useSignalR();

  useEffect(() => {
    // Portfolio updates
    const unsubPortfolio = subscribe('PortfolioUpdated', () => {
      queryClient.invalidateQueries({ queryKey: ['portfolio'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });

      toast.success('Portfolio recalculated', {
        icon: '📊',
      });
    });

    // Wallet balance updates
    const unsubWalletBalance = subscribe('WalletBalanceUpdated', () => {
      queryClient.invalidateQueries({ queryKey: ['portfolio'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
    });

    // Account balance updates
    const unsubAccountBalance = subscribe('AccountBalanceUpdated', () => {
      queryClient.invalidateQueries({ queryKey: ['portfolio'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    });

    // Sync status updates
    const unsubSync = subscribe('SyncStatusUpdated', (data: any) => {
      if (data.status === 'completed') {
        queryClient.invalidateQueries({ queryKey: ['portfolio'] });
        queryClient.invalidateQueries({ queryKey: ['dashboard'] });
        toast.success(`${data.source} sync completed`, {
          icon: '✅',
        });
      } else if (data.status === 'failed') {
        toast.error(`${data.source} sync failed`, {
          icon: '❌',
        });
      }
    });

    return () => {
      unsubPortfolio();
      unsubWalletBalance();
      unsubAccountBalance();
      unsubSync();
    };
  }, [subscribe, queryClient]);
};
