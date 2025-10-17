import * as signalR from '@microsoft/signalr';

/**
 * SignalR Client Service for real-time dashboard updates
 * Manages WebSocket connection to backend hub and provides subscription management
 */
class SignalRClient {
  private connection: signalR.HubConnection | null = null;
  private subscribers = new Map<string, Set<(data: any) => void>>();
  private activeSubscriptions = new Set<string>();
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private isConnecting = false;

  /**
   * Establishes connection to SignalR hub
   */
  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.isConnecting) {
      return;
    }

    this.isConnecting = true;

    try {
      const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiBaseUrl}/hubs/dashboard`, {
          // Add auth token if available in the future
          // accessTokenFactory: () => authStorage.getAccessToken() || '',
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 0s, 2s, 4s, 8s, 16s, 30s (max)
            if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
              console.warn('SignalR: Max reconnection attempts reached');
              return null; // Stop retrying
            }
            const delay = Math.min(2000 * Math.pow(2, retryContext.previousRetryCount), 30000);
            console.log(`SignalR: Reconnecting in ${delay}ms...`);
            return delay;
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Event handlers
      this.connection.onreconnecting(() => {
        console.log('SignalR: Reconnecting...');
        this.reconnectAttempts++;
        this.emit('ConnectionStateChanged', { state: 'reconnecting' });
      });

      this.connection.onreconnected(() => {
        console.log('SignalR: Reconnected successfully');
        this.reconnectAttempts = 0;
        this.emit('ConnectionStateChanged', { state: 'connected' });
        this.resubscribeAll();
      });

      this.connection.onclose((error) => {
        console.log('SignalR: Connection closed', error);
        this.emit('ConnectionStateChanged', { state: 'disconnected' });

        if (error && this.reconnectAttempts < this.maxReconnectAttempts) {
          setTimeout(() => {
            this.isConnecting = false;
            this.connect();
          }, 5000);
        } else {
          this.isConnecting = false;
        }
      });

      // Register message handlers
      this.registerHandlers();

      await this.connection.start();
      console.log('SignalR: Connected successfully');
      this.emit('ConnectionStateChanged', { state: 'connected' });
    } catch (error) {
      console.error('SignalR: Connection error:', error);
      this.emit('ConnectionStateChanged', { state: 'error', error });

      // Retry connection after delay
      setTimeout(() => {
        this.isConnecting = false;
        this.connect();
      }, 5000);
    } finally {
      this.isConnecting = false;
    }
  }

  /**
   * Registers all SignalR message handlers
   */
  private registerHandlers(): void {
    if (!this.connection) return;

    // Portfolio updates
    this.connection.on('PortfolioUpdated', (data) => {
      console.log('SignalR: Portfolio updated', data);
      this.emit('PortfolioUpdated', data);
    });

    // Wallet balance updates
    this.connection.on('WalletBalanceUpdated', (data) => {
      console.log('SignalR: Wallet balance updated', data);
      this.emit('WalletBalanceUpdated', data);
    });

    // Account balance updates
    this.connection.on('AccountBalanceUpdated', (data) => {
      console.log('SignalR: Account balance updated', data);
      this.emit('AccountBalanceUpdated', data);
    });

    // New transactions
    this.connection.on('NewTransaction', (data) => {
      console.log('SignalR: New transaction', data);
      this.emit('NewTransaction', data);
    });

    // New alerts
    this.connection.on('NewAlert', (data) => {
      console.log('SignalR: New alert', data);
      this.emit('NewAlert', data);
    });

    // Allocation drift
    this.connection.on('AllocationDrift', (data) => {
      console.log('SignalR: Allocation drift detected', data);
      this.emit('AllocationDrift', data);
    });

    // System messages
    this.connection.on('SystemMessage', (data) => {
      console.log('SignalR: System message', data);
      this.emit('SystemMessage', data);
    });

    // Sync status updates
    this.connection.on('SyncStatusUpdated', (data) => {
      console.log('SignalR: Sync status updated', data);
      this.emit('SyncStatusUpdated', data);
    });
  }

  /**
   * Subscribe to client-specific updates
   */
  async subscribeToClient(clientId: string): Promise<void> {
    try {
      await this.connection?.invoke('SubscribeToClient', clientId);
      this.activeSubscriptions.add(`client:${clientId}`);
      console.log(`SignalR: Subscribed to client ${clientId}`);
    } catch (error) {
      console.error(`SignalR: Failed to subscribe to client ${clientId}`, error);
    }
  }

  /**
   * Unsubscribe from client-specific updates
   */
  async unsubscribeFromClient(clientId: string): Promise<void> {
    try {
      await this.connection?.invoke('UnsubscribeFromClient', clientId);
      this.activeSubscriptions.delete(`client:${clientId}`);
      console.log(`SignalR: Unsubscribed from client ${clientId}`);
    } catch (error) {
      console.error(`SignalR: Failed to unsubscribe from client ${clientId}`, error);
    }
  }

  /**
   * Subscribe to wallet-specific updates
   */
  async subscribeToWallet(walletId: string): Promise<void> {
    try {
      await this.connection?.invoke('SubscribeToWallet', walletId);
      this.activeSubscriptions.add(`wallet:${walletId}`);
      console.log(`SignalR: Subscribed to wallet ${walletId}`);
    } catch (error) {
      console.error(`SignalR: Failed to subscribe to wallet ${walletId}`, error);
    }
  }

  /**
   * Unsubscribe from wallet-specific updates
   */
  async unsubscribeFromWallet(walletId: string): Promise<void> {
    try {
      await this.connection?.invoke('UnsubscribeFromWallet', walletId);
      this.activeSubscriptions.delete(`wallet:${walletId}`);
      console.log(`SignalR: Unsubscribed from wallet ${walletId}`);
    } catch (error) {
      console.error(`SignalR: Failed to unsubscribe from wallet ${walletId}`, error);
    }
  }

  /**
   * Subscribe to account-specific updates
   */
  async subscribeToAccount(accountId: string): Promise<void> {
    try {
      await this.connection?.invoke('SubscribeToAccount', accountId);
      this.activeSubscriptions.add(`account:${accountId}`);
      console.log(`SignalR: Subscribed to account ${accountId}`);
    } catch (error) {
      console.error(`SignalR: Failed to subscribe to account ${accountId}`, error);
    }
  }

  /**
   * Unsubscribe from account-specific updates
   */
  async unsubscribeFromAccount(accountId: string): Promise<void> {
    try {
      await this.connection?.invoke('UnsubscribeFromAccount', accountId);
      this.activeSubscriptions.delete(`account:${accountId}`);
      console.log(`SignalR: Unsubscribed from account ${accountId}`);
    } catch (error) {
      console.error(`SignalR: Failed to unsubscribe from account ${accountId}`, error);
    }
  }

  /**
   * Subscribe to system-wide alerts
   */
  async subscribeToAlerts(): Promise<void> {
    try {
      await this.connection?.invoke('SubscribeToAlerts');
      this.activeSubscriptions.add('alerts');
      console.log('SignalR: Subscribed to alerts');
    } catch (error) {
      console.error('SignalR: Failed to subscribe to alerts', error);
    }
  }

  /**
   * Unsubscribe from system-wide alerts
   */
  async unsubscribeFromAlerts(): Promise<void> {
    try {
      await this.connection?.invoke('UnsubscribeFromAlerts');
      this.activeSubscriptions.delete('alerts');
      console.log('SignalR: Unsubscribed from alerts');
    } catch (error) {
      console.error('SignalR: Failed to unsubscribe from alerts', error);
    }
  }

  /**
   * Subscribe to a specific event
   * @returns Unsubscribe function
   */
  on(event: string, callback: (data: any) => void): () => void {
    if (!this.subscribers.has(event)) {
      this.subscribers.set(event, new Set());
    }
    this.subscribers.get(event)!.add(callback);

    // Return unsubscribe function
    return () => {
      this.subscribers.get(event)?.delete(callback);
      if (this.subscribers.get(event)?.size === 0) {
        this.subscribers.delete(event);
      }
    };
  }

  /**
   * Emit event to all subscribers
   */
  private emit(event: string, data: any): void {
    this.subscribers.get(event)?.forEach((callback) => {
      try {
        callback(data);
      } catch (error) {
        console.error(`SignalR: Error in event handler for ${event}`, error);
      }
    });
  }

  /**
   * Re-subscribe to all active subscriptions after reconnection
   */
  private async resubscribeAll(): Promise<void> {
    console.log('SignalR: Re-subscribing to all active subscriptions...');

    for (const sub of this.activeSubscriptions) {
      const [type, id] = sub.split(':');
      try {
        if (type === 'client') await this.subscribeToClient(id);
        else if (type === 'wallet') await this.subscribeToWallet(id);
        else if (type === 'account') await this.subscribeToAccount(id);
        else if (type === 'alerts') await this.subscribeToAlerts();
      } catch (error) {
        console.error(`SignalR: Failed to resubscribe to ${sub}`, error);
      }
    }
  }

  /**
   * Get current connection state
   */
  getState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  /**
   * Check if connected
   */
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Disconnect from SignalR hub
   */
  async disconnect(): Promise<void> {
    try {
      this.activeSubscriptions.clear();
      await this.connection?.stop();
      console.log('SignalR: Disconnected');
    } catch (error) {
      console.error('SignalR: Error during disconnect', error);
    }
  }
}

// Export singleton instance
export const signalRClient = new SignalRClient();
