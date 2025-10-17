import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { accountsApi } from '../api/accounts.api';

/**
 * React Query hooks for account operations
 */

/**
 * Fetch all accounts
 */
export const useAccounts = () => {
  return useQuery({
    queryKey: ['accounts'],
    queryFn: () => accountsApi.getAll(),
    staleTime: 30000,
  });
};

/**
 * Fetch single account by ID
 */
export const useAccount = (id: string) => {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: () => accountsApi.getById(id),
    enabled: !!id,
    staleTime: 30000,
  });
};

/**
 * Fetch account balances
 */
export const useAccountBalances = (id: string) => {
  return useQuery({
    queryKey: ['accounts', id, 'balances'],
    queryFn: () => accountsApi.getBalances(id),
    enabled: !!id,
    staleTime: 10000,
  });
};

/**
 * Create Pluggy Connect token
 */
export const usePluggyConnect = () => {
  return useMutation({
    mutationFn: () => accountsApi.createConnectToken(),
  });
};

/**
 * Sync account mutation
 */
export const useSyncAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => accountsApi.sync(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', id] });
      queryClient.invalidateQueries({ queryKey: ['accounts', id, 'balances'] });
    },
  });
};

/**
 * Delete account mutation
 */
export const useDeleteAccount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => accountsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};
