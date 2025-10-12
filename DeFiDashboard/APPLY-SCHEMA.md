# Apply Database Schema to Supabase

## ✅ User Secrets Already Set

Your credentials are securely stored in user secrets:
- ✅ Supabase URL
- ✅ Supabase Anon Key
- ✅ Supabase Service Role Key
- ✅ Moralis API Key

## 🔑 Step 1: Get Database Password

You need the PostgreSQL database password to complete the connection string.

### Get Password from Supabase Dashboard:

1. Go to https://app.supabase.com/project/xqndfgrhyekxttvsddob
2. Click **Settings** (gear icon)
3. Click **Database**
4. Under **Database Settings**, find **Database password**
5. Click **Show** to reveal your password
6. Copy the password

### Set Connection String with Password:

```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/src/ApiService

# Replace YOUR_DB_PASSWORD with the actual password
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.xqndfgrhyekxttvsddob.supabase.co;Database=postgres;Username=postgres;Password=YOUR_DB_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

## 📊 Step 2: Apply Schema to Supabase

### Method 1: Supabase Dashboard SQL Editor (Easiest)

1. Go to https://app.supabase.com/project/xqndfgrhyekxttvsddob/sql
2. Click **New Query**
3. Copy the entire contents from: `/Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/migrations-dash-schema.sql`
4. Paste into the SQL Editor
5. Click **Run** (or press Cmd+Enter)
6. ✅ You should see "Success" and a list of tables created in the 'dash' schema

### Method 2: Use EF Core Migrations (After setting password)

```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/src/ApiService
dotnet ef database update

# Should output:
# Applying migration '20251012051546_InitialCreate'.
# Done.
```

### Method 3: Supabase MCP (If you have it configured)

If you have Supabase MCP configured in Claude Desktop or another tool:
1. Use the MCP tool to connect to your Supabase project
2. Execute the SQL from `migrations.sql`

## ✅ Step 3: Verify Tables Created

### Via Supabase Dashboard:

1. Go to https://app.supabase.com/project/xqndfgrhyekxttvsddob/editor
2. You should see 12 tables:
   - `clients`
   - `custody_wallets`
   - `traditional_accounts`
   - `client_asset_allocations` ⭐ (most important)
   - `wallet_balances`
   - `account_balances`
   - `transactions`
   - `transaction_audit`
   - `PriceHistories`
   - `performance_metrics`
   - `rebalancing_alerts`
   - `system_configuration`

### Via SQL Query:

Run this in SQL Editor to verify:
```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'dash'
ORDER BY table_name;
```

Should return 12 tables plus `__EFMigrationsHistory` (all in the 'dash' schema).

## 🚀 Step 4: Test Connection from API

```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/DeFiDashboard.AppHost
dotnet run
```

Check:
- ✅ Aspire Dashboard opens (https://localhost:17xxx)
- ✅ ApiService shows as "Running" (green)
- ✅ No database connection errors in logs

## 🔍 Troubleshooting

### "Connection string not found"
**Solution**: Run the dotnet user-secrets set command from Step 1

### "password authentication failed"
**Solution**: Double-check password from Supabase Dashboard → Settings → Database

### "no pg_hba.conf entry"
**Solution**: Ensure `SSL Mode=Require` is in connection string

### "relation does not exist"
**Solution**: Tables not created yet. Follow Step 2 to apply schema

## 📝 Your Current Configuration

**Project ID**: `xqndfgrhyekxttvsddob`
**Database Host**: `db.xqndfgrhyekxttvsddob.supabase.co`
**Database**: `postgres`
**User**: `postgres`

**Supabase Dashboard**: https://app.supabase.com/project/xqndfgrhyekxttvsddob
**SQL Editor**: https://app.supabase.com/project/xqndfgrhyekxttvsddob/sql
**Table Editor**: https://app.supabase.com/project/xqndfgrhyekxttvsddob/editor

---

## Next: Once tables are created and connection tested:

✨ **Phase 3**: Implement first vertical slice (Client CRUD endpoints)

Let me know when you've completed these steps!
