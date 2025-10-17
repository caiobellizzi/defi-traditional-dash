import { apiClient } from '@/shared/lib/api-client';
import type { Wallet, WalletPagedResult, AddWalletDto, WalletBalance } from '../types/wallet.types';

/**
 * Wallets API Service
 */
export const walletsApi = {
  /**
   * Get all wallets (handles pagination internally)
   */
  getAll: async (): Promise<Wallet[]> => {
    const response = await apiClient.get<WalletPagedResult>('/api/wallets', {
      params: { pageNumber: 1, pageSize: 1000 },
    });
    return response.data.items;
  },

  /**
   * Get wallet by ID
   */
  getById: async (id: string): Promise<Wallet> => {
    const response = await apiClient.get<Wallet>(`/api/wallets/${id}`);
    return response.data;
  },

  /**
   * Add new wallet
   */
  add: async (data: AddWalletDto): Promise<string> => {
    const response = await apiClient.post<string>('/api/wallets', data);
    return response.data;
  },

  /**
   * Delete wallet
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/wallets/${id}`);
  },

  /**
   * Get wallet balances
   */
  getBalances: async (id: string): Promise<WalletBalance[]> => {
    const response = await apiClient.get<WalletBalance[]>(`/api/wallets/${id}/balances`);
    return response.data;
  },

  /**
   * Sync wallet data from blockchain
   */
  sync: async (id: string): Promise<void> => {
    await apiClient.post(`/api/wallets/${id}/sync`);
  },
};
