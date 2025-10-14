import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { walletsApi } from '../api/wallets';
import type { AddWalletCommand } from '../types/api';

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
 * Add new wallet mutation
 */
export const useAddWallet = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AddWalletCommand) => walletsApi.add(data),
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
