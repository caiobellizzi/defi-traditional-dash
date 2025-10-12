# Next Steps - Database Setup & Testing

## ✅ What's Been Completed

- [x] Complete EF Core entity model (12 tables)
- [x] ApplicationDbContext with auto-timestamps
- [x] Entity configurations matching PostgreSQL schema
- [x] EF Core migration (`InitialCreate`)
- [x] SQL script for Supabase (`migrations.sql`)
- [x] appsettings.json templates configured
- [x] .gitignore configured for security

## 🚀 What You Need to Do Now

### Step 1: Get Supabase Credentials (5 minutes)

1. Go to [Supabase Dashboard](https://app.supabase.com/)
2. Select your project (or create new)
3. Go to **Settings** → **Database**
4. Copy your **Connection string** details:
   - Host: `db.YOUR_PROJECT_ID.supabase.co`
   - Password: (shown in Database settings)

### Step 2: Configure Connection String (2 methods)

#### Method A: User Secrets (Recommended - Secure)

```bash
cd src/ApiService

dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.YOUR_PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

# Verify
dotnet user-secrets list
```

#### Method B: appsettings.Development.json (Quick Test)

Edit `src/ApiService/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=db.YOUR_PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

⚠️ **Warning**: Don't commit real credentials to git!

### Step 3: Apply Database Schema (Choose One)

#### Option A: SQL Script (Fastest - Recommended)

1. Open Supabase Dashboard → **SQL Editor**
2. Click **New Query**
3. Copy contents from: `migrations.sql`
4. Paste and click **Run**
5. ✅ All tables, indexes, and constraints created!

#### Option B: EF Core Migration

```bash
cd src/ApiService
dotnet ef database update

# Should see:
# Applying migration '20251012051546_InitialCreate'.
# Done.
```

### Step 4: Test the Connection

```bash
# From DeFiDashboard.AppHost folder
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/DeFiDashboard.AppHost
dotnet run

# Check output:
# - Aspire Dashboard URL (https://localhost:17xxx)
# - ApiService should start successfully
# - No database connection errors
```

Then open Aspire Dashboard and check:
- ✅ ApiService status: Running (green)
- ✅ Logs: No connection errors
- ✅ Health checks: Healthy

### Step 5: Verify Schema in Supabase

1. Go to Supabase Dashboard → **Table Editor**
2. You should see 12 tables:
   - clients
   - custody_wallets
   - traditional_accounts
   - **client_asset_allocations** (critical!)
   - wallet_balances
   - account_balances
   - transactions
   - transaction_audit
   - price_history
   - performance_metrics
   - rebalancing_alerts
   - system_configuration

## 📋 Optional: Configure External Providers

### Moralis (Blockchain Data)

1. Get API key: https://admin.moralis.io/register
2. Add to `appsettings.json`:

```json
{
  "ExternalProviders": {
    "Moralis": {
      "ApiKey": "YOUR_MORALIS_API_KEY",
      "BaseUrl": "https://deep-index.moralis.io/api/v2.2"
    }
  }
}
```

### Pluggy (OpenFinance Brasil)

1. Get credentials: https://dashboard.pluggy.ai/
2. Add to `appsettings.json`:

```json
{
  "ExternalProviders": {
    "Pluggy": {
      "ClientId": "YOUR_PLUGGY_CLIENT_ID",
      "ClientSecret": "YOUR_PLUGGY_CLIENT_SECRET",
      "BaseUrl": "https://api.pluggy.ai"
    }
  }
}
```

## 🔍 Troubleshooting

### Connection Error: "Connection string 'Supabase' not found"

**Solution**: User secrets not set or appsettings not configured. Follow Step 2 above.

### SSL/Certificate Error

**Solution**: Add to connection string:
```
SSL Mode=Require;Trust Server Certificate=true
```

### Migration Error: "A network-related error occurred"

**Solution**:
- Check Supabase project is running (not paused)
- Verify connection string is correct
- Check firewall/VPN blocking port 5432

### Aspire Not Finding Frontend

**Solution**:
```bash
cd frontend
npm install
# Then restart Aspire
```

## ✨ What's Next?

After database is connected and tested:

1. **Phase 3**: Implement first vertical slice (Client CRUD)
   - Create Client endpoints (POST, GET, PUT, DELETE)
   - Add FluentValidation rules
   - Create Carter endpoints
   - Test with Swagger

2. **Phase 4**: Add Moralis integration
   - Implement wallet sync
   - Create wallet balance tracking
   - Set up Hangfire background jobs

3. **Phase 5**: Add Pluggy integration
   - Implement account connection flow
   - Sync account balances
   - Track transactions

## 📚 Documentation Files

- `SUPABASE-CONFIG.md` - Detailed Supabase setup guide
- `ASPIRE-SETUP.md` - Aspire orchestration guide
- `CLAUDE.md` - Architecture and development guide
- `PLANNING.md` - Full implementation roadmap
- `SDK-INTEGRATION.md` - Moralis/Pluggy integration guide

---

**Ready to start Phase 3?** Let me know when database is connected!
