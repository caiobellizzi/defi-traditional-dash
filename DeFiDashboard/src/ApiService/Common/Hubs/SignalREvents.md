# SignalR Events Documentation

## Overview

The DeFi-Traditional Finance Dashboard uses SignalR for real-time communication between the server and connected clients. This document describes all available events, subscription methods, and payload structures.

## Hub Endpoint

**URL**: `/hubs/dashboard`

**Full URL (Development)**: `https://localhost:7xxx/hubs/dashboard`

## Connection

### JavaScript/TypeScript Example

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7xxx/hubs/dashboard')
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

await connection.start();
console.log('Connected to SignalR hub');
```

## Subscription Methods

These are server-side methods that clients can invoke to subscribe/unsubscribe from specific event groups.

### Client Portfolio Updates

Subscribe to portfolio updates for a specific client:

```typescript
await connection.invoke('SubscribeToClient', clientId);
await connection.invoke('UnsubscribeFromClient', clientId);
```

**Parameters**:
- `clientId` (Guid): The client ID to subscribe to

**Events Received**:
- `PortfolioUpdated`
- `AllocationDrift`

---

### Wallet Balance Updates

Subscribe to balance updates for a specific wallet:

```typescript
await connection.invoke('SubscribeToWallet', walletId);
await connection.invoke('UnsubscribeFromWallet', walletId);
```

**Parameters**:
- `walletId` (Guid): The wallet ID to subscribe to

**Events Received**:
- `WalletBalanceUpdated`
- `NewTransaction`

---

### Account Balance Updates

Subscribe to balance updates for a specific traditional account:

```typescript
await connection.invoke('SubscribeToAccount', accountId);
await connection.invoke('UnsubscribeFromAccount', accountId);
```

**Parameters**:
- `accountId` (Guid): The account ID to subscribe to

**Events Received**:
- `AccountBalanceUpdated`
- `NewTransaction`

---

### Alerts

Subscribe to system-wide alerts:

```typescript
await connection.invoke('SubscribeToAlerts');
await connection.invoke('UnsubscribeFromAlerts');
```

**Events Received**:
- `NewAlert`
- `AlertStatusChanged`
- `AllocationDrift`

---

### Dashboard Metrics

Subscribe to global dashboard metrics:

```typescript
await connection.invoke('SubscribeToDashboard');
await connection.invoke('UnsubscribeFromDashboard');
```

**Events Received**:
- `DashboardMetricsUpdated`

---

## Events (Server to Client)

These events are sent from the server to subscribed clients.

### 1. PortfolioUpdated

**Event Name**: `PortfolioUpdated`

**Trigger**: Portfolio recalculation job completes or client allocation changes

**Subscription Required**: `SubscribeToClient(clientId)`

**Payload Structure**:

```typescript
{
  clientId: string;           // Client UUID
  timestamp: string;          // ISO 8601 timestamp
  data: {
    totalValueUsd: number;
    walletValueUsd: number;
    accountValueUsd: number;
    assets: Array<{
      assetId: string;
      assetType: 'Wallet' | 'Account';
      allocatedValue: number;
      currentValue: number;
    }>;
  };
}
```

**Example Handler**:

```typescript
connection.on('PortfolioUpdated', (payload) => {
  console.log('Portfolio updated for client:', payload.clientId);
  console.log('Total value:', payload.data.totalValueUsd);
  // Update UI with new portfolio data
});
```

---

### 2. WalletBalanceUpdated

**Event Name**: `WalletBalanceUpdated`

**Trigger**: Background job syncs wallet balances from blockchain

**Subscription Required**: `SubscribeToWallet(walletId)`

**Payload Structure**:

```typescript
{
  walletId: string;           // Wallet UUID
  timestamp: string;          // ISO 8601 timestamp
  data: {
    walletAddress: string;
    balances: Array<{
      chain: string;
      tokenSymbol: string;
      tokenName: string;
      balance: number;
      balanceUsd: number;
    }>;
    totalBalances: number;
  };
}
```

**Example Handler**:

```typescript
connection.on('WalletBalanceUpdated', (payload) => {
  console.log('Wallet balance updated:', payload.data.walletAddress);
  // Update wallet balance display
});
```

---

### 3. AccountBalanceUpdated

**Event Name**: `AccountBalanceUpdated`

**Trigger**: Background job syncs account balances from Pluggy

**Subscription Required**: `SubscribeToAccount(accountId)`

**Payload Structure**:

```typescript
{
  accountId: string;          // Account UUID
  timestamp: string;          // ISO 8601 timestamp
  data: {
    accountName: string;
    institution: string;
    balances: Array<{
      currency: string;
      available: number;
      current: number;
    }>;
    totalBalanceUsd: number;
  };
}
```

**Example Handler**:

```typescript
connection.on('AccountBalanceUpdated', (payload) => {
  console.log('Account balance updated:', payload.data.accountName);
  // Update account balance display
});
```

---

### 4. NewTransaction

**Event Name**: `NewTransaction`

**Trigger**: New transaction detected or manual transaction created

**Subscription Required**: `SubscribeToWallet(walletId)` OR `SubscribeToAccount(accountId)`

**Payload Structure**:

```typescript
{
  assetId: string;            // Wallet or Account UUID
  assetType: 'Wallet' | 'Account';
  timestamp: string;          // ISO 8601 timestamp
  data: {
    transactionId: string;
    transactionHash?: string;
    chain?: string;
    direction: 'Inbound' | 'Outbound';
    tokenSymbol: string;
    amount: number;
    amountUsd: number;
    transactionDate: string;
    category?: string;
    description?: string;
    isManualEntry: boolean;
  };
}
```

**Example Handler**:

```typescript
connection.on('NewTransaction', (payload) => {
  console.log('New transaction:', payload.data.transactionId);
  // Show notification toast
  // Refresh transaction list
});
```

---

### 5. NewAlert

**Event Name**: `NewAlert`

**Trigger**: Alert generation job creates a new alert

**Subscription Required**: `SubscribeToAlerts()`

**Payload Structure**:

```typescript
{
  timestamp: string;          // ISO 8601 timestamp
  data: {
    alertId: string;
    alertType: string;
    severity: 'Low' | 'Medium' | 'High' | 'Critical';
    title: string;
    message: string;
    relatedEntityType?: string;
    relatedEntityId?: string;
    metadata?: object;
  };
}
```

**Example Handler**:

```typescript
connection.on('NewAlert', (payload) => {
  console.log('New alert:', payload.data.title);
  // Show alert notification
  // Play sound for critical alerts
});
```

---

### 6. AlertStatusChanged

**Event Name**: `AlertStatusChanged`

**Trigger**: Alert is acknowledged or resolved

**Subscription Required**: `SubscribeToAlerts()`

**Payload Structure**:

```typescript
{
  alertId: string;
  newStatus: 'Acknowledged' | 'Resolved' | 'Dismissed';
  timestamp: string;          // ISO 8601 timestamp
  data: {
    alertType: string;
    severity: string;
    title: string;
    acknowledgedBy?: string;
    acknowledgedAt?: string;
    resolvedBy?: string;
    resolvedAt?: string;
  };
}
```

**Example Handler**:

```typescript
connection.on('AlertStatusChanged', (payload) => {
  console.log('Alert status changed:', payload.alertId, payload.newStatus);
  // Update alert list UI
});
```

---

### 7. AllocationDrift

**Event Name**: `AllocationDrift`

**Trigger**: Client's allocated percentage drifts beyond threshold

**Subscription Required**: `SubscribeToClient(clientId)` OR `SubscribeToAlerts()`

**Payload Structure**:

```typescript
{
  clientId: string;
  timestamp: string;          // ISO 8601 timestamp
  data: {
    assetId: string;
    assetType: 'Wallet' | 'Account';
    targetAllocation: number;
    currentAllocation: number;
    driftPercentage: number;
    recommendedAction: string;
  };
}
```

**Example Handler**:

```typescript
connection.on('AllocationDrift', (payload) => {
  console.log('Allocation drift detected for client:', payload.clientId);
  // Show rebalancing recommendation
});
```

---

### 8. SystemMessage

**Event Name**: `SystemMessage`

**Trigger**: System-wide announcements or notifications

**Subscription Required**: None (broadcast to all connected clients)

**Payload Structure**:

```typescript
{
  message: string;
  severity: 'info' | 'warning' | 'error' | 'success';
  timestamp: string;          // ISO 8601 timestamp
}
```

**Example Handler**:

```typescript
connection.on('SystemMessage', (payload) => {
  console.log('System message:', payload.message);
  // Show toast notification
});
```

---

### 9. DashboardMetricsUpdated

**Event Name**: `DashboardMetricsUpdated`

**Trigger**: Dashboard metrics recalculation completes

**Subscription Required**: `SubscribeToDashboard()`

**Payload Structure**:

```typescript
{
  timestamp: string;          // ISO 8601 timestamp
  data: {
    totalClientsActive: number;
    totalWallets: number;
    totalAccounts: number;
    totalValueUsd: number;
    totalProfitLoss: number;
    totalProfitLossPercentage: number;
    alertsCount: {
      critical: number;
      high: number;
      medium: number;
      low: number;
    };
  };
}
```

**Example Handler**:

```typescript
connection.on('DashboardMetricsUpdated', (payload) => {
  console.log('Dashboard metrics updated');
  // Update dashboard stats
});
```

---

## Integration Points

### Background Jobs

The following background jobs send SignalR notifications:

1. **WalletSyncJob** (Every 5 minutes)
   - Sends: `WalletBalanceUpdated`

2. **PluggySyncJob** (Every 10 minutes)
   - Sends: `AccountBalanceUpdated`

3. **PortfolioCalculationJob** (Every 15 minutes)
   - Sends: `PortfolioUpdated`

4. **AlertGenerationJob** (Every 5 minutes)
   - Sends: `NewAlert`, `AllocationDrift`

### Handlers

The following feature handlers send SignalR notifications:

1. **CreateManualTransactionHandler**
   - Sends: `NewTransaction`

2. **CreateAllocationHandler**
   - Sends: `PortfolioUpdated`

3. **AcknowledgeAlertHandler**
   - Sends: `AlertStatusChanged`

4. **ResolveAlertHandler**
   - Sends: `AlertStatusChanged`

---

## Error Handling

### Connection Errors

```typescript
connection.onclose((error) => {
  console.error('Connection closed:', error);
  // Attempt reconnection
});

connection.onreconnecting((error) => {
  console.warn('Reconnecting...', error);
  // Show reconnecting indicator
});

connection.onreconnected((connectionId) => {
  console.log('Reconnected:', connectionId);
  // Re-subscribe to all channels
  resubscribeAll();
});
```

### Server Errors

The server wraps all notifications in try-catch blocks and logs errors without throwing exceptions to clients.

---

## Testing SignalR

### Using Browser Console

```javascript
// Connect
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7xxx/hubs/dashboard')
  .build();

await connection.start();

// Subscribe
await connection.invoke('SubscribeToClient', 'client-uuid-here');

// Listen
connection.on('PortfolioUpdated', (data) => console.log(data));

// Unsubscribe
await connection.invoke('UnsubscribeFromClient', 'client-uuid-here');
```

### Using .NET SignalR Client (for testing)

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7xxx/hubs/dashboard")
    .Build();

connection.On<object>("PortfolioUpdated", (data) =>
{
    Console.WriteLine($"Received: {data}");
});

await connection.StartAsync();
await connection.InvokeAsync("SubscribeToClient", Guid.Parse("client-uuid"));
```

---

## Best Practices

1. **Always use automatic reconnection**:
   ```typescript
   .withAutomaticReconnect([0, 2000, 10000, 30000])
   ```

2. **Resubscribe on reconnection**:
   ```typescript
   connection.onreconnected(() => {
     // Resubscribe to all previous subscriptions
   });
   ```

3. **Handle all events defensively**:
   ```typescript
   connection.on('PortfolioUpdated', (payload) => {
     try {
       // Handle event
     } catch (error) {
       console.error('Error handling event:', error);
     }
   });
   ```

4. **Unsubscribe when component unmounts**:
   ```typescript
   useEffect(() => {
     connection.invoke('SubscribeToClient', clientId);
     return () => {
       connection.invoke('UnsubscribeFromClient', clientId);
     };
   }, [clientId]);
   ```

5. **Use TypeScript for type safety**:
   ```typescript
   interface PortfolioUpdatedPayload {
     clientId: string;
     timestamp: string;
     data: PortfolioData;
   }

   connection.on<PortfolioUpdatedPayload>('PortfolioUpdated', (payload) => {
     // TypeScript knows the payload structure
   });
   ```

---

## Security Considerations

1. **Authentication**: When authentication is implemented, clients must include a valid JWT token in the connection:
   ```typescript
   .withUrl('/hubs/dashboard', {
     accessTokenFactory: () => getAuthToken()
   })
   ```

2. **Authorization**: The hub should validate that clients can only subscribe to resources they have access to.

3. **Rate Limiting**: Consider implementing rate limiting on subscription methods to prevent abuse.

4. **Message Size**: Current max message size is 100KB. Large payloads should reference database records instead.

---

## Performance Tips

1. **Selective Subscription**: Only subscribe to resources currently visible to the user
2. **Debounce Updates**: If receiving frequent updates, debounce UI updates
3. **Pagination**: For large datasets, use pagination and only update visible items
4. **Batch Updates**: Group multiple updates into a single UI refresh

---

**Last Updated**: 2025-10-16
**Version**: 1.0.0
**SignalR Version**: ASP.NET Core SignalR 1.2.0
