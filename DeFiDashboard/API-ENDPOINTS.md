# DeFi Dashboard API - Implemented Endpoints

## ✅ Currently Implemented

### 👥 Clients API (Complete CRUD)

#### Create Client
```http
POST /api/clients
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "document": "12345678900",
  "phoneNumber": "+5511999999999",
  "notes": "VIP client"
}

Response: 201 Created
{ "id": "guid" }
```

#### Get All Clients (Paginated)
```http
GET /api/clients?status=Active&pageNumber=1&pageSize=50

Response: 200 OK
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 2
}
```

#### Get Client by ID
```http
GET /api/clients/{id}

Response: 200 OK
{
  "id": "guid",
  "name": "João Silva",
  "email": "joao@example.com",
  ...
}
```

#### Update Client
```http
PUT /api/clients/{id}
Content-Type: application/json

{
  "name": "João Silva Updated",
  "email": "joao@example.com",
  "document": "12345678900",
  "phoneNumber": "+5511999999999",
  "notes": "Updated notes",
  "status": "Active"
}

Response: 204 No Content
```

#### Delete Client
```http
DELETE /api/clients/{id}

Response: 204 No Content
```

---

### 💰 Wallets API

#### Add Wallet
```http
POST /api/wallets
Content-Type: application/json

{
  "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "label": "Main Treasury Wallet",
  "supportedChains": ["ethereum", "polygon", "bsc"],
  "notes": "Primary custody wallet"
}

Response: 201 Created
{ "id": "guid" }
```

**Features:**
- ✅ Validates Ethereum address format (0x + 40 hex chars)
- ✅ Prevents duplicate wallet addresses
- ✅ Auto-sets provider to "Moralis"

#### Get All Wallets (Paginated)
```http
GET /api/wallets?status=Active&pageNumber=1&pageSize=50

Response: 200 OK
{
  "items": [
    {
      "id": "guid",
      "walletAddress": "0x742d35...",
      "label": "Main Treasury",
      "blockchainProvider": "Moralis",
      "supportedChains": ["ethereum", "polygon"],
      "status": "Active",
      "notes": "...",
      "createdAt": "2025-10-12T...",
      "updatedAt": "2025-10-12T..."
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

---

### 📊 Allocations API (Critical Feature)

#### Create Allocation
```http
POST /api/allocations
Content-Type: application/json

{
  "clientId": "client-guid",
  "assetType": "Wallet",
  "assetId": "wallet-guid",
  "allocationType": "Percentage",
  "allocationValue": 25.5,
  "startDate": "2025-10-12T00:00:00Z",
  "notes": "Q4 2025 allocation"
}

Response: 201 Created
{ "id": "guid" }
```

**Asset Types:** `"Wallet"` or `"Account"`
**Allocation Types:** `"Percentage"` or `"FixedAmount"`

**Business Rules Enforced:**
- ✅ Client must exist and be Active
- ✅ Asset (Wallet or Account) must exist
- ✅ No duplicate active allocations for same client+asset
- ✅ **Percentage allocations**: Total cannot exceed 100% per asset
- ✅ Percentage values: Must be 0-100
- ✅ Fixed amount values: Must be > 0

#### Get Client Allocations
```http
GET /api/clients/{clientId}/allocations?activeOnly=true

Response: 200 OK
[
  {
    "id": "guid",
    "clientId": "guid",
    "assetType": "Wallet",
    "assetId": "guid",
    "assetIdentifier": "0x742d35...",
    "allocationType": "Percentage",
    "allocationValue": 25.5,
    "startDate": "2025-10-12T...",
    "endDate": null,
    "notes": "Q4 2025 allocation",
    "createdAt": "2025-10-12T..."
  }
]
```

**Query Parameters:**
- `activeOnly`: `true` (default) = only active allocations, `false` = all allocations

---

## 🔧 Features Implemented

### ✅ Architecture
- **Vertical Slice Architecture** - Each feature is self-contained
- **CQRS Pattern** - Commands (write) separated from Queries (read)
- **MediatR Pipeline** - Centralized request handling
- **FluentValidation** - Automatic validation before handler execution
- **Result Pattern** - Consistent error handling

### ✅ Database
- **Supabase PostgreSQL** - "dash" schema
- **12 Tables** - All created and indexed
- **EF Core 9** - Code-first with migrations
- **Automatic Timestamps** - CreatedAt/UpdatedAt handled by DbContext

### ✅ API Features
- **Carter** - Minimal APIs with auto-discovery
- **Swagger/OpenAPI** - Auto-generated documentation
- **FluentValidation** - Input validation
- **Aspire Observability** - Logs, traces, metrics

---

## 🚀 Testing the API

### Method 1: Swagger UI (Recommended)

1. **Open Aspire Dashboard**: https://localhost:17128
2. **Navigate to Resources** → **apiservice**
3. **Click on Swagger** link (or find the HTTPS endpoint)
4. **Try out the endpoints** interactively

### Method 2: curl Commands

**Find API port:**
```bash
# Check Aspire Dashboard or logs for the ApiService HTTPS port
# Usually: https://localhost:7xxx
```

**Create a client:**
```bash
curl -X POST https://localhost:7xxx/api/clients \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Client",
    "email": "test@example.com",
    "document": "12345678900",
    "phoneNumber": "+5511999999999",
    "notes": "API test"
  }' \
  --insecure
```

**Add a wallet:**
```bash
curl -X POST https://localhost:7xxx/api/wallets \
  -H "Content-Type: application/json" \
  -d '{
    "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
    "label": "Test Wallet",
    "supportedChains": ["ethereum"],
    "notes": "Test wallet"
  }' \
  --insecure
```

**Create an allocation:**
```bash
# First get client ID and wallet ID from previous requests
curl -X POST https://localhost:7xxx/api/allocations \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "<client-id>",
    "assetType": "Wallet",
    "assetId": "<wallet-id>",
    "allocationType": "Percentage",
    "allocationValue": 50,
    "startDate": "2025-10-12T00:00:00Z",
    "notes": "50% allocation"
  }' \
  --insecure
```

---

## 📁 Project Structure

```
Features/
├── Clients/          # ✅ Complete CRUD (5 operations)
│   ├── Create/
│   ├── GetList/
│   ├── GetById/
│   ├── Update/
│   └── Delete/
├── Wallets/          # ✅ Add + List (2 operations)
│   ├── Add/
│   └── GetList/
└── Allocations/      # ✅ Create + GetByClient (2 operations)
    ├── Create/
    └── GetByClient/
```

**Total Files Created**: 35+ files across 3 vertical slices

---

## 🎯 What's Working Now

1. ✅ **Complete Client Management**
   - CRUD operations
   - Email/document uniqueness validation
   - Status management (Active/Inactive/Suspended)
   - Safe deletion (prevents if has active allocations)

2. ✅ **Wallet Registration**
   - Add custody wallets
   - Ethereum address validation
   - List all wallets with pagination

3. ✅ **Client Asset Allocations** ⭐ (Most Important)
   - Create allocations (Percentage or Fixed Amount)
   - Prevent over-allocation (>100%)
   - View client allocations
   - Asset existence validation

---

## 📊 Database Status

**Schema**: `dash`
**Tables**: 13 tables (12 + migrations history)
**Connection**: ✅ Connected to Supabase
**Status**: ✅ All tables created with indexes

---

## 🔜 Next Steps (Not Yet Implemented)

1. **Transactions API** - View transaction history
2. **Portfolio API** - Get consolidated client portfolio
3. **Moralis Integration** - Sync wallet balances from blockchain
4. **Pluggy Integration** - Connect traditional bank accounts
5. **Hangfire Jobs** - Background sync tasks
6. **Analytics API** - Performance metrics, ROI, P&L

---

## 🛠️ Quick Reference

**Aspire Dashboard**: https://localhost:17128
**API Base URL**: Check Aspire → Resources → apiservice
**Swagger UI**: API Base URL + `/swagger`
**Database Schema**: `dash`
**Database Provider**: Supabase PostgreSQL

---

**Last Updated**: 2025-10-12
**Phase**: 3 Complete - Core CRUD Operations
**Next Phase**: 4 - External Integrations (Moralis/Pluggy)
