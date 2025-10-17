import { useQuery } from '@tanstack/react-query';
import { transactionsApi } from '../api/transactions.api';
import type { TransactionFilters } from '@/shared/types/api.types';

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
