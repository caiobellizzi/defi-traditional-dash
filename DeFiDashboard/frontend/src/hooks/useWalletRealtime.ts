import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useSignalR } from '@/shared/hooks/useSignalR';

/**
 * Real-time updates hook for wallet-specific data
 */
export const useWalletRealtime = (walletId: string | undefined) => {
  const queryClient = useQueryClient();
  const { subscribe, subscribeToWallet, unsubscribeFromWallet } = useSignalR();

  useEffect(() => {
    if (!walletId) return;

    // Subscribe to wallet-specific updates
    subscribeToWallet(walletId);

    // Wallet balance updates
    const unsubBalance = subscribe('WalletBalanceUpdated', (data: any) => {
      if (data.walletId === walletId) {
        queryClient.invalidateQueries({ queryKey: ['wallet', walletId] });
        queryClient.invalidateQueries({ queryKey: ['wallet', walletId, 'balances'] });
        queryClient.invalidateQueries({ queryKey: ['wallets'] });
        toast.success('Wallet balance updated', {
          icon: '💰',
        });
      }
    });

    // New transactions
    const unsubTransaction = subscribe('NewTransaction', (data: any) => {
      if (data.walletId === walletId) {
        queryClient.invalidateQueries({ queryKey: ['wallet', walletId, 'transactions'] });
        queryClient.invalidateQueries({ queryKey: ['transactions'] });
        toast.success(`New transaction detected`, {
          icon: '💸',
        });
      }
    });

    return () => {
      unsubscribeFromWallet(walletId);
      unsubBalance();
      unsubTransaction();
    };
  }, [walletId, subscribe, subscribeToWallet, unsubscribeFromWallet, queryClient]);
};
