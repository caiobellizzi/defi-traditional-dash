import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { allocationsApi } from '../api/allocations';
import type { CreateAllocationCommand } from '../types/api';

/**
 * React Query hooks for allocation operations
 */

/**
 * Fetch all allocations
 */
export const useAllocations = () => {
  return useQuery({
    queryKey: ['allocations'],
    queryFn: () => allocationsApi.getAll(),
    staleTime: 10000,
  });
};

/**
 * Fetch allocations for a specific client
 */
export const useClientAllocations = (clientId: string) => {
  return useQuery({
    queryKey: ['allocations', 'client', clientId],
    queryFn: () => allocationsApi.getByClient(clientId),
    enabled: !!clientId,
    staleTime: 10000,
  });
};

/**
 * Create new allocation mutation
 */
export const useCreateAllocation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAllocationCommand) => allocationsApi.create(data),
    onSuccess: (_, variables) => {
      // Invalidate allocations for the specific client
      queryClient.invalidateQueries({
        queryKey: ['allocations', 'client', variables.clientId],
      });
      // Also invalidate client portfolio
      queryClient.invalidateQueries({
        queryKey: ['clients', variables.clientId, 'portfolio'],
      });
    },
  });
};

/**
 * Delete allocation mutation
 */
export const useDeleteAllocation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => allocationsApi.delete(id),
    onSuccess: () => {
      // Invalidate all allocations (we don't have clientId here)
      queryClient.invalidateQueries({ queryKey: ['allocations'] });
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
