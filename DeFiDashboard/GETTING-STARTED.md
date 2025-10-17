# 🚀 Getting Started - DeFi-Traditional Finance Dashboard

**Quick start completed!** Your application is ready to run.

---

## ✅ Current Status

- ✅ Prerequisites verified (.NET 9.0.305, Node.js v22.16.0)
- ✅ Frontend dependencies installed
- ✅ Build successful (0 errors)
- 🟡 Database: Use Aspire (recommended) or local PostgreSQL

---

## 🎯 Option 1: Start with Aspire (Recommended - No Database Setup)

Aspire will automatically handle the database, frontend, and backend for you.

### Start the Application:

```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/DeFiDashboard.AppHost
dotnet run
```

### What Aspire Does:
- ✅ Starts PostgreSQL container automatically
- ✅ Runs database migrations
- ✅ Seeds initial data (5 sample clients, 2 wallets)
- ✅ Starts backend API
- ✅ Starts frontend (Vite dev server)
- ✅ Provides observability dashboard

### Access Points:
- **Aspire Dashboard**: https://localhost:17243 (check this first!)
- **Frontend**: http://localhost:5173 (or check Aspire for actual URL)
- **API**: Check Aspire dashboard for the actual URL (usually https://localhost:7xxx)
- **Swagger**: {API_URL}/swagger
- **Hangfire**: {API_URL}/hangfire

### Expected Output:
```
info: Aspire.Hosting.DistributedApplication[0]
      Aspire version: 9.0.0
info: Aspire.Hosting.DistributedApplication[0]
      Distributed application starting.
info: Aspire.Hosting.DistributedApplication[0]
      Application host directory is: /Users/.../DeFiDashboard.AppHost
info: Aspire.Hosting.DistributedApplication[0]
      Now listening on: https://localhost:17243
```

**Then open**: https://localhost:17243 in your browser

---

## 🎯 Option 2: Manual Setup (Local PostgreSQL)

If you prefer to manage services separately:

### 1. Start PostgreSQL:

**macOS (Homebrew)**:
```bash
brew services start postgresql@15
```

**Or Docker**:
```bash
docker run -d \
  --name defi-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=defi_dashboard \
  -p 5432:5432 \
  postgres:15
```

### 2. Create Database (if needed):
```bash
createdb defi_dashboard
```

### 3. Run Migrations:
```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/src/ApiService
dotnet ef database update
```

### 4. Start Backend:
```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/src/ApiService
dotnet run
```

### 5. Start Frontend (separate terminal):
```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/frontend
npm run dev
```

---

## 🎯 Option 3: Use Supabase (Cloud Database)

### 1. Create Supabase Project:
- Go to https://supabase.com
- Create new project
- Get connection string from Settings > Database

### 2. Update Configuration:
```bash
cd /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard

# Copy template
cp src/ApiService/appsettings.Development.json.template \
   src/ApiService/appsettings.Development.json

# Edit and add your Supabase connection string
nano src/ApiService/appsettings.Development.json
```

Update the connection string:
```json
{
  "ConnectionStrings": {
    "Supabase": "postgresql://postgres:[YOUR-PASSWORD]@[YOUR-PROJECT-REF].supabase.co:5432/postgres"
  }
}
```

### 3. Run Migrations:
```bash
cd src/ApiService
dotnet ef database update
```

### 4. Start with Aspire:
```bash
cd ../DeFiDashboard.AppHost
dotnet run
```

---

## 🔧 Add API Credentials (Optional but Recommended)

To enable full functionality (wallet sync, account sync):

### 1. Edit Configuration:
```bash
nano /Users/caiobellizzi/Projects/defi-traditional-dash/DeFiDashboard/src/ApiService/appsettings.Development.json
```

### 2. Add Your Credentials:
```json
{
  "ExternalProviders": {
    "Moralis": {
      "ApiKey": "your-moralis-api-key-here",
      "BaseUrl": "https://deep-index.moralis.io/api/v2.2"
    },
    "Pluggy": {
      "ClientId": "your-pluggy-client-id-here",
      "ClientSecret": "your-pluggy-client-secret-here",
      "BaseUrl": "https://api.pluggy.ai"
    }
  }
}
```

### 3. Get Credentials:

**Moralis** (Blockchain Data):
1. Sign up: https://moralis.io
2. Create new project
3. Copy API key from dashboard
4. Free tier: 40,000 requests/day

**Pluggy** (OpenFinance):
1. Sign up: https://pluggy.ai
2. Request sandbox access
3. Get credentials from dashboard
4. Approval may take 24-48 hours

**See**: `API-CREDENTIALS-GUIDE.md` for detailed instructions

---

## 🧪 Verify Everything Works

### 1. Check Aspire Dashboard:
- Open https://localhost:17243
- Verify all services are "Running" (green)
- Check logs for any errors

### 2. Access Frontend:
- Open http://localhost:5173
- You should see the dashboard with seeded data
- 5 sample clients should be visible

### 3. Test API:
- Open {API_URL}/swagger
- Try GET /api/clients
- Should return 5 clients

### 4. Check Hangfire:
- Open {API_URL}/hangfire
- Verify background jobs are registered:
  - wallet-sync (every 5 min)
  - account-sync (every 15 min)
  - portfolio-calculation (every hour)
  - alert-generation (every 30 min)
  - export-cleanup (daily at 3 AM)

### 5. Test Features:
- ✅ Add a new client
- ✅ Add a test wallet: `0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb`
- ✅ View portfolio page
- ✅ Check alerts page
- ✅ Test real-time connection (check browser console for "SignalR connected")

---

## 🐛 Troubleshooting

### Issue: Database connection refused

**Solution**: Use Aspire (Option 1) which handles database automatically.

Or start PostgreSQL:
```bash
# macOS
brew services start postgresql@15

# Or check if it's running
pg_isready
```

### Issue: Port already in use

**Solution**: Kill existing processes:
```bash
# Find process using port 5173 (frontend)
lsof -ti:5173 | xargs kill -9

# Find process using port 7xxx (backend)
lsof -ti:7185 | xargs kill -9
```

### Issue: Aspire dashboard not loading

**Solution**: Check terminal output for the actual URL. It might be different than expected.

### Issue: Frontend build errors

**Solution**: Rebuild frontend:
```bash
cd frontend
rm -rf node_modules
npm install
npm run dev
```

### Issue: Migration errors

**Solution**:
1. Check database is running: `pg_isready`
2. Check connection string in appsettings.Development.json
3. Try dropping and recreating database:
   ```bash
   dropdb defi_dashboard
   createdb defi_dashboard
   cd src/ApiService
   dotnet ef database update
   ```

---

## 📊 What You'll See

### Seeded Data (Automatic):

**5 Sample Clients**:
1. John Doe - VIP client
2. Jane Smith - Corporate account manager
3. Tech Ventures LLC - Investment fund
4. Maria Silva
5. Global Investments Inc - Institutional investor

**2 Sample Wallets**:
1. Main ETH Wallet - `0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb`
2. DeFi Operations Wallet - `0x8626f6940E2eb28930eFb4CeF49B2d1F2C9C1199`

### Available Features:

**Without API Credentials**:
- ✅ Client management (CRUD)
- ✅ Wallet management (add, view, edit)
- ✅ Manual transactions
- ✅ Portfolio views
- ✅ Analytics (with mock data)
- ✅ Alerts management
- ✅ Export (generates placeholder files)

**With API Credentials**:
- ✅ Real wallet balance sync (Moralis)
- ✅ Traditional account sync (Pluggy)
- ✅ Automatic background jobs
- ✅ Real-time balance updates
- ✅ Allocation drift detection
- ✅ Complete portfolio calculations

---

## 🎯 Next Steps

### Immediate:
1. ✅ Start application (Option 1 recommended)
2. ✅ Explore the UI
3. ✅ Test CRUD operations
4. ✅ Check Hangfire dashboard

### When Ready:
1. 🔑 Add Moralis API key
2. 🔑 Add Pluggy credentials
3. 🧪 Test provider integrations
4. 📊 Monitor background jobs

### Later:
1. 🔐 Add authentication (JWT)
2. 🧪 Write integration tests
3. 🚀 Deploy to production
4. 📈 Monitor and optimize

---

## 📚 Additional Resources

- **ENVIRONMENT-SETUP.md** - Detailed setup instructions
- **API-CREDENTIALS-GUIDE.md** - How to get API keys
- **API-DOCUMENTATION.md** - Complete API reference
- **USER-GUIDE.md** - End-user documentation
- **DEPLOYMENT.md** - Production deployment
- **TESTING-GUIDE.md** - Testing best practices

---

## 🎉 You're All Set!

Your DeFi-Traditional Finance Dashboard is ready to use!

**Start now**:
```bash
cd DeFiDashboard.AppHost && dotnet run
```

Then open **https://localhost:17243** to see the Aspire Dashboard.

Happy coding! 🚀
