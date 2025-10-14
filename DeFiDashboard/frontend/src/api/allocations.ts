import { apiClient } from '../lib/api-client';
import type { AllocationDto, CreateAllocationCommand } from '../types/api';

/**
 * Allocations API Service
 */
export const allocationsApi = {
  /**
   * Get all allocations
   */
  getAll: async (): Promise<AllocationDto[]> => {
    const response = await apiClient.get<AllocationDto[]>('/api/allocations');
    return response.data;
  },

  /**
   * Get allocations for a specific client
   */
  getByClient: async (clientId: string): Promise<AllocationDto[]> => {
    const response = await apiClient.get<AllocationDto[]>(
      `/api/allocations/client/${clientId}`
    );
    return response.data;
  },

  /**
   * Create new allocation
   */
  create: async (data: CreateAllocationCommand): Promise<string> => {
    const response = await apiClient.post<string>('/api/allocations', data);
    return response.data;
  },

  /**
   * Delete allocation
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/allocations/${id}`);
  },
};
