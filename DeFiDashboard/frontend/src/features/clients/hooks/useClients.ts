import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clientsApi } from '../api/clients.api';
import type { CreateClientDto, UpdateClientDto } from '../types/client.types';

/**
 * React Query hooks for client operations
 */

/**
 * Fetch paginated clients list
 */
export const useClients = (params?: { pageNumber?: number; pageSize?: number }) => {
  return useQuery({
    queryKey: ['clients', params],
    queryFn: () => clientsApi.getAll(params),
    staleTime: 30000, // 30 seconds
  });
};

/**
 * Fetch single client by ID
 */
export const useClient = (id: string) => {
  return useQuery({
    queryKey: ['clients', id],
    queryFn: () => clientsApi.getById(id),
    enabled: !!id,
    staleTime: 30000,
  });
};

/**
 * Fetch client portfolio
 */
export const useClientPortfolio = (id: string) => {
  return useQuery({
    queryKey: ['clients', id, 'portfolio'],
    queryFn: () => clientsApi.getPortfolio(id),
    enabled: !!id,
    staleTime: 10000, // 10 seconds
  });
};

/**
 * Create new client mutation
 */
export const useCreateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateClientDto) => clientsApi.create(data),
    onSuccess: () => {
      // Invalidate and refetch clients list
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};

/**
 * Update existing client mutation
 */
export const useUpdateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateClientDto }) =>
      clientsApi.update(id, data),
    onSuccess: (_, variables) => {
      // Invalidate specific client and clients list
      queryClient.invalidateQueries({ queryKey: ['clients', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};

/**
 * Delete client mutation
 */
export const useDeleteClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => clientsApi.delete(id),
    onSuccess: () => {
      // Invalidate clients list
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
