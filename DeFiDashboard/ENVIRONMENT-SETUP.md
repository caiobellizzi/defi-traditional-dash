# Environment Setup Guide

## Prerequisites

- .NET 9 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- Node.js 18+ ([Download](https://nodejs.org))
- PostgreSQL 15+ (local) OR Supabase account ([Sign up](https://supabase.com))
- Moralis API Key ([Get started](https://moralis.io))
- Pluggy API Credentials ([Get started](https://pluggy.ai))

## Development Setup

### 1. Clone and Configure

```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard

# Copy configuration templates
cp src/ApiService/appsettings.Development.json.template src/ApiService/appsettings.Development.json

# Install frontend dependencies
cd frontend
npm install
cd ..
```

### 2. Configure Database

**Option A: Local PostgreSQL** (Recommended for development)

```bash
# Create database
createdb defi_dashboard

# Connection string is already configured in template:
# Host=localhost;Database=defi_dashboard;Username=postgres;Password=postgres

# If using different credentials, update in appsettings.Development.json:
# ConnectionStrings:Supabase
```

**Option B: Supabase** (Cloud PostgreSQL)

1. Create project at [supabase.com](https://supabase.com)
2. Navigate to Settings > Database
3. Copy the connection string (Connection Pooling > Transaction mode)
4. Update `ConnectionStrings:Supabase` in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=db.YOUR_PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 3. Configure External Providers

#### Moralis (Blockchain Data)

1. Sign up at [moralis.io](https://moralis.io)
2. Create a new project
3. Navigate to Settings > API Keys
4. Copy your API key
5. Update in `appsettings.Development.json`:

```json
{
  "ExternalProviders": {
    "Moralis": {
      "ApiKey": "your-actual-api-key-here"
    }
  }
}
```

**Supported Chains**:
- Ethereum (ETH)
- Polygon (MATIC)
- BNB Smart Chain (BSC)
- Avalanche (AVAX)
- Fantom (FTM)
- Arbitrum
- Optimism

#### Pluggy (OpenFinance / Traditional Accounts)

1. Sign up at [pluggy.ai](https://pluggy.ai)
2. Get sandbox credentials from dashboard
3. Update in `appsettings.Development.json`:

```json
{
  "ExternalProviders": {
    "Pluggy": {
      "ClientId": "your-client-id-here",
      "ClientSecret": "your-client-secret-here"
    }
  }
}
```

**Supported Integrations**:
- Brazilian banks (via OpenFinance)
- Investment accounts
- Credit cards
- Payment accounts

### 4. Configure Frontend Environment

```bash
cd frontend

# Copy environment template
cp .env.example .env

# Update VITE_API_BASE_URL if needed (default: https://localhost:7185)
# When running with Aspire, this is auto-configured
```

### 5. Run Database Migrations

```bash
# From project root
cd src/ApiService

# Ensure EF Core tools are installed
dotnet tool install --global dotnet-ef

# Run migrations
dotnet ef database update

# You should see:
# - InitialCreate migration applied
# - UpdateSchema migration applied
# - Database schema created
```

**Verify Tables Created**:
```sql
-- Connect to your database and check:
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;

-- Expected tables:
-- Clients, CustodyWallets, TraditionalAccounts, ClientAssetAllocations,
-- WalletBalances, AccountBalances, Transactions, TransactionAudit,
-- PriceHistory, PerformanceMetrics, RebalancingAlerts, SystemConfiguration
```

### 6. Start Application

**Option A: Using Aspire (Recommended)** - Full orchestration with observability

```bash
# From project root
cd DeFiDashboard.AppHost
dotnet run

# This starts:
# ✅ ApiService (Backend API)
# ✅ Frontend (React + Vite with hot-reload)
# ✅ Aspire Dashboard (Observability)
# ✅ Database (if using containerized PostgreSQL)
```

**Option B: Run Services Separately** - Traditional approach

```bash
# Terminal 1: Backend API
cd src/ApiService
dotnet run

# Terminal 2: Frontend
cd frontend
npm run dev
```

### 7. Access Application

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:5173 | React application |
| **API** | https://localhost:7185 | Backend API |
| **Swagger UI** | https://localhost:7185/swagger | API documentation |
| **Hangfire Dashboard** | https://localhost:7185/hangfire | Background jobs |
| **Aspire Dashboard** | https://localhost:17243 | Observability (logs, traces, metrics) |

### 8. Initial Configuration

After starting the application:

1. **Add Test Client**:
   - Navigate to Clients page
   - Add a test client (e.g., "Test Client", "test@example.com")

2. **Add Test Wallet**:
   - Go to Wallets page
   - Add a wallet address (e.g., Ethereum: `0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb`)
   - Select blockchain network (Ethereum, Polygon, etc.)

3. **Create Allocation**:
   - Allocate percentage of wallet to test client
   - Example: 100% of wallet to test client

4. **Trigger Sync**:
   - Navigate to Hangfire dashboard
   - Manually trigger "Moralis Sync" job
   - Check logs in Aspire dashboard

5. **Verify Data**:
   - Check wallet balances updated
   - View client portfolio
   - Check transaction history

## Production Setup

### 1. Configure Secrets

**Using Environment Variables** (Recommended):

```bash
export ConnectionStrings__Supabase="Host=db.xxx.supabase.co;Database=postgres;..."
export ExternalProviders__Moralis__ApiKey="your-production-key"
export ExternalProviders__Pluggy__ClientId="your-production-id"
export ExternalProviders__Pluggy__ClientSecret="your-production-secret"
```

**Using Azure Key Vault**:

```json
{
  "KeyVault": {
    "VaultUri": "https://your-vault.vault.azure.net/"
  }
}
```

**Using Docker Secrets**:

```yaml
services:
  api:
    environment:
      - ConnectionStrings__Supabase=/run/secrets/db_connection
    secrets:
      - db_connection
      - moralis_key
      - pluggy_credentials
```

### 2. Update appsettings.Production.json

1. Copy template:
```bash
cp src/ApiService/appsettings.Production.json.template src/ApiService/appsettings.Production.json
```

2. Replace tokenized values (`#{...}#`) with actual secrets or leave for CI/CD

3. Ensure file is NOT committed to git (.gitignore already configured)

### 3. Deploy

See deployment guides for specific platforms:
- [Deploy to Azure App Service](./docs/DEPLOY-AZURE.md)
- [Deploy with Docker](./docs/DEPLOY-DOCKER.md)
- [Deploy to Kubernetes](./docs/DEPLOY-K8S.md)

## Troubleshooting

### Database Connection Issues

**Problem**: `Npgsql.NpgsqlException: Connection refused`

**Solutions**:
- Verify PostgreSQL is running: `pg_ctl status`
- Check connection string format
- Ensure database exists: `psql -l`
- For Supabase: Check SSL mode and certificate trust settings

### Migration Errors

**Problem**: `relation "Clients" already exists`

**Solutions**:
```bash
# Drop and recreate database (development only!)
dropdb defi_dashboard
createdb defi_dashboard
dotnet ef database update

# Or remove specific migration
dotnet ef migrations remove
```

**Problem**: `No migrations found`

**Solutions**:
```bash
# Verify EF Core tools installed
dotnet ef --version

# Reinstall if needed
dotnet tool update --global dotnet-ef

# Check migrations directory exists
ls src/ApiService/Migrations/
```

### Provider API Errors

**Moralis Rate Limits**:
- Free tier: 40,000 requests/day
- Implement caching in `MoralisProvider`
- Adjust sync interval in `appsettings.json` (`BackgroundJobs:MoralisSyncIntervalMinutes`)

**Pluggy Authentication Failed**:
- Verify ClientId and ClientSecret are correct
- Check if using sandbox vs production credentials
- Review Pluggy dashboard for API key status

**Check Hangfire Dashboard**:
- Navigate to `/hangfire`
- Review failed jobs
- Check job logs for detailed errors
- Manually retry failed jobs

### Frontend Not Loading

**Problem**: `Failed to fetch` or CORS errors

**Solutions**:
```bash
# Verify API is running
curl https://localhost:7185/health

# Check VITE_API_BASE_URL in .env
cat frontend/.env

# Verify CORS configuration in Program.cs
# Should include: http://localhost:5173
```

**Problem**: `Module not found` errors

**Solutions**:
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

### Aspire Dashboard Issues

**Problem**: Dashboard not accessible at https://localhost:17243

**Solutions**:
```bash
# Verify AppHost is running
cd DeFiDashboard.AppHost
dotnet run --verbose

# Check firewall blocking port 17243
# Check browser accepts self-signed certificate
```

## Testing Provider Integrations

### Test Moralis API

**Manual Sync Trigger**:
```bash
# POST to trigger wallet sync
curl -X POST https://localhost:7185/api/system/sync/wallets \
  -H "Content-Type: application/json"

# Check job execution in Hangfire
open https://localhost:7185/hangfire
```

**Direct API Test**:
```bash
# Test Moralis connectivity (replace with your API key)
curl -X GET "https://deep-index.moralis.io/api/v2.2/0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb/balance" \
  -H "X-API-Key: YOUR_API_KEY"
```

### Test Pluggy OAuth Flow

**Create Connect Token**:
```bash
# GET connect token
curl https://localhost:7185/api/accounts/connect-token

# Response:
# {
#   "accessToken": "...",
#   "connectUrl": "https://connect.pluggy.ai?accessToken=..."
# }
```

**Complete OAuth Flow**:
1. Open `connectUrl` in browser
2. Select test institution (Pluggy Sandbox)
3. Enter test credentials
4. Complete authorization
5. Verify callback received in API logs
6. Check accounts synced in database

### Monitor Real-time Updates

**Aspire Dashboard**:
1. Navigate to https://localhost:17243
2. Select "Logs" tab
3. Filter by service: `ApiService`
4. Watch for:
   - `MoralisSyncJob executing...`
   - `PluggySyncJob executing...`
   - `Portfolio recalculation started...`

**Hangfire Dashboard**:
1. Navigate to https://localhost:7185/hangfire
2. Check "Recurring Jobs" tab
3. Verify jobs scheduled:
   - `moralis-sync` (every 5 min)
   - `pluggy-sync` (every 10 min)
   - `portfolio-calculation` (every 15 min)
   - `rebalancing-alerts` (every 60 min)

## Performance Optimization

### Database Indexing

Ensure migrations created indexes for:
```sql
CREATE INDEX idx_walletbalances_wallet_token ON WalletBalances(WalletId, TokenAddress);
CREATE INDEX idx_transactions_client_date ON Transactions(ClientId, Timestamp DESC);
CREATE INDEX idx_allocations_client_active ON ClientAssetAllocations(ClientId, EndDate) WHERE EndDate IS NULL;
```

### Caching Strategy

Configure Redis for caching (optional):
```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "DeFiDashboard:"
  }
}
```

### Background Job Tuning

Adjust sync intervals based on data freshness needs:
```json
{
  "BackgroundJobs": {
    "MoralisSyncIntervalMinutes": 5,     // More frequent = more API calls
    "PluggySyncIntervalMinutes": 10,     // Balance freshness vs cost
    "PortfolioCalculationIntervalMinutes": 15,
    "RebalancingCheckIntervalMinutes": 60
  }
}
```

## Security Checklist

- [ ] Database connection strings use environment variables in production
- [ ] API keys stored in Azure Key Vault or equivalent
- [ ] HTTPS enforced in production
- [ ] CORS configured for specific origins only
- [ ] Hangfire dashboard protected with authentication
- [ ] SQL injection protection (EF Core parameterization)
- [ ] Input validation enabled (FluentValidation)
- [ ] Rate limiting configured
- [ ] Audit logging enabled for sensitive operations

## Next Steps

1. **Configure All API Credentials**
   - Add Moralis API key
   - Add Pluggy credentials
   - Verify provider connectivity

2. **Run Initial Data Sync**
   - Add first client
   - Add first wallet
   - Create allocation
   - Trigger manual sync

3. **Verify Background Jobs**
   - Check Hangfire dashboard
   - Monitor job execution
   - Review logs for errors

4. **Test Frontend Real-time Updates**
   - Add client via UI
   - Watch data appear in dashboard
   - Verify portfolio calculations

5. **Review Observability**
   - Check Aspire dashboard logs
   - Review distributed traces
   - Monitor performance metrics

6. **Production Preparation**
   - Configure production secrets
   - Set up CI/CD pipeline
   - Configure monitoring/alerting
   - Run load tests

## Support Resources

- [Project Documentation](./CLAUDE.md)
- [Planning Document](./PLANNING.md)
- [Moralis API Docs](https://docs.moralis.io)
- [Pluggy API Docs](https://docs.pluggy.ai)
- [.NET Aspire Docs](https://learn.microsoft.com/dotnet/aspire)
- [Entity Framework Core Docs](https://learn.microsoft.com/ef/core)

## Common Commands Reference

```bash
# Database
dotnet ef migrations add MigrationName --project src/ApiService
dotnet ef database update --project src/ApiService
dotnet ef migrations remove --project src/ApiService

# Development
dotnet run --project DeFiDashboard.AppHost  # Run with Aspire
dotnet run --project src/ApiService          # Run API only
dotnet watch --project src/ApiService        # Watch mode
npm run dev                                   # Frontend dev server

# Testing
dotnet test                                   # Run all tests
dotnet test --logger "console;verbosity=detailed"

# Build
dotnet build
dotnet publish -c Release -o ./publish

# Tools
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```
