import { apiClient } from '../lib/api-client';
import type {
  TransactionDto,
  TransactionDtoPagedResult,
  TransactionFilters,
} from '../types/api';

/**
 * Transactions API Service
 */
export const transactionsApi = {
  /**
   * Get paginated list of transactions with filtering
   */
  getAll: async (filters?: TransactionFilters): Promise<TransactionDtoPagedResult> => {
    const response = await apiClient.get<TransactionDtoPagedResult>('/api/transactions', {
      params: {
        pageNumber: filters?.pageNumber ?? 1,
        pageSize: filters?.pageSize ?? 20,
        transactionType: filters?.transactionType,
        assetId: filters?.assetId,
        direction: filters?.direction,
        fromDate: filters?.fromDate,
        toDate: filters?.toDate,
        tokenSymbol: filters?.tokenSymbol,
        status: filters?.status,
      },
    });
    return response.data;
  },

  /**
   * Get transaction by ID
   */
  getById: async (id: string): Promise<TransactionDto> => {
    const response = await apiClient.get<TransactionDto>(`/api/transactions/${id}`);
    return response.data;
  },
};
