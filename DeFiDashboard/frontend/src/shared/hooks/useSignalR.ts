import { useEffect, useCallback, useState } from 'react';
import { signalRClient } from '@/shared/lib/signalr-client';

/**
 * Base SignalR hook for managing real-time connections
 * Automatically connects on mount and provides subscription utilities
 */
export const useSignalR = () => {
  const [connectionState, setConnectionState] = useState<'connected' | 'reconnecting' | 'disconnected' | 'error'>('disconnected');

  useEffect(() => {
    // Connect to SignalR hub
    signalRClient.connect();

    // Subscribe to connection state changes
    const unsubscribe = signalRClient.on('ConnectionStateChanged', (data: any) => {
      setConnectionState(data.state);
    });

    // Check initial connection state
    if (signalRClient.isConnected()) {
      setConnectionState('connected');
    }

    return () => {
      unsubscribe();
      // Don't disconnect on unmount (keep connection alive for other components)
    };
  }, []);

  const subscribe = useCallback((event: string, callback: (data: any) => void) => {
    return signalRClient.on(event, callback);
  }, []);

  return {
    connectionState,
    isConnected: connectionState === 'connected',
    subscribe,
    subscribeToClient: signalRClient.subscribeToClient.bind(signalRClient),
    unsubscribeFromClient: signalRClient.unsubscribeFromClient.bind(signalRClient),
    subscribeToWallet: signalRClient.subscribeToWallet.bind(signalRClient),
    unsubscribeFromWallet: signalRClient.unsubscribeFromWallet.bind(signalRClient),
    subscribeToAccount: signalRClient.subscribeToAccount.bind(signalRClient),
    unsubscribeFromAccount: signalRClient.unsubscribeFromAccount.bind(signalRClient),
    subscribeToAlerts: signalRClient.subscribeToAlerts.bind(signalRClient),
    unsubscribeFromAlerts: signalRClient.unsubscribeFromAlerts.bind(signalRClient),
  };
};
