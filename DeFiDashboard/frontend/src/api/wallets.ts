import { apiClient } from '../lib/api-client';
import type { WalletDto, AddWalletCommand } from '../types/api';

/**
 * Wallets API Service
 */
export const walletsApi = {
  /**
   * Get all wallets
   */
  getAll: async (): Promise<WalletDto[]> => {
    const response = await apiClient.get<WalletDto[]>('/api/wallets');
    return response.data;
  },

  /**
   * Get wallet by ID
   */
  getById: async (id: string): Promise<WalletDto> => {
    const response = await apiClient.get<WalletDto>(`/api/wallets/${id}`);
    return response.data;
  },

  /**
   * Add new wallet
   */
  add: async (data: AddWalletCommand): Promise<string> => {
    const response = await apiClient.post<string>('/api/wallets', data);
    return response.data;
  },

  /**
   * Delete wallet
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/wallets/${id}`);
  },
};
