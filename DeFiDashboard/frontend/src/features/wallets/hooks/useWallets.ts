import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { walletsApi } from '../api/wallets.api';
import type { AddWalletDto } from '../types/wallet.types';

/**
 * React Query hooks for wallet operations
 */

/**
 * Fetch all wallets
 */
export const useWallets = () => {
  return useQuery({
    queryKey: ['wallets'],
    queryFn: () => walletsApi.getAll(),
    staleTime: 30000,
  });
};

/**
 * Fetch single wallet by ID
 */
export const useWallet = (id: string) => {
  return useQuery({
    queryKey: ['wallets', id],
    queryFn: () => walletsApi.getById(id),
    enabled: !!id,
    staleTime: 30000,
  });
};

/**
 * Fetch wallet balances
 */
export const useWalletBalances = (id: string) => {
  return useQuery({
    queryKey: ['wallets', id, 'balances'],
    queryFn: () => walletsApi.getBalances(id),
    enabled: !!id,
    staleTime: 10000, // 10 seconds
  });
};

/**
 * Add new wallet mutation
 */
export const useAddWallet = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AddWalletDto) => walletsApi.add(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
    },
  });
};

/**
 * Delete wallet mutation
 */
export const useDeleteWallet = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => walletsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
    },
  });
};

/**
 * Sync wallet mutation
 */
export const useSyncWallet = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => walletsApi.sync(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['wallets', id] });
      queryClient.invalidateQueries({ queryKey: ['wallets', id, 'balances'] });
    },
  });
};
