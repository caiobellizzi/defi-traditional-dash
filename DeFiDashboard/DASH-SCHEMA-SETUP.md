# Dash Schema Setup - Complete

## ✅ What Was Done

### 1. User Secrets Configured
All your credentials are securely stored:
```bash
cd src/ApiService
dotnet user-secrets list

# Shows:
# - Supabase:Url
# - Supabase:AnonKey
# - Supabase:ServiceRoleKey
# - ExternalProviders:Moralis:ApiKey
```

### 2. SQL Script for "dash" Schema Created
**File**: `migrations-dash-schema.sql`

This script:
- ✅ Creates the "dash" schema
- ✅ Sets search path to use "dash" schema by default
- ✅ Creates all 12 tables in the "dash" schema
- ✅ Creates all indexes and foreign keys
- ✅ Includes idempotency checks (safe to re-run)

**Key differences from original:**
- All tables prefixed with `dash.` (e.g., `dash.clients`)
- Uses `CREATE SCHEMA IF NOT EXISTS dash;`
- Sets `SET search_path TO dash, public;`
- Foreign keys reference tables in dash schema

### 3. EF Core Updated to Use "dash" Schema
**File**: `src/ApiService/Common/Database/ApplicationDbContext.cs` (line 32)

Added:
```csharp
modelBuilder.HasDefaultSchema("dash");
```

This ensures all EF Core operations use the "dash" schema.

## 📋 What You Need To Do

### Step 1: Get Database Password

1. Go to https://app.supabase.com/project/xqndfgrhyekxttvsddob/settings/database
2. Find "Database password" section
3. Click "Show" and copy the password

### Step 2: Set Connection String with Password

```bash
cd src/ApiService

# Replace YOUR_DB_PASSWORD with actual password
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.xqndfgrhyekxttvsddob.supabase.co;Database=postgres;Username=postgres;Password=YOUR_DB_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

# Verify it's set
dotnet user-secrets list | grep ConnectionStrings
```

### Step 3: Apply Schema to Supabase

**Method 1: SQL Editor (Recommended)**

1. Open https://app.supabase.com/project/xqndfgrhyekxttvsddob/sql
2. Click **New Query**
3. Copy entire contents from: `migrations-dash-schema.sql`
4. Paste into SQL Editor
5. Click **Run** (Cmd+Enter)
6. ✅ Should see success message and list of tables

**Method 2: EF Core Migration**

```bash
cd src/ApiService
dotnet ef database update

# Should output:
# Applying migration '20251012051546_InitialCreate'.
# Done.
```

### Step 4: Verify Tables in Supabase

**Via Dashboard:**
- Go to Table Editor: https://app.supabase.com/project/xqndfgrhyekxttvsddob/editor
- Select "dash" schema (dropdown at top)
- Should see 12 tables

**Via SQL:**
```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'dash'
ORDER BY table_name;
```

Expected output (13 tables):
1. `__EFMigrationsHistory`
2. `account_balances`
3. `client_asset_allocations` ⭐
4. `clients`
5. `custody_wallets`
6. `PerformanceMetrics`
7. `PriceHistories`
8. `RebalancingAlerts`
9. `system_configuration`
10. `traditional_accounts`
11. `TransactionAudits`
12. `transactions`
13. `wallet_balances`

### Step 5: Test Connection

```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/DeFiDashboard.AppHost
dotnet run
```

Check:
- ✅ Aspire Dashboard opens
- ✅ ApiService shows green (Running)
- ✅ No database connection errors in logs

## 📝 Files Created/Modified

### Created:
1. **`migrations-dash-schema.sql`** - Complete SQL script for dash schema
2. **`DASH-SCHEMA-SETUP.md`** - This file (setup instructions)

### Modified:
1. **`src/ApiService/ApiService.csproj`** - Added UserSecretsId
2. **`src/ApiService/Common/Database/ApplicationDbContext.cs`** - Added `HasDefaultSchema("dash")`
3. **`APPLY-SCHEMA.md`** - Updated to reference dash schema

## 🔍 Schema Structure

```
postgres (database)
└── dash (schema)
    ├── clients
    ├── custody_wallets
    ├── traditional_accounts
    ├── client_asset_allocations (CRITICAL - allocation tracking)
    ├── wallet_balances
    ├── account_balances
    ├── transactions
    ├── TransactionAudits
    ├── PriceHistories
    ├── PerformanceMetrics
    ├── RebalancingAlerts
    ├── system_configuration
    └── __EFMigrationsHistory
```

## 🔗 Quick Links

- **Supabase Dashboard**: https://app.supabase.com/project/xqndfgrhyekxttvsddob
- **SQL Editor**: https://app.supabase.com/project/xqndfgrhyekxttvsddob/sql
- **Database Settings**: https://app.supabase.com/project/xqndfgrhyekxttvsddob/settings/database
- **Table Editor**: https://app.supabase.com/project/xqndfgrhyekxttvsddob/editor

## ⚠️ Important Notes

1. **Schema Name**: All tables are in "dash" schema, not "public"
2. **Connection String**: Must include password (get from Supabase Dashboard)
3. **User Secrets**: Never commit connection strings with passwords to git
4. **EF Migrations**: Already configured to use "dash" schema automatically

## 🐛 Troubleshooting

### "relation does not exist"
**Solution**: Schema not applied yet. Run migrations-dash-schema.sql

### "schema dash does not exist"
**Solution**: SQL script not run yet. Execute migrations-dash-schema.sql first

### "password authentication failed"
**Solution**: Wrong password in connection string. Get from Supabase → Settings → Database

### Can't see tables in Supabase Table Editor
**Solution**: Select "dash" schema from dropdown at top of Table Editor

## ✨ Next Steps

After tables are created and connection tested:

1. **Verify schema**: Run verification query to confirm 13 tables
2. **Test EF Core**: Run a simple query from API
3. **Phase 3**: Implement first vertical slice (Client CRUD endpoints)

---

**Ready to continue?** Once you've:
1. ✅ Set connection string with password
2. ✅ Applied migrations-dash-schema.sql
3. ✅ Verified tables exist in 'dash' schema

Let me know and we'll test the connection and move to Phase 3!
