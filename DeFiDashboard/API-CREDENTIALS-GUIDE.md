# API Credentials Quick Reference

This guide helps you quickly obtain and configure all required API credentials for the DeFi Dashboard.

## Required Credentials

| Provider | Purpose | Required For | Free Tier |
|----------|---------|--------------|-----------|
| **Moralis** | Blockchain data (wallets, balances, transactions) | DeFi features | 40,000 requests/day |
| **Pluggy** | OpenFinance/traditional accounts | Traditional finance features | Sandbox available |
| **PostgreSQL** | Database | All features | Free (local/Supabase) |

---

## 1. Moralis (Blockchain Data)

### What it provides:
- Wallet balances across multiple chains
- Token transfers and transaction history
- Real-time price data
- NFT data (optional)

### Sign up:
1. Go to [moralis.io](https://moralis.io)
2. Click "Start for Free"
3. Create account (GitHub OAuth available)

### Get API Key:
1. Navigate to [admin.moralis.io](https://admin.moralis.io)
2. Select your project (or create new one)
3. Click "Settings" → "API Keys"
4. Copy "Web3 API Key"

### Add to configuration:
```json
{
  "ExternalProviders": {
    "Moralis": {
      "ApiKey": "paste-your-key-here"
    }
  }
}
```

### Supported Chains:
- Ethereum Mainnet
- Polygon
- BNB Smart Chain
- Avalanche
- Fantom
- Arbitrum
- Optimism
- Base
- Cronos

### Rate Limits (Free Tier):
- 40,000 compute units/day
- ~1,500-2,000 wallet queries/day
- Automatically resets daily

### Test Your Integration:
```bash
curl -X GET \
  "https://deep-index.moralis.io/api/v2.2/0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb/balance?chain=eth" \
  -H "X-API-Key: YOUR_API_KEY"
```

### Pricing:
- **Free**: 40,000 CU/day
- **Pro**: $49/month - 250,000 CU/day
- **Business**: $249/month - 1,500,000 CU/day

---

## 2. Pluggy (OpenFinance / Traditional Accounts)

### What it provides:
- Bank account connections (Brazil OpenFinance)
- Account balances
- Transaction history
- Investment accounts
- Credit card data

### Sign up:
1. Go to [pluggy.ai](https://pluggy.ai)
2. Click "Get Started"
3. Fill out company information
4. Wait for approval (usually 24-48 hours)

### Get Credentials:
1. Login to [dashboard.pluggy.ai](https://dashboard.pluggy.ai)
2. Navigate to "API Keys"
3. Find "Sandbox" section (for development)
4. Copy "Client ID" and "Client Secret"

### Add to configuration:
```json
{
  "ExternalProviders": {
    "Pluggy": {
      "ClientId": "paste-client-id-here",
      "ClientSecret": "paste-client-secret-here"
    }
  }
}
```

### Supported Institutions:
- 50+ Brazilian banks (via OpenFinance)
- Investment platforms (B3, XP, etc.)
- Digital wallets
- Credit card providers

### Test Credentials (Sandbox):
```
Institution: Pluggy Bank (sandbox)
Username: user-ok
Password: password-ok
```

### OAuth Flow:
1. Backend generates connect token
2. Frontend opens Pluggy Connect widget
3. User selects institution and authenticates
4. Pluggy redirects with authorization code
5. Backend exchanges code for access token
6. Accounts are synced automatically

### Rate Limits (Sandbox):
- Unlimited requests
- Limited to test institutions
- No real bank data

### Pricing:
- **Sandbox**: Free (test data only)
- **Production**: Custom pricing based on volume
- Contact sales for production access

---

## 3. PostgreSQL Database

### Option A: Local PostgreSQL (Development)

**Install PostgreSQL 15+**:

**macOS**:
```bash
brew install postgresql@15
brew services start postgresql@15
createdb defi_dashboard
```

**Ubuntu/Debian**:
```bash
sudo apt install postgresql-15
sudo systemctl start postgresql
sudo -u postgres createdb defi_dashboard
```

**Windows**:
Download from [postgresql.org/download/windows](https://www.postgresql.org/download/windows/)

**Connection String**:
```
Host=localhost;Database=defi_dashboard;Username=postgres;Password=postgres
```

### Option B: Supabase (Cloud PostgreSQL)

**Why Supabase?**
- Free tier: 500MB database
- Built-in authentication (optional)
- Auto-scaling
- Real-time subscriptions (optional)
- Automatic backups

**Setup**:
1. Go to [supabase.com](https://supabase.com)
2. Click "Start your project"
3. Create new organization
4. Create new project:
   - Name: `defi-dashboard`
   - Database Password: (generate strong password)
   - Region: Select closest to you

**Get Connection String**:
1. Navigate to Settings → Database
2. Scroll to "Connection string"
3. Select "Connection pooling" → "Transaction mode"
4. Copy the connection string
5. Replace `[YOUR-PASSWORD]` with your actual password

**Connection String Format**:
```
Host=db.PROJECT_ID.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**Free Tier Limits**:
- 500MB database storage
- 1GB file storage
- 2GB bandwidth
- 50,000 monthly active users

---

## Configuration Files

### Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=localhost;Database=defi_dashboard;Username=postgres;Password=postgres"
  },
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
  },
  "BackgroundJobs": {
    "MoralisSyncIntervalMinutes": 5,
    "PluggySyncIntervalMinutes": 10,
    "PortfolioCalculationIntervalMinutes": 15
  }
}
```

### Frontend (.env)

```bash
VITE_API_BASE_URL=https://localhost:7185
```

---

## Security Best Practices

### Development:
- Use separate API keys for dev/staging/production
- Never commit real credentials to git
- Use `.template` files with placeholders
- Keep `.env` and `appsettings.Development.json` local only

### Production:
- Use environment variables or Key Vault
- Rotate API keys regularly
- Monitor API usage/costs
- Enable IP restrictions where possible
- Use read-only database users for reporting

---

## Verification Checklist

After configuring all credentials:

- [ ] Moralis API key added to `appsettings.Development.json`
- [ ] Pluggy credentials added to `appsettings.Development.json`
- [ ] Database connection string configured
- [ ] `dotnet ef database update` runs successfully
- [ ] Application starts without errors
- [ ] Hangfire dashboard accessible at `/hangfire`
- [ ] Background jobs scheduled and running
- [ ] Test wallet sync completes successfully
- [ ] Logs show no authentication errors

---

## Testing Your Setup

### 1. Start Application
```bash
cd DeFiDashboard.AppHost
dotnet run
```

### 2. Check Aspire Dashboard
Open https://localhost:17243
- Verify all services are running
- Check logs for errors

### 3. Test Moralis Integration
1. Navigate to Wallets page in UI
2. Add test wallet: `0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb` (Ethereum)
3. Go to Hangfire dashboard: https://localhost:7185/hangfire
4. Find "Moralis Sync" job
5. Click "Trigger now"
6. Check logs for successful sync

### 4. Test Pluggy Integration
1. Navigate to Accounts page in UI
2. Click "Connect Account"
3. Should redirect to Pluggy Connect
4. Select "Pluggy Bank" (sandbox)
5. Use test credentials
6. Verify accounts appear in UI

### 5. Verify Data
```sql
-- Connect to database
psql -d defi_dashboard

-- Check synced data
SELECT * FROM CustodyWallets;
SELECT * FROM WalletBalances;
SELECT * FROM TraditionalAccounts;
SELECT * FROM AccountBalances;
```

---

## Common Issues

### "Invalid API Key" (Moralis)
- Verify key is copied correctly (no extra spaces)
- Check if key is for correct environment (dev/prod)
- Verify account is active and not suspended

### "Authentication failed" (Pluggy)
- Ensure using sandbox credentials for development
- Check ClientId and ClientSecret match
- Verify Pluggy account is approved

### "Connection refused" (Database)
- PostgreSQL service not running
- Wrong host/port in connection string
- Firewall blocking connection
- For Supabase: Check SSL settings

---

## Support Resources

- **Moralis**: [docs.moralis.io](https://docs.moralis.io)
- **Pluggy**: [docs.pluggy.ai](https://docs.pluggy.ai)
- **Supabase**: [supabase.com/docs](https://supabase.com/docs)
- **Project Docs**: See `ENVIRONMENT-SETUP.md`

---

**Last Updated**: 2025-10-16
