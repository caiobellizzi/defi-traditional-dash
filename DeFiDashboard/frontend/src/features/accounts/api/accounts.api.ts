import { apiClient } from '@/shared/lib/api-client';
import type { Account, AccountBalance, PluggyConnectToken } from '../types/account.types';

/**
 * Accounts API Service (Traditional Finance via Pluggy)
 */
export const accountsApi = {
  /**
   * Get all accounts
   */
  getAll: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/api/accounts');
    return response.data;
  },

  /**
   * Get account by ID
   */
  getById: async (id: string): Promise<Account> => {
    const response = await apiClient.get<Account>(`/api/accounts/${id}`);
    return response.data;
  },

  /**
   * Get account balances
   */
  getBalances: async (id: string): Promise<AccountBalance[]> => {
    const response = await apiClient.get<AccountBalance[]>(`/api/accounts/${id}/balances`);
    return response.data;
  },

  /**
   * Create Pluggy Connect token for linking new account
   */
  createConnectToken: async (): Promise<PluggyConnectToken> => {
    const response = await apiClient.post<PluggyConnectToken>('/api/accounts/connect-token');
    return response.data;
  },

  /**
   * Sync account data from Pluggy
   */
  sync: async (id: string): Promise<void> => {
    await apiClient.post(`/api/accounts/${id}/sync`);
  },

  /**
   * Delete account
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/accounts/${id}`);
  },
};
