import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useSignalR } from '@/shared/hooks/useSignalR';

/**
 * Real-time updates hook for system-wide alerts
 */
export const useAlertsRealtime = () => {
  const queryClient = useQueryClient();
  const { subscribe, subscribeToAlerts, unsubscribeFromAlerts } = useSignalR();

  useEffect(() => {
    // Subscribe to system-wide alerts
    subscribeToAlerts();

    // New alerts
    const unsubAlert = subscribe('NewAlert', (alert: any) => {
      queryClient.invalidateQueries({ queryKey: ['alerts'] });

      // Show toast based on severity
      const severityMap: Record<string, { icon: string; duration: number }> = {
        High: { icon: '🚨', duration: 10000 },
        Medium: { icon: '⚠️', duration: 6000 },
        Low: { icon: 'ℹ️', duration: 4000 },
      };
      const severityConfig = severityMap[alert.severity] || { icon: 'ℹ️', duration: 4000 };

      toast(alert.message, {
        icon: severityConfig.icon,
        duration: severityConfig.duration,
        style: alert.severity === 'High' ? {
          background: '#fef2f2',
          color: '#dc2626',
          border: '1px solid #fca5a5',
        } : alert.severity === 'Medium' ? {
          background: '#fffbeb',
          color: '#d97706',
          border: '1px solid #fcd34d',
        } : undefined,
      });
    });

    // Allocation drift
    const unsubDrift = subscribe('AllocationDrift', (data: any) => {
      queryClient.invalidateQueries({ queryKey: ['allocations'] });
      queryClient.invalidateQueries({ queryKey: ['alerts'] });

      toast.error(data.message, {
        icon: '⚖️',
        duration: 8000,
      });
    });

    // System messages
    const unsubSystem = subscribe('SystemMessage', (data: any) => {
      toast(data.message, {
        icon: '🔔',
        duration: 6000,
      });
    });

    return () => {
      unsubscribeFromAlerts();
      unsubAlert();
      unsubDrift();
      unsubSystem();
    };
  }, [subscribe, subscribeToAlerts, unsubscribeFromAlerts, queryClient]);
};
