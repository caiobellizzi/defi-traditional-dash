import { apiClient } from '../lib/api-client';
import type {
  ClientDto,
  ClientDtoPagedResult,
  CreateClientCommand,
  UpdateClientCommand,
  ClientPortfolioDto,
} from '../types/api';

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
  }): Promise<ClientDtoPagedResult> => {
    const response = await apiClient.get<ClientDtoPagedResult>('/api/clients', {
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
  getById: async (id: string): Promise<ClientDto> => {
    const response = await apiClient.get<ClientDto>(`/api/clients/${id}`);
    return response.data;
  },

  /**
   * Create new client
   */
  create: async (data: CreateClientCommand): Promise<string> => {
    const response = await apiClient.post<string>('/api/clients', data);
    return response.data;
  },

  /**
   * Update existing client
   */
  update: async (id: string, data: UpdateClientCommand): Promise<void> => {
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
  getPortfolio: async (id: string): Promise<ClientPortfolioDto> => {
    const response = await apiClient.get<ClientPortfolioDto>(
      `/api/clients/${id}/portfolio`
    );
    return response.data;
  },
};
