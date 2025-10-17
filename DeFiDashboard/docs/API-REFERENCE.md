# API Reference

> **Complete REST API documentation for the DeFi-Traditional Finance Dashboard**

**Base URL**: `https://localhost:7185` (Development)
**API Version**: 1.0
**Last Updated**: 2025-10-16

---

## Table of Contents

- [Authentication](#authentication)
- [Rate Limiting](#rate-limiting)
- [Error Handling](#error-handling)
- [Real-Time Updates (SignalR)](#real-time-updates-signalr)
- [Clients API](#clients-api)
- [Wallets API](#wallets-api)
- [Accounts API](#accounts-api)
- [Allocations API](#allocations-api)
- [Transactions API](#transactions-api)
- [Portfolio API](#portfolio-api)
- [Analytics API](#analytics-api)
- [Alerts API](#alerts-api)
- [Export API](#export-api)
- [System API](#system-api)

---

## Authentication

**Current Status**: No authentication required (development mode)

**Planned**: Azure AD B2C integration in v1.1
- Bearer token authentication
- Role-based access control (Admin, Advisor, Read-Only)
- Client portal access

**Request Headers** (when authentication is enabled):
```http
Authorization: Bearer {access_token}
Content-Type: application/json
```

---

## Rate Limiting

The API implements rate limiting to prevent abuse and ensure fair usage.

### Global Limits

- **100 requests per minute** per IP address
- HTTP Status: `429 Too Many Requests`
- Retry-After header included in response

### Endpoint-Specific Limits

| Endpoint Type | Limit | Window |
|--------------|-------|---------|
| **Read Operations** (GET) | 200 requests | 1 minute |
| **Write Operations** (POST/PUT/DELETE) | 50 requests | 1 minute |
| **Export Operations** | 5 requests | 10 minutes |

### Rate Limit Response

```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Please try again later.",
    "retryAfter": 60.0
  }
}
```

### Rate Limit Headers

```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 42
X-RateLimit-Reset: 1729045920
```

---

## Error Handling

### Standard Error Response

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      "field": "Additional context"
    }
  }
}
```

### HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| `200 OK` | Request successful |
| `201 Created` | Resource created successfully |
| `204 No Content` | Request successful, no content to return |
| `400 Bad Request` | Invalid request parameters |
| `404 Not Found` | Resource not found |
| `409 Conflict` | Resource conflict (e.g., duplicate email) |
| `422 Unprocessable Entity` | Validation failed |
| `429 Too Many Requests` | Rate limit exceeded |
| `500 Internal Server Error` | Server error |

### Common Error Codes

| Code | Description |
|------|-------------|
| `VALIDATION_ERROR` | Request validation failed |
| `NOT_FOUND` | Resource not found |
| `DUPLICATE_EMAIL` | Email already exists |
| `ALLOCATION_EXCEEDS_100` | Total allocations exceed 100% |
| `RATE_LIMIT_EXCEEDED` | Too many requests |
| `EXTERNAL_PROVIDER_ERROR` | Moralis/Pluggy API error |

### Validation Error Example

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": {
      "Name": ["Name is required"],
      "Email": ["Invalid email format"]
    }
  }
}
```

---

## Real-Time Updates (SignalR)

The API provides real-time updates via SignalR WebSocket connections.

### Connection

**Hub URL**: `https://localhost:7185/hubs/dashboard`

**Client Libraries**:
- JavaScript/TypeScript: `@microsoft/signalr`
- .NET: `Microsoft.AspNetCore.SignalR.Client`

### Example Connection (TypeScript)

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7185/hubs/dashboard')
  .withAutomaticReconnect()
  .build();

// Start connection
await connection.start();

// Subscribe to events
connection.on('WalletBalanceUpdated', (data) => {
  console.log('Wallet updated:', data);
});

connection.on('AlertCreated', (alert) => {
  console.log('New alert:', alert);
});
```

### Real-Time Events

| Event | Trigger | Payload |
|-------|---------|---------|
| `WalletBalanceUpdated` | Wallet sync completed | `{ walletId, balances }` |
| `AccountBalanceUpdated` | Account sync completed | `{ accountId, balance }` |
| `PortfolioRecalculated` | Portfolio calculation done | `{ clientId, totalValue }` |
| `AlertCreated` | New alert generated | `{ alertId, type, message }` |
| `ExportReady` | Export job completed | `{ exportId, downloadUrl }` |
| `TransactionDetected` | New transaction found | `{ transactionId, type }` |

### Subscribe to Client-Specific Updates

```typescript
// Subscribe to specific client's portfolio updates
await connection.invoke('SubscribeToClient', clientId);

// Unsubscribe
await connection.invoke('UnsubscribeFromClient', clientId);
```

---

## Clients API

Manage client profiles and portfolios.

### List Clients

Retrieve a paginated list of clients with optional filtering and sorting.

```http
GET /api/clients?pageNumber=1&pageSize=50&status=Active&sortBy=Name&sortDescending=false
```

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 50, max: 100)
- `status` (string, optional): Filter by status (`Active`, `Inactive`, `Suspended`)
- `search` (string, optional): Search by name or email
- `sortBy` (string, optional): Sort field (`Name`, `Email`, `CreatedAt`)
- `sortDescending` (boolean, optional): Sort direction (default: false)

**Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "John Doe",
      "email": "john.doe@example.com",
      "document": "123.456.789-00",
      "phoneNumber": "+1 (555) 123-4567",
      "status": "Active",
      "notes": "VIP client - high net worth individual",
      "createdAt": "2025-10-16T00:00:00Z",
      "updatedAt": "2025-10-16T00:00:00Z"
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

**Rate Limit**: ClientsRead policy (200 requests/minute)

---

### Get Client By ID

Retrieve detailed information about a specific client.

```http
GET /api/clients/{id}
```

**Path Parameters**:
- `id` (uuid, required): Client ID

**Response** (`200 OK`):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "document": "123.456.789-00",
  "phoneNumber": "+1 (555) 123-4567",
  "status": "Active",
  "notes": "VIP client - high net worth individual",
  "createdAt": "2025-10-16T00:00:00Z",
  "updatedAt": "2025-10-16T00:00:00Z"
}
```

**Errors**:
- `404 Not Found`: Client not found

**cURL Example**:
```bash
curl -X GET "https://localhost:7185/api/clients/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "accept: application/json"
```

---

### Create Client

Create a new client profile.

```http
POST /api/clients
```

**Request Body**:
```json
{
  "name": "Jane Smith",
  "email": "jane.smith@example.com",
  "document": "987.654.321-00",
  "phoneNumber": "+1 (555) 987-6543",
  "notes": "Corporate account manager"
}
```

**Validation Rules**:
- `name`: Required, max 200 characters
- `email`: Required, valid email format, unique, max 200 characters
- `document`: Optional, max 50 characters
- `phoneNumber`: Optional, max 20 characters
- `notes`: Optional, HTML tags sanitized for XSS protection

**Response** (`201 Created`):
```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**Location Header**: `/api/clients/3fa85f64-5717-4562-b3fc-2c963f66afa6`

**Errors**:
- `400 Bad Request`: Validation failed
- `409 Conflict`: Email already exists

**Rate Limit**: ClientsWrite policy (50 requests/minute)

**cURL Example**:
```bash
curl -X POST "https://localhost:7185/api/clients" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Smith",
    "email": "jane.smith@example.com",
    "document": "987.654.321-00",
    "phoneNumber": "+1 (555) 987-6543",
    "notes": "Corporate account manager"
  }'
```

---

### Update Client

Update an existing client's information.

```http
PUT /api/clients/{id}
```

**Path Parameters**:
- `id` (uuid, required): Client ID

**Request Body**:
```json
{
  "name": "Jane Smith Updated",
  "email": "jane.smith@example.com",
  "document": "987.654.321-00",
  "phoneNumber": "+1 (555) 987-6543",
  "notes": "Updated notes",
  "status": "Active"
}
```

**Fields**:
- `name`: Required, max 200 characters
- `email`: Required, valid email format, max 200 characters
- `document`: Optional, max 50 characters
- `phoneNumber`: Optional, max 20 characters
- `notes`: Optional, HTML sanitized
- `status`: Optional (null = no change), values: `Active`, `Inactive`, `Suspended`

**Response** (`200 OK`):
```json
true
```

**Errors**:
- `404 Not Found`: Client not found
- `400 Bad Request`: Validation failed
- `409 Conflict`: Email already exists (if changed)

**Rate Limit**: ClientsWrite policy (50 requests/minute)

---

### Delete Client

Soft delete a client (marks as inactive, preserves data).

```http
DELETE /api/clients/{id}
```

**Path Parameters**:
- `id` (uuid, required): Client ID

**Response** (`204 No Content`): No body

**Errors**:
- `404 Not Found`: Client not found

**Note**: This is a soft delete. Client data is preserved but marked as deleted. Associated allocations are ended automatically.

**Rate Limit**: ClientsWrite policy (50 requests/minute)

---

### Get Client Portfolio

Retrieve comprehensive portfolio information for a specific client.

```http
GET /api/clients/{id}/portfolio
```

**Path Parameters**:
- `id` (uuid, required): Client ID

**Response** (`200 OK`):
```json
{
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clientName": "John Doe",
  "totalValueUsd": 125000.50,
  "cryptoValueUsd": 75000.30,
  "traditionalValueUsd": 50000.20,
  "lastUpdated": "2025-10-16T12:00:00Z",
  "allocations": [
    {
      "assetType": "Wallet",
      "assetId": "wallet-123",
      "assetName": "Main ETH Wallet",
      "allocationType": "Percentage",
      "allocationValue": 30.0,
      "currentBalanceUsd": 33333.33,
      "clientShareUsd": 10000.00
    }
  ],
  "performanceMetrics": {
    "roi": 15.5,
    "profitLoss": 16700.00,
    "period": "30days"
  }
}
```

**Errors**:
- `404 Not Found`: Client not found

**Performance**: Optimized with AsNoTracking() for read-only queries

---

## Wallets API

Manage cryptocurrency wallets and monitor blockchain assets.

### List Wallets

Retrieve all custody wallets with their current status.

```http
GET /api/wallets?status=Active&blockchain=ethereum
```

**Query Parameters**:
- `status` (string, optional): Filter by status (`Active`, `Inactive`)
- `blockchain` (string, optional): Filter by blockchain (e.g., `ethereum`, `polygon`)
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 50)

**Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "wallet-123",
      "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
      "label": "Main ETH Wallet",
      "blockchainProvider": "Moralis",
      "supportedChains": ["ethereum", "polygon", "bsc"],
      "status": "Active",
      "notes": "Primary custody wallet",
      "lastSyncedAt": "2025-10-16T12:00:00Z",
      "createdAt": "2025-10-01T00:00:00Z"
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

---

### Get Wallet By ID

Retrieve detailed information about a specific wallet.

```http
GET /api/wallets/{id}
```

**Response** (`200 OK`):
```json
{
  "id": "wallet-123",
  "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "label": "Main ETH Wallet",
  "blockchainProvider": "Moralis",
  "supportedChains": ["ethereum", "polygon", "bsc"],
  "status": "Active",
  "notes": "Primary custody wallet",
  "lastSyncedAt": "2025-10-16T12:00:00Z",
  "createdAt": "2025-10-01T00:00:00Z",
  "updatedAt": "2025-10-16T12:00:00Z"
}
```

---

### Add Wallet

Add a new custody wallet for monitoring.

```http
POST /api/wallets
```

**Request Body**:
```json
{
  "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "label": "DeFi Operations Wallet",
  "supportedChains": ["ethereum", "arbitrum"],
  "notes": "Used for yield farming and staking"
}
```

**Validation Rules**:
- `walletAddress`: Required, valid Ethereum address format (40 hex characters), unique
- `label`: Optional, max 200 characters, HTML sanitized
- `supportedChains`: Required, array of blockchain names
- `notes`: Optional, HTML sanitized for XSS protection

**Response** (`201 Created`):
```json
"wallet-456"
```

**Errors**:
- `400 Bad Request`: Invalid wallet address format
- `409 Conflict`: Wallet address already exists

**Note**: After creation, the wallet is automatically synced via Hangfire job within 5 minutes.

---

### Update Wallet

Update wallet label, supported chains, or notes.

```http
PUT /api/wallets/{id}
```

**Request Body**:
```json
{
  "label": "Updated Label",
  "supportedChains": ["ethereum", "polygon", "bsc"],
  "notes": "Updated notes",
  "status": "Active"
}
```

**Response** (`200 OK`):
```json
true
```

---

### Delete Wallet

Remove a wallet from monitoring (ends all client allocations).

```http
DELETE /api/wallets/{id}
```

**Response** (`204 No Content`): No body

**Warning**: This ends all client allocations to this wallet and stops monitoring.

---

### Get Wallet Balances

Retrieve current token balances for a wallet across all supported chains.

```http
GET /api/wallets/{id}/balances
```

**Response** (`200 OK`):
```json
{
  "walletId": "wallet-123",
  "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "totalValueUsd": 45000.00,
  "balances": [
    {
      "chain": "ethereum",
      "tokenAddress": "0x0000000000000000000000000000000000000000",
      "tokenSymbol": "ETH",
      "tokenName": "Ethereum",
      "balance": "10.5",
      "balanceFormatted": "10.5 ETH",
      "usdValue": 35000.00,
      "priceUsd": 3333.33,
      "lastUpdated": "2025-10-16T12:00:00Z"
    },
    {
      "chain": "polygon",
      "tokenAddress": "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
      "tokenSymbol": "USDC",
      "tokenName": "USD Coin",
      "balance": "10000",
      "balanceFormatted": "10,000 USDC",
      "usdValue": 10000.00,
      "priceUsd": 1.00,
      "lastUpdated": "2025-10-16T12:00:00Z"
    }
  ],
  "lastSyncedAt": "2025-10-16T12:00:00Z"
}
```

**Data Source**: Moralis API (cached, updates every 5 minutes via background job)

---

### Get Wallet Transactions

Retrieve transaction history for a wallet.

```http
GET /api/wallets/{id}/transactions?fromDate=2025-10-01&toDate=2025-10-16&chain=ethereum&pageNumber=1&pageSize=50
```

**Query Parameters**:
- `fromDate` (datetime, optional): Start date (ISO 8601)
- `toDate` (datetime, optional): End date (ISO 8601)
- `chain` (string, optional): Filter by blockchain
- `transactionType` (string, optional): Filter by type (`Transfer`, `Swap`, `Unknown`)
- `pageNumber` (integer, optional): Page number
- `pageSize` (integer, optional): Items per page (max: 100)

**Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "tx-123",
      "transactionHash": "0xabc123...",
      "chain": "ethereum",
      "fromAddress": "0x123...",
      "toAddress": "0x742d35...",
      "tokenSymbol": "ETH",
      "amount": "1.5",
      "amountUsd": 5000.00,
      "transactionType": "Transfer",
      "timestamp": "2025-10-16T10:30:00Z",
      "blockNumber": 18500000
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 3
}
```

---

### Sync Wallet

Manually trigger a wallet balance sync (bypasses scheduled job).

```http
POST /api/wallets/{id}/sync
```

**Response** (`202 Accepted`):
```json
{
  "message": "Wallet sync initiated",
  "jobId": "hangfire-job-123",
  "estimatedCompletion": "2025-10-16T12:05:00Z"
}
```

**Note**: Sync typically completes in 10-30 seconds. Results available via SignalR event `WalletBalanceUpdated`.

---

### Get Wallet History

Retrieve historical balance snapshots for analytics.

```http
GET /api/wallets/{id}/history?fromDate=2025-10-01&toDate=2025-10-16&interval=daily
```

**Query Parameters**:
- `fromDate` (datetime, required): Start date
- `toDate` (datetime, required): End date
- `interval` (string, optional): Snapshot interval (`hourly`, `daily`, `weekly`) (default: `daily`)

**Response** (`200 OK`):
```json
{
  "walletId": "wallet-123",
  "interval": "daily",
  "snapshots": [
    {
      "date": "2025-10-01T00:00:00Z",
      "totalValueUsd": 40000.00,
      "balances": {
        "ETH": 12.0,
        "USDC": 10000.0
      }
    },
    {
      "date": "2025-10-02T00:00:00Z",
      "totalValueUsd": 42000.00,
      "balances": {
        "ETH": 12.5,
        "USDC": 10000.0
      }
    }
  ]
}
```

---

## Allocations API

Manage client allocations to custody assets.

### List Allocations

Retrieve all active allocations with optional filtering.

```http
GET /api/allocations?clientId={clientId}&assetType=Wallet&status=Active
```

**Query Parameters**:
- `clientId` (uuid, optional): Filter by client
- `assetType` (string, optional): Filter by asset type (`Wallet`, `Account`)
- `assetId` (uuid, optional): Filter by specific asset
- `status` (string, optional): Filter by status (`Active`, `Ended`)
- `pageNumber` (integer, optional): Page number
- `pageSize` (integer, optional): Items per page

**Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "alloc-123",
      "clientId": "client-456",
      "clientName": "John Doe",
      "assetType": "Wallet",
      "assetId": "wallet-789",
      "assetName": "Main ETH Wallet",
      "allocationType": "Percentage",
      "allocationValue": 30.0,
      "startDate": "2025-10-01",
      "endDate": null,
      "notes": "30% allocation",
      "createdAt": "2025-10-01T00:00:00Z"
    }
  ],
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

---

### Get Allocation By ID

Retrieve detailed information about a specific allocation.

```http
GET /api/allocations/{id}
```

**Response** (`200 OK`):
```json
{
  "id": "alloc-123",
  "clientId": "client-456",
  "clientName": "John Doe",
  "assetType": "Wallet",
  "assetId": "wallet-789",
  "assetName": "Main ETH Wallet",
  "allocationType": "Percentage",
  "allocationValue": 30.0,
  "startDate": "2025-10-01",
  "endDate": null,
  "notes": "30% allocation",
  "currentValueUsd": 10000.00,
  "createdAt": "2025-10-01T00:00:00Z",
  "updatedAt": "2025-10-01T00:00:00Z"
}
```

---

### Create Allocation

Create a new client allocation to a custody asset.

```http
POST /api/allocations
```

**Request Body**:
```json
{
  "clientId": "client-456",
  "assetType": "Wallet",
  "assetId": "wallet-789",
  "allocationType": "Percentage",
  "allocationValue": 30.0,
  "startDate": "2025-10-01",
  "notes": "30% allocation of Main ETH Wallet"
}
```

**Validation Rules**:
- `clientId`: Required, must exist
- `assetType`: Required, values: `Wallet`, `Account`
- `assetId`: Required, asset must exist
- `allocationType`: Required, values: `Percentage`, `FixedAmount`
- `allocationValue`: Required
  - For `Percentage`: 0-100 (total percentage allocations per asset cannot exceed 100%)
  - For `FixedAmount`: > 0
- `startDate`: Required, ISO 8601 date
- `notes`: Optional, HTML sanitized

**Response** (`201 Created`):
```json
"alloc-123"
```

**Errors**:
- `400 Bad Request`: Validation failed
- `404 Not Found`: Client or asset not found
- `409 Conflict`: Total allocations would exceed 100%

**Example Error - Allocation Exceeds 100%**:
```json
{
  "error": {
    "code": "ALLOCATION_EXCEEDS_100",
    "message": "Total percentage allocations for this asset would exceed 100%",
    "details": {
      "currentTotal": 85.0,
      "attemptedAllocation": 30.0,
      "available": 15.0
    }
  }
}
```

---

### Update Allocation

Update an existing allocation's value or notes.

```http
PUT /api/allocations/{id}
```

**Request Body**:
```json
{
  "allocationValue": 35.0,
  "notes": "Updated to 35% allocation"
}
```

**Response** (`200 OK`):
```json
true
```

**Errors**:
- `404 Not Found`: Allocation not found
- `409 Conflict`: New value would cause total to exceed 100%

---

### End Allocation

End an active allocation (sets end date to today).

```http
POST /api/allocations/{id}/end
```

**Response** (`200 OK`):
```json
true
```

**Note**: Once ended, an allocation cannot be reactivated. Create a new allocation instead.

---

### Delete Allocation

Permanently delete an allocation (only if not yet started or already ended).

```http
DELETE /api/allocations/{id}
```

**Response** (`204 No Content`): No body

**Errors**:
- `409 Conflict`: Cannot delete active allocation (end it first)

---

### Get Client Allocations

Retrieve all allocations for a specific client.

```http
GET /api/clients/{clientId}/allocations?status=Active
```

**Query Parameters**:
- `status` (string, optional): Filter by status (`Active`, `Ended`, `All`)

**Response** (`200 OK`):
```json
{
  "clientId": "client-456",
  "clientName": "John Doe",
  "allocations": [
    {
      "id": "alloc-123",
      "assetType": "Wallet",
      "assetId": "wallet-789",
      "assetName": "Main ETH Wallet",
      "allocationType": "Percentage",
      "allocationValue": 30.0,
      "currentValueUsd": 10000.00,
      "status": "Active",
      "startDate": "2025-10-01",
      "endDate": null
    }
  ],
  "totalAllocations": 5,
  "activeAllocations": 3,
  "totalCurrentValueUsd": 125000.00
}
```

---

### Validate Allocation

Validate an allocation before creating it (dry-run).

```http
POST /api/allocations/validate
```

**Request Body**: Same as Create Allocation

**Response** (`200 OK`):
```json
{
  "isValid": true,
  "message": "Allocation is valid",
  "availablePercentage": 15.0,
  "currentTotalPercentage": 85.0
}
```

**Or if invalid**:
```json
{
  "isValid": false,
  "message": "Total allocations would exceed 100%",
  "errors": [
    "Current allocations: 85%. Attempted: 30%. Available: 15%."
  ]
}
```

---

### Get Allocation Conflicts

Check for allocation conflicts on a specific asset.

```http
GET /api/allocations/conflicts?assetType=Wallet&assetId=wallet-789
```

**Response** (`200 OK`):
```json
{
  "assetType": "Wallet",
  "assetId": "wallet-789",
  "assetName": "Main ETH Wallet",
  "totalPercentageAllocated": 95.0,
  "availablePercentage": 5.0,
  "activeAllocations": [
    {
      "clientId": "client-123",
      "clientName": "John Doe",
      "allocationValue": 30.0
    },
    {
      "clientId": "client-456",
      "clientName": "Jane Smith",
      "allocationValue": 65.0
    }
  ],
  "hasConflict": false,
  "conflictDetails": null
}
```

---

## Transactions API

Track and manage transactions across all custody assets.

### List Transactions

Retrieve transactions with filtering and pagination.

```http
GET /api/transactions?fromDate=2025-10-01&toDate=2025-10-16&assetType=Wallet&transactionType=Transfer&pageNumber=1&pageSize=50
```

**Query Parameters**:
- `fromDate` (datetime, optional): Start date
- `toDate` (datetime, optional): End date
- `assetType` (string, optional): Filter by asset type (`Wallet`, `Account`)
- `assetId` (uuid, optional): Filter by specific asset
- `transactionType` (string, optional): Filter by type
- `minAmount` (decimal, optional): Minimum amount USD
- `maxAmount` (decimal, optional): Maximum amount USD
- `search` (string, optional): Search in description or hash
- `pageNumber` (integer, optional): Page number
- `pageSize` (integer, optional): Items per page

**Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "tx-123",
      "assetType": "Wallet",
      "assetId": "wallet-789",
      "assetName": "Main ETH Wallet",
      "transactionType": "Transfer",
      "description": "Received 1.5 ETH",
      "amountUsd": 5000.00,
      "transactionDate": "2025-10-16T10:30:00Z",
      "externalReference": "0xabc123...",
      "createdAt": "2025-10-16T10:35:00Z"
    }
  ],
  "totalCount": 250,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 5
}
```

**Performance**: Optimized with AsNoTracking() and database index on (asset_id, transaction_date)

---

### Get Transaction By ID

Retrieve detailed information about a specific transaction.

```http
GET /api/transactions/{id}
```

**Response** (`200 OK`):
```json
{
  "id": "tx-123",
  "assetType": "Wallet",
  "assetId": "wallet-789",
  "assetName": "Main ETH Wallet",
  "transactionType": "Transfer",
  "description": "Received 1.5 ETH from DeFi protocol",
  "amount": 1.5,
  "tokenSymbol": "ETH",
  "amountUsd": 5000.00,
  "priceUsd": 3333.33,
  "transactionDate": "2025-10-16T10:30:00Z",
  "externalReference": "0xabc123...",
  "blockchain": "ethereum",
  "fromAddress": "0x123...",
  "toAddress": "0x742d35...",
  "blockNumber": 18500000,
  "gasUsed": "21000",
  "gasPriceGwei": "30",
  "status": "Confirmed",
  "createdAt": "2025-10-16T10:35:00Z",
  "updatedAt": "2025-10-16T10:35:00Z"
}
```

---

### Create Manual Transaction

Create a manual transaction entry (for off-chain operations).

```http
POST /api/transactions/manual
```

**Request Body**:
```json
{
  "assetType": "Wallet",
  "assetId": "wallet-789",
  "transactionType": "Transfer",
  "description": "Manual entry: Client withdrawal",
  "amount": 1000.00,
  "tokenSymbol": "USDC",
  "amountUsd": 1000.00,
  "transactionDate": "2025-10-16T10:00:00Z",
  "externalReference": "invoice-12345"
}
```

**Validation Rules**:
- `assetType`: Required, values: `Wallet`, `Account`
- `assetId`: Required, asset must exist
- `transactionType`: Required
- `description`: Required, max 500 characters
- `amount`: Required, > 0
- `amountUsd`: Required, > 0
- `transactionDate`: Required

**Response** (`201 Created`):
```json
"tx-456"
```

---

### Update Transaction

Update a transaction's description or metadata.

```http
PUT /api/transactions/{id}
```

**Request Body**:
```json
{
  "description": "Updated description",
  "transactionType": "Swap"
}
```

**Response** (`200 OK`):
```json
true
```

**Note**: Only manual transactions and transaction metadata can be updated. Blockchain transactions are immutable.

---

### Delete Transaction

Delete a manual transaction entry.

```http
DELETE /api/transactions/{id}
```

**Response** (`204 No Content`): No body

**Errors**:
- `409 Conflict`: Cannot delete synced blockchain transaction

---

### Bulk Import Transactions

Import multiple transactions from CSV/Excel.

```http
POST /api/transactions/bulk-import
Content-Type: multipart/form-data
```

**Form Data**:
- `file`: CSV/Excel file with columns: assetId, transactionType, description, amount, tokenSymbol, amountUsd, transactionDate

**Response** (`202 Accepted`):
```json
{
  "jobId": "import-job-123",
  "message": "Import job created",
  "estimatedCompletion": "2025-10-16T12:10:00Z"
}
```

**CSV Format Example**:
```csv
assetId,transactionType,description,amount,tokenSymbol,amountUsd,transactionDate
wallet-789,Transfer,Received from client,100,USDC,100.00,2025-10-16T10:00:00Z
```

---

### Get Transaction Audit Trail

Retrieve audit trail for a transaction (all changes).

```http
GET /api/transactions/{id}/audit
```

**Response** (`200 OK`):
```json
{
  "transactionId": "tx-123",
  "auditEntries": [
    {
      "id": "audit-1",
      "action": "Created",
      "performedBy": "system",
      "performedAt": "2025-10-16T10:35:00Z",
      "changes": {
        "description": {
          "oldValue": null,
          "newValue": "Received 1.5 ETH"
        }
      }
    },
    {
      "id": "audit-2",
      "action": "Updated",
      "performedBy": "admin-user-123",
      "performedAt": "2025-10-16T11:00:00Z",
      "changes": {
        "description": {
          "oldValue": "Received 1.5 ETH",
          "newValue": "Received 1.5 ETH from DeFi protocol"
        }
      }
    }
  ]
}
```

---

## Portfolio API

Access consolidated and client-specific portfolio data.

### Get Portfolio Overview

Retrieve high-level portfolio metrics across all clients.

```http
GET /api/portfolio/overview
```

**Response** (`200 OK`):
```json
{
  "totalClientsCount": 50,
  "totalAssetsUnderManagement": 5000000.00,
  "cryptoAum": 3000000.00,
  "traditionalAum": 2000000.00,
  "topAssets": [
    {
      "assetType": "Wallet",
      "assetName": "Main ETH Wallet",
      "valueUsd": 500000.00,
      "percentageOfTotal": 10.0
    }
  ],
  "performanceSummary": {
    "avgRoi30Days": 8.5,
    "totalProfitLoss30Days": 425000.00
  },
  "lastUpdated": "2025-10-16T12:00:00Z"
}
```

---

### Get Consolidated Portfolio

Retrieve detailed portfolio with all allocations and balances.

```http
GET /api/portfolio/consolidated?includeEnded=false
```

**Query Parameters**:
- `includeEnded` (boolean, optional): Include ended allocations (default: false)

**Response** (`200 OK`):
```json
{
  "totalValueUsd": 5000000.00,
  "cryptoValueUsd": 3000000.00,
  "traditionalValueUsd": 2000000.00,
  "assetBreakdown": [
    {
      "assetType": "Wallet",
      "assetId": "wallet-789",
      "assetName": "Main ETH Wallet",
      "totalValueUsd": 500000.00,
      "allocatedPercentage": 100.0,
      "clientAllocations": [
        {
          "clientId": "client-123",
          "clientName": "John Doe",
          "allocationPercentage": 30.0,
          "valueUsd": 150000.00
        }
      ]
    }
  ],
  "unallocatedAssets": [],
  "lastCalculated": "2025-10-16T12:00:00Z"
}
```

---

### Recalculate Portfolio

Manually trigger portfolio recalculation for all clients.

```http
POST /api/portfolio/recalculate
```

**Response** (`202 Accepted`):
```json
{
  "message": "Portfolio recalculation initiated",
  "jobId": "calc-job-123",
  "estimatedCompletion": "2025-10-16T12:05:00Z"
}
```

**Note**: Results broadcast via SignalR event `PortfolioRecalculated` when complete.

---

### Get Portfolio Snapshot

Get a point-in-time snapshot of the entire portfolio.

```http
GET /api/portfolio/snapshot?date=2025-10-16T12:00:00Z
```

**Query Parameters**:
- `date` (datetime, optional): Snapshot date (default: now)

**Response** (`200 OK`):
```json
{
  "snapshotDate": "2025-10-16T12:00:00Z",
  "totalValueUsd": 5000000.00,
  "clientPortfolios": [
    {
      "clientId": "client-123",
      "clientName": "John Doe",
      "valueUsd": 125000.00
    }
  ]
}
```

---

### Get Portfolio Composition

Analyze portfolio composition by asset type, blockchain, or token.

```http
GET /api/portfolio/composition?groupBy=assetType
```

**Query Parameters**:
- `groupBy` (string, required): Grouping method (`assetType`, `blockchain`, `tokenSymbol`)

**Response** (`200 OK`):
```json
{
  "composition": [
    {
      "category": "Wallet",
      "valueUsd": 3000000.00,
      "percentage": 60.0,
      "count": 15
    },
    {
      "category": "Account",
      "valueUsd": 2000000.00,
      "percentage": 40.0,
      "count": 25
    }
  ],
  "totalValueUsd": 5000000.00,
  "asOfDate": "2025-10-16T12:00:00Z"
}
```

---

## Analytics API

Advanced analytics and performance metrics.

### Get Performance Metrics

Retrieve performance metrics for a specific period.

```http
GET /api/analytics/performance?clientId=client-123&period=30days
```

**Query Parameters**:
- `clientId` (uuid, optional): Specific client (omit for all clients)
- `period` (string, required): Time period (`7days`, `30days`, `90days`, `1year`, `all`)

**Response** (`200 OK`):
```json
{
  "period": "30days",
  "startDate": "2025-09-16T00:00:00Z",
  "endDate": "2025-10-16T00:00:00Z",
  "startingValueUsd": 110000.00,
  "endingValueUsd": 125000.00,
  "profitLoss": 15000.00,
  "roi": 13.64,
  "dailyReturns": [
    {
      "date": "2025-09-16",
      "valueUsd": 110000.00,
      "dailyReturn": 0.0
    },
    {
      "date": "2025-09-17",
      "valueUsd": 111000.00,
      "dailyReturn": 0.91
    }
  ],
  "volatility": 2.5,
  "sharpeRatio": 1.8
}
```

---

### Get Allocation Drift

Detect allocation drift and rebalancing opportunities.

```http
GET /api/analytics/allocation-drift?threshold=5.0
```

**Query Parameters**:
- `threshold` (decimal, optional): Drift threshold percentage (default: 5.0)

**Response** (`200 OK`):
```json
{
  "driftAnalysis": [
    {
      "clientId": "client-123",
      "clientName": "John Doe",
      "assetId": "wallet-789",
      "assetName": "Main ETH Wallet",
      "targetAllocation": 30.0,
      "currentAllocation": 35.5,
      "drift": 5.5,
      "driftPercentage": 18.33,
      "requiresRebalancing": true,
      "recommendedAction": "Reduce allocation by 5.5% or reallocate $5,500"
    }
  ],
  "totalDriftDetected": 5,
  "averageDrift": 6.2
}
```

---

### Get Historical Performance

Retrieve historical performance data for charting.

```http
GET /api/analytics/historical?clientId=client-123&fromDate=2025-01-01&toDate=2025-10-16&interval=daily
```

**Query Parameters**:
- `clientId` (uuid, optional): Specific client
- `fromDate` (datetime, required): Start date
- `toDate` (datetime, required): End date
- `interval` (string, optional): Data point interval (`hourly`, `daily`, `weekly`) (default: `daily`)

**Response** (`200 OK`):
```json
{
  "clientId": "client-123",
  "clientName": "John Doe",
  "interval": "daily",
  "dataPoints": [
    {
      "date": "2025-01-01T00:00:00Z",
      "totalValueUsd": 100000.00,
      "cryptoValueUsd": 60000.00,
      "traditionalValueUsd": 40000.00
    }
  ],
  "summary": {
    "startValue": 100000.00,
    "endValue": 125000.00,
    "totalReturn": 25.0,
    "maxValue": 130000.00,
    "minValue": 95000.00
  }
}
```

---

### Get Risk Metrics

Calculate risk-adjusted metrics for portfolios.

```http
GET /api/analytics/risk?clientId=client-123&period=90days
```

**Response** (`200 OK`):
```json
{
  "clientId": "client-123",
  "period": "90days",
  "volatility": 15.5,
  "sharpeRatio": 1.8,
  "maxDrawdown": -12.3,
  "beta": 1.05,
  "alpha": 2.3,
  "valueAtRisk95": -5000.00,
  "diversificationScore": 7.5,
  "concentrationRisk": "Medium"
}
```

---

### Get Correlation Analysis

Analyze correlation between different assets.

```http
GET /api/analytics/correlation?period=90days
```

**Response** (`200 OK`):
```json
{
  "period": "90days",
  "correlationMatrix": [
    {
      "asset1": "ETH",
      "asset2": "BTC",
      "correlation": 0.85
    },
    {
      "asset1": "ETH",
      "asset2": "Stocks",
      "correlation": 0.45
    }
  ],
  "insights": [
    "High correlation between crypto assets suggests concentrated risk",
    "Low correlation with traditional assets provides diversification benefit"
  ]
}
```

---

## Alerts API

Manage system alerts and notifications.

### List Alerts

Retrieve alerts with filtering.

```http
GET /api/alerts?status=Unacknowledged&severity=High&pageNumber=1&pageSize=50
```

**Query Parameters**:
- `status` (string, optional): Filter by status (`Unacknowledged`, `Acknowledged`, `Resolved`)
- `severity` (string, optional): Filter by severity (`Low`, `Medium`, `High`, `Critical`)
- `alertType` (string, optional): Filter by type
- `clientId` (uuid, optional): Filter by client
- `fromDate` (datetime, optional): Start date
- `toDate` (datetime, optional): End date
- `pageNumber` (integer, optional): Page number
- `pageSize` (integer, optional): Items per page

**Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "alert-123",
      "alertType": "AllocationDrift",
      "severity": "High",
      "title": "Allocation drift detected",
      "message": "Client allocation has drifted 8.5% from target",
      "clientId": "client-456",
      "clientName": "John Doe",
      "status": "Unacknowledged",
      "createdAt": "2025-10-16T11:00:00Z",
      "acknowledgedAt": null,
      "resolvedAt": null
    }
  ],
  "totalCount": 15,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1,
  "unacknowledgedCount": 8,
  "highSeverityCount": 3
}
```

---

### Get Alert By ID

Retrieve detailed information about a specific alert.

```http
GET /api/alerts/{id}
```

**Response** (`200 OK`):
```json
{
  "id": "alert-123",
  "alertType": "AllocationDrift",
  "severity": "High",
  "title": "Allocation drift detected",
  "message": "Client allocation has drifted 8.5% from target allocation of 30%",
  "clientId": "client-456",
  "clientName": "John Doe",
  "status": "Unacknowledged",
  "alertData": {
    "assetId": "wallet-789",
    "assetName": "Main ETH Wallet",
    "targetAllocation": 30.0,
    "currentAllocation": 38.5,
    "drift": 8.5
  },
  "createdAt": "2025-10-16T11:00:00Z",
  "acknowledgedAt": null,
  "acknowledgedBy": null,
  "resolvedAt": null,
  "resolvedBy": null,
  "resolutionNotes": null
}
```

---

### Acknowledge Alert

Mark an alert as acknowledged.

```http
POST /api/alerts/{id}/acknowledge
```

**Response** (`200 OK`):
```json
true
```

**Note**: Sends SignalR event `AlertAcknowledged` to subscribed clients.

---

### Resolve Alert

Mark an alert as resolved with optional notes.

```http
POST /api/alerts/{id}/resolve
```

**Request Body**:
```json
{
  "resolutionNotes": "Rebalanced client portfolio, drift corrected"
}
```

**Response** (`200 OK`):
```json
true
```

---

### Get Alert Summary

Get summary statistics of current alerts.

```http
GET /api/alerts/summary
```

**Response** (`200 OK`):
```json
{
  "totalAlerts": 25,
  "unacknowledged": 8,
  "acknowledged": 10,
  "resolved": 7,
  "bySeverity": {
    "Critical": 1,
    "High": 3,
    "Medium": 12,
    "Low": 9
  },
  "byType": {
    "AllocationDrift": 5,
    "SignificantBalanceChange": 8,
    "SyncFailure": 2,
    "LowBalance": 10
  },
  "recentAlerts": [
    {
      "id": "alert-456",
      "title": "Allocation drift detected",
      "severity": "High",
      "createdAt": "2025-10-16T11:30:00Z"
    }
  ]
}
```

---

## Export API

Generate and download portfolio reports and data exports.

### Export Portfolio PDF

Generate a PDF report for a client's portfolio.

```http
POST /api/export/portfolio-pdf
```

**Request Body**:
```json
{
  "clientId": "client-123",
  "includeSummary": true,
  "includeAllocations": true,
  "includeTransactions": true,
  "transactionPeriod": "30days",
  "includePerformanceMetrics": true
}
```

**Response** (`202 Accepted`):
```json
{
  "exportId": "export-job-456",
  "message": "PDF export job created",
  "estimatedCompletion": "2025-10-16T12:10:00Z",
  "statusUrl": "/api/export/export-job-456"
}
```

**Rate Limit**: Export policy (5 requests per 10 minutes)

---

### Export Transactions Excel

Export transactions to Excel format.

```http
POST /api/export/transactions-excel
```

**Request Body**:
```json
{
  "clientId": "client-123",
  "fromDate": "2025-01-01T00:00:00Z",
  "toDate": "2025-10-16T23:59:59Z",
  "assetType": "Wallet",
  "includeAuditTrail": false
}
```

**Response** (`202 Accepted`):
```json
{
  "exportId": "export-job-789",
  "message": "Excel export job created",
  "estimatedCompletion": "2025-10-16T12:05:00Z"
}
```

---

### Export Performance Excel

Export performance metrics to Excel.

```http
POST /api/export/performance-excel
```

**Request Body**:
```json
{
  "clientIds": ["client-123", "client-456"],
  "period": "90days",
  "includeHistoricalData": true,
  "includeRiskMetrics": true
}
```

**Response** (`202 Accepted`):
```json
{
  "exportId": "export-job-101",
  "message": "Performance export job created",
  "estimatedCompletion": "2025-10-16T12:15:00Z"
}
```

---

### Get Export Job Status

Check the status of an export job.

```http
GET /api/export/{exportId}
```

**Response** (`200 OK`):
```json
{
  "exportId": "export-job-456",
  "status": "Completed",
  "progress": 100,
  "createdAt": "2025-10-16T12:00:00Z",
  "completedAt": "2025-10-16T12:08:00Z",
  "expiresAt": "2025-10-23T12:08:00Z",
  "downloadUrl": "/api/export/export-job-456/download",
  "fileSize": 2048576,
  "fileName": "client-portfolio-2025-10-16.pdf"
}
```

**Job Statuses**:
- `Pending`: Job queued
- `Processing`: Job in progress
- `Completed`: Ready for download
- `Failed`: Job failed (check error message)
- `Expired`: Download link expired (7 days)

---

### Download Export

Download a completed export file.

```http
GET /api/export/{exportId}/download
```

**Response** (`200 OK`):
- Content-Type: `application/pdf` or `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Content-Disposition: `attachment; filename="export.pdf"`
- Binary file content

**Errors**:
- `404 Not Found`: Export not found or expired
- `409 Conflict`: Export not yet completed

---

## System API

System configuration and maintenance endpoints.

### Get Configuration

Retrieve system configuration settings.

```http
GET /api/system/configuration
```

**Response** (`200 OK`):
```json
{
  "syncIntervals": {
    "walletSyncMinutes": 5,
    "accountSyncMinutes": 15,
    "portfolioCalcMinutes": 60,
    "alertGenerationMinutes": 30
  },
  "rateLimits": {
    "globalLimit": 100,
    "windowMinutes": 1,
    "readLimit": 200,
    "writeLimit": 50
  },
  "features": {
    "autoSeeding": true,
    "realTimeUpdates": true,
    "backgroundJobs": true
  },
  "version": "1.0.0",
  "environment": "Development"
}
```

---

### Update Configuration

Update system configuration (admin only).

```http
PUT /api/system/configuration
```

**Request Body**:
```json
{
  "syncIntervals": {
    "walletSyncMinutes": 10,
    "accountSyncMinutes": 30
  }
}
```

**Response** (`200 OK`):
```json
true
```

---

### Trigger Wallet Sync

Manually trigger wallet sync job for all wallets.

```http
POST /api/system/trigger-wallet-sync
```

**Response** (`202 Accepted`):
```json
{
  "message": "Wallet sync job triggered",
  "jobId": "wallet-sync-789",
  "walletsQueued": 15
}
```

---

### Trigger Account Sync

Manually trigger account sync job for all accounts.

```http
POST /api/system/trigger-account-sync
```

**Response** (`202 Accepted`):
```json
{
  "message": "Account sync job triggered",
  "jobId": "account-sync-456",
  "accountsQueued": 25
}
```

---

## Appendix

### Data Types

| Type | Description | Example |
|------|-------------|---------|
| `uuid` | Unique identifier (GUID) | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| `datetime` | ISO 8601 timestamp | `2025-10-16T12:00:00Z` |
| `decimal` | Decimal number | `125000.50` |
| `string` | Text string | `"John Doe"` |
| `boolean` | True/false value | `true` |
| `array` | Array of values | `["ethereum", "polygon"]` |

### Common Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `pageNumber` | integer | Page number (default: 1) |
| `pageSize` | integer | Items per page (default: 50, max: 100) |
| `sortBy` | string | Field to sort by |
| `sortDescending` | boolean | Sort direction (default: false) |
| `search` | string | Search term |
| `fromDate` | datetime | Start date filter |
| `toDate` | datetime | End date filter |

### Status Values

**Client Status**:
- `Active`: Client is active
- `Inactive`: Client temporarily inactive
- `Suspended`: Client suspended

**Wallet/Account Status**:
- `Active`: Asset actively monitored
- `Inactive`: Asset not monitored

**Allocation Status**:
- `Active`: Allocation currently active
- `Ended`: Allocation has ended

**Alert Status**:
- `Unacknowledged`: New alert
- `Acknowledged`: Alert seen by user
- `Resolved`: Alert resolved

### Best Practices

1. **Pagination**: Always use pagination for list endpoints to avoid timeouts
2. **Filtering**: Use specific filters to reduce response size and improve performance
3. **Rate Limits**: Monitor rate limit headers and implement backoff strategies
4. **Real-Time Updates**: Use SignalR for live updates instead of polling
5. **Async Operations**: For exports and heavy operations, use async job endpoints
6. **Error Handling**: Always check for error responses and handle appropriately
7. **Caching**: Implement client-side caching for frequently accessed, rarely changing data
8. **Batch Operations**: Use bulk endpoints when available instead of individual requests

### Support

- **Swagger UI**: https://localhost:7185/swagger (interactive API documentation)
- **Hangfire Dashboard**: https://localhost:7185/hangfire (job monitoring, dev only)
- **Aspire Dashboard**: https://localhost:17128 (observability, logs, metrics)
- **GitHub Issues**: Report API bugs or request features
- **Documentation**: See [README.md](../README.md) for getting started

---

**API Version**: 1.0
**Last Updated**: 2025-10-16
**Total Endpoints**: 65+
**Base URL**: https://localhost:7185 (Development)

---

**Made with d for modern wealth management**
