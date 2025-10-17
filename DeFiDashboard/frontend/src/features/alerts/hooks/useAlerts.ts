import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { alertsApi } from '../api/alerts.api';
import type { AlertFilters } from '../types/alert.types';
import toast from 'react-hot-toast';

/**
 * Hook to fetch all alerts
 */
export const useAlerts = (filters?: AlertFilters) => {
  return useQuery({
    queryKey: ['alerts', filters],
    queryFn: () => alertsApi.getAll(filters),
    retry: false, // Don't retry if endpoint doesn't exist yet
  });
};

/**
 * Hook to fetch a single alert
 */
export const useAlert = (id: string) => {
  return useQuery({
    queryKey: ['alert', id],
    queryFn: () => alertsApi.getById(id),
    enabled: !!id,
    retry: false,
  });
};

/**
 * Hook to fetch alerts summary
 */
export const useAlertsSummary = () => {
  return useQuery({
    queryKey: ['alerts', 'summary'],
    queryFn: alertsApi.getSummary,
    retry: false,
  });
};

/**
 * Hook to acknowledge an alert
 */
export const useAcknowledgeAlert = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: alertsApi.acknowledge,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['alerts'] });
      toast.success('Alert acknowledged');
    },
    onError: () => {
      toast.error('Failed to acknowledge alert');
    },
  });
};

/**
 * Hook to resolve an alert
 */
export const useResolveAlert = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
      alertsApi.resolve(id, notes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['alerts'] });
      toast.success('Alert resolved');
    },
    onError: () => {
      toast.error('Failed to resolve alert');
    },
  });
};

/**
 * Hook to dismiss an alert
 */
export const useDismissAlert = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: alertsApi.dismiss,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['alerts'] });
      toast.success('Alert dismissed');
    },
    onError: () => {
      toast.error('Failed to dismiss alert');
    },
  });
};
