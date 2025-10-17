# DeFi-Traditional Finance Dashboard API Documentation

## Overview

Complete REST API documentation for the DeFi-Traditional Finance Dashboard backend service.

**Base URL**: `http://localhost:5000/api` (development)

**API Version**: v1.0
**Last Updated**: 2025-10-16

## Table of Contents

- [Authentication](#authentication)
- [Response Format](#response-format)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)
- [Endpoints](#endpoints)
  - [Clients](#clients)
  - [Wallets](#wallets)
  - [Allocations](#allocations)
  - [Portfolio](#portfolio)
  - [Transactions](#transactions)
  - [Alerts](#alerts)
  - [Export](#export)

---

## Authentication

**Current Status**: Not implemented. All endpoints are public.

**Future**: JWT-based authentication with role-based access control (RBAC).

---

## Response Format

### Success Response

```json
{
  "isSuccess": true,
  "value": { ... },
  "error": null
}
```

### Error Response

```json
{
  "isSuccess": false,
  "value": null,
  "error": "Error message describing what went wrong"
}
```

### Paginated Response

```json
{
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 150,
  "totalPages": 3,
  "data": [ ... ]
}
```

---

## Error Handling

### HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success - Request completed successfully |
| 201 | Created - Resource created successfully |
| 400 | Bad Request - Invalid request data or validation error |
| 404 | Not Found - Resource does not exist |
| 500 | Internal Server Error - Server error occurred |

### Error Response Structure

```json
{
  "isSuccess": false,
  "error": "Detailed error message"
}
```

---

## Rate Limiting

**Current Status**: Not implemented.

**Planned Limits**:
- Global: 100 requests per minute per IP
- Read operations: 200 requests per minute
- Write operations: 50 requests per minute
- Export operations: 5 requests per 10 minutes

---

## Endpoints

### Clients

#### GET /api/clients

List all clients with pagination and filtering.

**Query Parameters**:
- `status` (optional) - Filter by status: `Active`, `Inactive`, `Archived`
- `search` (optional) - Search by name or email
- `pageNumber` (optional, default: 1) - Page number
- `pageSize` (optional, default: 50) - Items per page

**Example Request**:
```bash
GET /api/clients?status=Active&search=John&pageNumber=1&pageSize=10
```

**Response** (200 OK):
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "John Doe",
      "email": "john.doe@example.com",
      "document": "123456789",
      "phoneNumber": "+1234567890",
      "status": "Active",
      "notes": "VIP client",
      "createdAt": "2025-01-15T10:30:00Z",
      "updatedAt": "2025-10-16T14:22:00Z"
    }
  ]
}
```

---

#### GET /api/clients/{id}

Get a single client by ID.

**Path Parameters**:
- `id` (required) - Client UUID

**Example Request**:
```bash
GET /api/clients/550e8400-e29b-41d4-a716-446655440000
```

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "John Doe",
    "email": "john.doe@example.com",
    "document": "123456789",
    "phoneNumber": "+1234567890",
    "status": "Active",
    "notes": "VIP client",
    "createdAt": "2025-01-15T10:30:00Z",
    "updatedAt": "2025-10-16T14:22:00Z"
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "isSuccess": false,
  "error": "Client with ID 550e8400-e29b-41d4-a716-446655440000 not found"
}
```

---

#### POST /api/clients

Create a new client.

**Request Body**:
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "document": "123456789",
  "phoneNumber": "+1234567890",
  "notes": "VIP client"
}
```

**Validation Rules**:
- `name`: Required, max 200 characters
- `email`: Required, valid email format, max 200 characters, must be unique
- `document`: Optional, max 50 characters, must be unique if provided
- `phoneNumber`: Optional, max 20 characters
- `notes`: Optional, sanitized for XSS prevention

**Response** (201 Created):
```json
{
  "isSuccess": true,
  "value": "550e8400-e29b-41d4-a716-446655440000"
}
```
Headers:
```
Location: /api/clients/550e8400-e29b-41d4-a716-446655440000
```

**Error Response** (400 Bad Request):
```json
{
  "isSuccess": false,
  "error": "A client with this email already exists"
}
```

---

#### PUT /api/clients/{id}

Update an existing client.

**Path Parameters**:
- `id` (required) - Client UUID

**Request Body**:
```json
{
  "name": "John Doe Updated",
  "email": "john.updated@example.com",
  "document": "987654321",
  "phoneNumber": "+0987654321",
  "status": "Active",
  "notes": "Updated notes"
}
```

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": true
}
```

---

#### DELETE /api/clients/{id}

Delete a client (soft delete - sets status to Archived).

**Path Parameters**:
- `id` (required) - Client UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": true
}
```

**Error Response** (400 Bad Request):
```json
{
  "isSuccess": false,
  "error": "Cannot delete client with active allocations"
}
```

---

### Wallets

#### GET /api/wallets

List all custody wallets.

**Query Parameters**:
- `status` (optional) - Filter by status: `Active`, `Inactive`
- `chain` (optional) - Filter by blockchain: `Ethereum`, `Polygon`, `BSC`, etc.
- `pageNumber` (optional, default: 1)
- `pageSize` (optional, default: 50)

**Response** (200 OK):
```json
{
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 10,
  "totalPages": 1,
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "address": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
      "chain": "Ethereum",
      "label": "Main Custody Wallet",
      "status": "Active",
      "notes": "Primary wallet for ETH holdings",
      "createdAt": "2025-01-15T10:30:00Z",
      "updatedAt": "2025-10-16T14:22:00Z"
    }
  ]
}
```

---

#### POST /api/wallets

Add a new custody wallet for monitoring.

**Request Body**:
```json
{
  "address": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "chain": "Ethereum",
  "label": "Main Custody Wallet",
  "notes": "Primary wallet for ETH holdings"
}
```

**Validation Rules**:
- `address`: Required, valid blockchain address format
- `chain`: Required, supported chains: Ethereum, Polygon, BSC, Arbitrum, Optimism, Avalanche
- `label`: Required, max 200 characters
- `notes`: Optional, sanitized for XSS prevention

**Response** (201 Created):
```json
{
  "isSuccess": true,
  "value": "660e8400-e29b-41d4-a716-446655440000"
}
```

---

#### GET /api/wallets/{id}/balances

Get current token balances for a wallet.

**Path Parameters**:
- `id` (required) - Wallet UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": [
    {
      "tokenAddress": "0x0000000000000000000000000000000000000000",
      "tokenSymbol": "ETH",
      "tokenName": "Ethereum",
      "balance": "10.5",
      "balanceUsd": "35000.00",
      "priceUsd": "3333.33",
      "lastUpdated": "2025-10-16T14:22:00Z"
    },
    {
      "tokenAddress": "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48",
      "tokenSymbol": "USDC",
      "tokenName": "USD Coin",
      "balance": "50000.00",
      "balanceUsd": "50000.00",
      "priceUsd": "1.00",
      "lastUpdated": "2025-10-16T14:22:00Z"
    }
  ]
}
```

---

### Allocations

#### GET /api/allocations/client/{clientId}

Get all allocations for a specific client.

**Path Parameters**:
- `clientId` (required) - Client UUID

**Query Parameters**:
- `activeOnly` (optional, default: true) - Return only active allocations

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "clientId": "550e8400-e29b-41d4-a716-446655440000",
      "assetType": "Wallet",
      "assetId": "660e8400-e29b-41d4-a716-446655440000",
      "allocationType": "Percentage",
      "allocationValue": 30.0,
      "startDate": "2025-01-15",
      "endDate": null,
      "notes": "30% of main ETH wallet",
      "createdAt": "2025-01-15T10:30:00Z",
      "updatedAt": "2025-10-16T14:22:00Z"
    }
  ]
}
```

---

#### POST /api/allocations

Create a new allocation.

**Request Body**:
```json
{
  "clientId": "550e8400-e29b-41d4-a716-446655440000",
  "assetType": "Wallet",
  "assetId": "660e8400-e29b-41d4-a716-446655440000",
  "allocationType": "Percentage",
  "allocationValue": 30.0,
  "startDate": "2025-10-16",
  "notes": "30% of main ETH wallet"
}
```

**Validation Rules**:
- `clientId`: Required, client must exist and be Active
- `assetType`: Required, must be `Wallet` or `Account`
- `assetId`: Required, asset must exist
- `allocationType`: Required, must be `Percentage` or `FixedAmount`
- `allocationValue`: Required, must be > 0
  - For Percentage: Must be between 0 and 100
  - Total percentage allocations for an asset cannot exceed 100%
- `startDate`: Required, must be valid date
- `notes`: Optional

**Response** (201 Created):
```json
{
  "isSuccess": true,
  "value": "770e8400-e29b-41d4-a716-446655440000"
}
```

**Error Response** (400 Bad Request):
```json
{
  "isSuccess": false,
  "error": "Total percentage allocation would exceed 100% (current: 75%)"
}
```

---

### Portfolio

#### GET /api/portfolio/overview

Get overall portfolio overview with aggregated metrics.

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": {
    "totalValueUsd": "1250000.00",
    "totalClients": 50,
    "totalWallets": 10,
    "totalAccounts": 5,
    "assetBreakdown": {
      "crypto": "850000.00",
      "traditional": "400000.00"
    },
    "topAssets": [
      {
        "symbol": "ETH",
        "valueUsd": "350000.00",
        "percentageOfTotal": 28.0
      },
      {
        "symbol": "BTC",
        "valueUsd": "250000.00",
        "percentageOfTotal": 20.0
      }
    ],
    "lastUpdated": "2025-10-16T14:22:00Z"
  }
}
```

---

#### GET /api/portfolio/client/{clientId}

Get detailed portfolio for a specific client.

**Path Parameters**:
- `clientId` (required) - Client UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": {
    "clientId": "550e8400-e29b-41d4-a716-446655440000",
    "clientName": "John Doe",
    "totalValueUsd": "50000.00",
    "allocations": [
      {
        "assetType": "Wallet",
        "assetLabel": "Main ETH Wallet",
        "allocationType": "Percentage",
        "allocationValue": 30.0,
        "currentValueUsd": "35000.00",
        "tokens": [
          {
            "symbol": "ETH",
            "balance": "10.5",
            "valueUsd": "35000.00"
          }
        ]
      }
    ],
    "performance": {
      "totalReturnUsd": "5000.00",
      "totalReturnPercentage": 11.11,
      "period": "all-time"
    },
    "lastUpdated": "2025-10-16T14:22:00Z"
  }
}
```

---

### Transactions

#### GET /api/transactions

List all transactions with filtering and pagination.

**Query Parameters**:
- `walletId` (optional) - Filter by wallet UUID
- `clientId` (optional) - Filter by client UUID
- `type` (optional) - Filter by type: `Transfer`, `Swap`, `Deposit`, `Withdrawal`, `Manual`
- `status` (optional) - Filter by status: `Confirmed`, `Pending`, `Failed`
- `fromDate` (optional) - Start date (ISO 8601)
- `toDate` (optional) - End date (ISO 8601)
- `pageNumber` (optional, default: 1)
- `pageSize` (optional, default: 50)

**Response** (200 OK):
```json
{
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 1250,
  "totalPages": 25,
  "data": [
    {
      "id": "880e8400-e29b-41d4-a716-446655440000",
      "walletId": "660e8400-e29b-41d4-a716-446655440000",
      "chain": "Ethereum",
      "transactionHash": "0xabc123...",
      "type": "Transfer",
      "status": "Confirmed",
      "fromAddress": "0x742d35Cc...",
      "toAddress": "0x123abc...",
      "tokenSymbol": "ETH",
      "amount": "1.5",
      "valueUsd": "5000.00",
      "gasFee": "0.002",
      "gasFeeUsd": "6.66",
      "timestamp": "2025-10-16T14:22:00Z",
      "blockNumber": 18500000
    }
  ]
}
```

---

#### GET /api/transactions/{id}

Get detailed information about a specific transaction.

**Path Parameters**:
- `id` (required) - Transaction UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": {
    "id": "880e8400-e29b-41d4-a716-446655440000",
    "walletId": "660e8400-e29b-41d4-a716-446655440000",
    "chain": "Ethereum",
    "transactionHash": "0xabc123...",
    "type": "Transfer",
    "status": "Confirmed",
    "fromAddress": "0x742d35Cc...",
    "toAddress": "0x123abc...",
    "tokenSymbol": "ETH",
    "tokenAddress": "0x000...",
    "amount": "1.5",
    "valueUsd": "5000.00",
    "gasFee": "0.002",
    "gasFeeUsd": "6.66",
    "timestamp": "2025-10-16T14:22:00Z",
    "blockNumber": 18500000,
    "confirmations": 150,
    "notes": "Monthly distribution"
  }
}
```

---

### Alerts

#### GET /api/alerts

Get all system alerts.

**Query Parameters**:
- `type` (optional) - Filter by type: `RebalancingNeeded`, `LargeTransaction`, `PriceAlert`, `SystemError`
- `severity` (optional) - Filter by severity: `Info`, `Warning`, `Critical`
- `status` (optional) - Filter by status: `Unread`, `Acknowledged`, `Resolved`
- `pageNumber` (optional, default: 1)
- `pageSize` (optional, default: 50)

**Response** (200 OK):
```json
{
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 15,
  "totalPages": 1,
  "data": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440000",
      "type": "RebalancingNeeded",
      "severity": "Warning",
      "status": "Unread",
      "title": "Client allocation drift detected",
      "message": "Client John Doe has 5% allocation drift on Main ETH Wallet",
      "relatedEntityType": "Allocation",
      "relatedEntityId": "770e8400-e29b-41d4-a716-446655440000",
      "createdAt": "2025-10-16T14:22:00Z",
      "acknowledgedAt": null,
      "resolvedAt": null
    }
  ]
}
```

---

#### POST /api/alerts/{id}/acknowledge

Acknowledge an alert.

**Path Parameters**:
- `id` (required) - Alert UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": true
}
```

---

#### POST /api/alerts/{id}/resolve

Mark an alert as resolved.

**Path Parameters**:
- `id` (required) - Alert UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": true
}
```

---

### Export

#### POST /api/export/portfolio/pdf

Export client portfolio as PDF (asynchronous job).

**Request Body**:
```json
{
  "clientId": "550e8400-e29b-41d4-a716-446655440000",
  "includeTransactions": true,
  "includePerformance": true,
  "dateRange": {
    "from": "2025-01-01",
    "to": "2025-10-16"
  }
}
```

**Response** (202 Accepted):
```json
{
  "isSuccess": true,
  "value": {
    "jobId": "aaa0e8400-e29b-41d4-a716-446655440000",
    "status": "Pending",
    "estimatedCompletionSeconds": 30
  }
}
```

---

#### GET /api/export/job/{jobId}

Check the status of an export job.

**Path Parameters**:
- `jobId` (required) - Export job UUID

**Response** (200 OK):
```json
{
  "isSuccess": true,
  "value": {
    "jobId": "aaa0e8400-e29b-41d4-a716-446655440000",
    "status": "Completed",
    "downloadUrl": "/api/export/download/aaa0e8400-e29b-41d4-a716-446655440000",
    "expiresAt": "2025-10-17T14:22:00Z",
    "createdAt": "2025-10-16T14:22:00Z",
    "completedAt": "2025-10-16T14:22:25Z"
  }
}
```

**Status Values**: `Pending`, `Processing`, `Completed`, `Failed`

---

#### GET /api/export/download/{jobId}

Download completed export file.

**Path Parameters**:
- `jobId` (required) - Export job UUID

**Response** (200 OK):
- Content-Type: `application/pdf` or `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Binary file download

---

## Webhooks (Future)

**Planned**: Webhook support for real-time notifications on:
- New transactions detected
- Portfolio value changes > threshold
- Alert triggers
- Export job completion

---

## SDK Support (Future)

**Planned**: Official client SDKs for:
- JavaScript/TypeScript
- Python
- C#/.NET

---

## Changelog

### v1.0.0 (2025-10-16)
- Initial API release
- Full CRUD for Clients, Wallets, Allocations
- Portfolio calculation and reporting
- Transaction tracking
- Alert system
- Export functionality (PDF/Excel)

---

## Support

**Documentation**: See `USER-GUIDE.md` and `DEPLOYMENT.md`
**Issues**: Create an issue in the project repository
**Email**: support@defi-dashboard.com (placeholder)
