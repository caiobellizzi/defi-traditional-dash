import { apiClient } from '@/shared/lib/api-client';
import type {
  Client,
  ClientPagedResult,
  CreateClientDto,
  UpdateClientDto,
  ClientPortfolio,
} from '../types/client.types';

/**
 * Clients API Service
 */
export const clientsApi = {
  /**
   * Get paginated list of clients
   */
  getAll: async (params?: {
    pageNumber?: number;
    pageSize?: number;
  }): Promise<ClientPagedResult> => {
    const response = await apiClient.get<ClientPagedResult>('/api/clients', {
      params: {
        pageNumber: params?.pageNumber ?? 1,
        pageSize: params?.pageSize ?? 10,
      },
    });
    return response.data;
  },

  /**
   * Get client by ID
   */
  getById: async (id: string): Promise<Client> => {
    const response = await apiClient.get<Client>(`/api/clients/${id}`);
    return response.data;
  },

  /**
   * Create new client
   */
  create: async (data: CreateClientDto): Promise<string> => {
    const response = await apiClient.post<string>('/api/clients', data);
    return response.data;
  },

  /**
   * Update existing client
   */
  update: async (id: string, data: UpdateClientDto): Promise<void> => {
    await apiClient.put(`/api/clients/${id}`, data);
  },

  /**
   * Delete client
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/clients/${id}`);
  },

  /**
   * Get client portfolio with allocations
   */
  getPortfolio: async (id: string): Promise<ClientPortfolio> => {
    const response = await apiClient.get<ClientPortfolio>(
      `/api/clients/${id}/portfolio`
    );
    return response.data;
  },
};
