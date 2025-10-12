# Supabase Configuration Guide

## 🔑 Getting Your Supabase Connection String

### Step 1: Get Supabase Credentials

1. Go to [Supabase Dashboard](https://app.supabase.com/)
2. Select your project (or create a new one)
3. Click **Settings** (gear icon in sidebar)
4. Click **Database** in the left menu
5. Scroll down to **Connection string** section
6. Copy the **Connection string** (URI format)

### Step 2: Extract Connection Details

Your Supabase connection string looks like:
```
postgresql://postgres:[YOUR-PASSWORD]@db.PROJECT_ID.supabase.co:5432/postgres
```

Extract these values:
- **Host**: `db.PROJECT_ID.supabase.co`
- **Database**: `postgres`
- **Username**: `postgres`
- **Password**: `[YOUR-PASSWORD]` (from Database settings → Database password)
- **Project ID**: `PROJECT_ID` (from URL)

### Step 3: Configure ApiService

#### Option A: Update appsettings.json (Not Recommended - will be committed to git)

Edit `src/ApiService/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=db.YOUR_PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

#### Option B: Use User Secrets (Recommended - secure, not committed)

```bash
cd src/ApiService

# Set the connection string using user secrets
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.YOUR_PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

# Verify it was set
dotnet user-secrets list
```

#### Option C: Use Environment Variable (Production)

Set environment variable:
```bash
export ConnectionStrings__Supabase="Host=db.YOUR_PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

Or in Aspire AppHost, it's already configured to pass the connection string from the database resource.

## 📊 Apply Database Schema to Supabase

### Method 1: SQL Script (Recommended)

1. Open Supabase Dashboard
2. Go to **SQL Editor**
3. Click **New Query**
4. Copy contents from: `/Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/migrations.sql`
5. Paste into SQL Editor
6. Click **Run**

### Method 2: EF Core Migrations

After configuring connection string:

```bash
cd src/ApiService
dotnet ef database update
```

## ✅ Verify Connection

Test the connection:

```bash
cd src/ApiService
dotnet ef database update

# Should see:
# Build started...
# Build succeeded.
# Applying migration '20251012051546_InitialCreate'.
# Done.
```

Or run the API:

```bash
cd DeFiDashboard.AppHost
dotnet run

# Check Aspire Dashboard at https://localhost:17xxx
# API should start without connection errors
```

## 🔐 Security Best Practices

1. **Never commit credentials** to git
   - Use `.gitignore` to exclude appsettings with credentials
   - Prefer User Secrets for local development
   - Use environment variables for production

2. **Supabase Security**
   - Enable RLS (Row Level Security) in Supabase
   - Create service role for API access (not anon key)
   - Restrict IP access if possible

3. **Connection String in Aspire**
   - Aspire will provide the connection string via configuration
   - No need to hardcode in AppHost

## 🌐 Supabase Features to Enable

After applying schema, in Supabase Dashboard:

1. **Enable PostgREST API** (optional)
   - Auto-generates REST API from your tables
   - Useful for quick testing

2. **Enable Realtime** (optional)
   - Real-time subscriptions for table changes
   - Useful for live dashboard updates

3. **Enable Auth** (optional)
   - Supabase Auth for user authentication
   - Can integrate later for multi-user support

## 🔗 Connection String Format Reference

```
Host=db.PROJECT_ID.supabase.co;
Database=postgres;
Username=postgres;
Password=YOUR_PASSWORD;
SSL Mode=Require;
Trust Server Certificate=true;
Port=5432;
Pooling=true;
Minimum Pool Size=0;
Maximum Pool Size=100;
```

## 📝 External Provider Configuration

Also configure in `appsettings.json`:

```json
{
  "ExternalProviders": {
    "Moralis": {
      "ApiKey": "YOUR_MORALIS_API_KEY",
      "BaseUrl": "https://deep-index.moralis.io/api/v2.2"
    },
    "Pluggy": {
      "ClientId": "YOUR_PLUGGY_CLIENT_ID",
      "ClientSecret": "YOUR_PLUGGY_CLIENT_SECRET",
      "BaseUrl": "https://api.pluggy.ai"
    }
  }
}
```

Get API keys:
- **Moralis**: https://admin.moralis.io/register
- **Pluggy**: https://dashboard.pluggy.ai/

---

**Last Updated**: 2025-10-12
