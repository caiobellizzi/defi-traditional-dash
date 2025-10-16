import { useQuery } from '@tanstack/react-query';
import { transactionsApi } from '../api/transactions';
import type { TransactionFilters } from '../types/api';

/**
 * Hook to fetch transactions with filtering and pagination
 */
export const useTransactions = (filters?: TransactionFilters) => {
  return useQuery({
    queryKey: ['transactions', filters],
    queryFn: () => transactionsApi.getAll(filters),
  });
};

/**
 * Hook to fetch a single transaction by ID
 */
export const useTransaction = (id: string) => {
  return useQuery({
    queryKey: ['transaction', id],
    queryFn: () => transactionsApi.getById(id),
    enabled: !!id,
  });
};
