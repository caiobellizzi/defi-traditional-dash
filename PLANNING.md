# DeFi-Traditional Finance Dashboard - Implementation Plan

**Version**: 2.0.0 - Vertical Slice Architecture
**Date**: 2025-10-12
**Architecture**: Vertical Slice + CQRS + .NET 9

---

## 🎯 Vertical Slice Architecture Overview

### Why Vertical Slices?

- **Feature-Centric**: Each slice represents a complete user story
- **Loose Coupling**: Features are independent and self-contained
- **Easy Testing**: Test entire feature in isolation
- **Team Scalability**: Different teams can work on different slices
- **Business Alignment**: Code structure matches business requirements

### Slice Structure

```
Feature → Command/Query → Validator → Handler → Endpoint
```

Each slice contains everything needed for one feature:
- Request/Response DTOs
- Business logic
- Validation rules
- Database access
- API endpoint

---

## 📋 Implementation Phases

### Phase 1: Project Setup with Aspire Orchestration (Week 1)

#### Step 1: Create Aspire Solution

```bash
# Install Aspire workload
dotnet workload install aspire

# Create new Aspire solution
dotnet new aspire -n DeFiDashboard

# This creates:
# - DeFiDashboard.AppHost (orchestrator)
# - DeFiDashboard.ServiceDefaults (shared configuration)
```

#### Step 2: Backend Setup (ApiService)

```bash
cd DeFiDashboard

# Create API project
dotnet new webapi -n ApiService -o src/ApiService

# Add to solution
dotnet sln add src/ApiService

# Add reference to ServiceDefaults
cd src/ApiService
dotnet add reference ../../DeFiDashboard.ServiceDefaults

# Install NuGet packages
dotnet add package MediatR
dotnet add package Carter
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Mapster
dotnet add package Serilog.AspNetCore
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.PostgreSql
```

**ApiService/Program.cs** (initial setup):
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (observability, health checks, etc.)
builder.AddServiceDefaults();

// Add services
builder.Services.AddCarter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
app.MapDefaultEndpoints(); // Aspire endpoints
app.UseSwagger();
app.UseSwaggerUI();
app.MapCarter();

app.Run();
```

#### Step 3: Frontend Setup (Vite + React)

```bash
# Create frontend project
cd ../..
npm create vite@latest frontend -- --template react-ts

# Install dependencies
cd frontend
npm install

# Install UI libraries
npm install @tanstack/react-query @tanstack/react-table
npm install react-router-dom axios
npm install react-hook-form zod @hookform/resolvers
npm install recharts
npm install -D tailwindcss postcss autoprefixer
npm install -D @playwright/test

# Initialize Tailwind
npx tailwindcss init -p

# Install shadcn/ui
npx shadcn-ui@latest init
```

#### Step 4: Add Frontend to Aspire AppHost

**Install Aspire Node Hosting package**:
```bash
cd ../DeFiDashboard.AppHost
dotnet add package Aspire.Hosting.NodeJs
```

**DeFiDashboard.AppHost/Program.cs**:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL (Supabase connection)
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "defi_dashboard");

var database = postgres.AddDatabase("defi-db");

// Add API Service
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithReference(database)
    .WithEnvironment("ConnectionStrings__Supabase", database);

// Add Frontend with Node.js hot-reload
var frontend = builder.AddNpmApp("frontend", "../frontend")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// Configure npm scripts
frontend.WithNpmCommand("dev"); // Use 'npm run dev' for development

builder.Build().Run();
```

**frontend/package.json** (ensure scripts are configured):
```json
{
  "scripts": {
    "dev": "vite --host",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

#### Step 5: Project Structure

After setup, your solution structure should look like:
```
DeFiDashboard/
├── DeFiDashboard.sln
├── DeFiDashboard.AppHost/
│   ├── Program.cs                    # Aspire orchestration
│   ├── DeFiDashboard.AppHost.csproj
│   └── appsettings.json
├── DeFiDashboard.ServiceDefaults/
│   ├── Extensions.cs
│   └── DeFiDashboard.ServiceDefaults.csproj
├── src/
│   └── ApiService/
│       ├── Features/                 # Vertical slices
│       ├── Common/
│       ├── BackgroundJobs/
│       ├── Program.cs
│       ├── ApiService.csproj
│       └── appsettings.json
└── frontend/
    ├── src/
    │   ├── features/                 # Feature slices
    │   ├── shared/
    │   ├── App.tsx
    │   └── main.tsx
    ├── package.json
    ├── vite.config.ts
    └── tsconfig.json
```

#### Step 6: Running with Aspire

```bash
# Run entire solution (backend + frontend + observability)
cd DeFiDashboard.AppHost
dotnet run

# Aspire Dashboard opens automatically at https://localhost:17243
# - API: https://localhost:7xxx
# - Frontend: http://localhost:5173 (with hot-reload)
# - Observability: Metrics, Logs, Traces
```

**Aspire Dashboard Features**:
- ✅ View all services (API + Frontend)
- ✅ Monitor health checks
- ✅ View logs in real-time
- ✅ Distributed tracing
- ✅ Metrics and performance
- ✅ Environment variables
- ✅ Service dependencies

#### Step 7: Frontend Hot-Reload Configuration

**vite.config.ts**:
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    host: true, // Required for Aspire
    strictPort: true,
    watch: {
      usePolling: true, // Better compatibility with containers
    },
  },
});
```

#### Step 8: Database Setup
- [ ] Configure Supabase connection string in AppHost
- [ ] Create database schema (use Supabase MCP)
- [ ] Set up EF Core migrations in ApiService
- [ ] Seed initial data

#### Checklist
- [ ] Install .NET 9 SDK
- [ ] Install Node.js 18+
- [ ] Install Aspire workload
- [ ] Create Aspire solution
- [ ] Add ApiService project
- [ ] Add Frontend project
- [ ] Configure AppHost to orchestrate both
- [ ] Test hot-reload for frontend
- [ ] Test API endpoints
- [ ] Verify Aspire dashboard connectivity

---

### Phase 2: Core Infrastructure (Week 1-2)

#### Backend Infrastructure
```
ApiService/
├── Common/
│   ├── Database/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Entities/
│   │   └── Configurations/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   └── LoggingBehavior.cs
│   ├── Abstractions/
│   │   └── Result.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
└── Program.cs
```

**Tasks**:
- [ ] Create `ApplicationDbContext`
- [ ] Define all EF Core entities
- [ ] Create MediatR pipeline behaviors (Validation, Logging)
- [ ] Implement Result pattern
- [ ] Configure Program.cs with MediatR, Carter, FluentValidation

#### Frontend Infrastructure
```
frontend/src/
├── shared/
│   ├── lib/
│   │   ├── api-client.ts
│   │   ├── utils.ts
│   │   └── queryClient.ts
│   ├── components/
│   │   ├── ui/              # shadcn/ui
│   │   └── layout/
│   └── types/
│       └── common.types.ts
└── App.tsx
```

**Tasks**:
- [ ] Set up Axios API client
- [ ] Configure TanStack Query client
- [ ] Install shadcn/ui components
- [ ] Create layout components (Sidebar, Header)
- [ ] Set up routing

---

### Phase 3: Client Management Feature (Week 2)

**First Vertical Slice: Complete client CRUD**

#### Backend Slices

```
Features/Clients/
├── Create/
│   ├── CreateClientCommand.cs
│   ├── CreateClientValidator.cs
│   ├── CreateClientHandler.cs
│   └── CreateClientEndpoint.cs
├── GetList/
│   ├── GetClientsQuery.cs
│   ├── GetClientsHandler.cs
│   └── GetClientsEndpoint.cs
├── GetById/
│   ├── GetClientByIdQuery.cs
│   ├── GetClientByIdHandler.cs
│   └── GetClientByIdEndpoint.cs
├── Update/
│   ├── UpdateClientCommand.cs
│   ├── UpdateClientValidator.cs
│   ├── UpdateClientHandler.cs
│   └── UpdateClientEndpoint.cs
└── Delete/
    ├── DeleteClientCommand.cs
    ├── DeleteClientHandler.cs
    └── DeleteClientEndpoint.cs
```

#### Frontend Slices

```
features/clients/
├── api/
│   └── clientsApi.ts
├── hooks/
│   ├── useClients.ts
│   ├── useClient.ts
│   ├── useCreateClient.ts
│   ├── useUpdateClient.ts
│   └── useDeleteClient.ts
├── components/
│   ├── ClientList.tsx
│   ├── ClientForm.tsx
│   ├── ClientDetail.tsx
│   └── ClientCard.tsx
├── types/
│   └── client.types.ts
└── routes/
    ├── ClientsPage.tsx
    └── ClientDetailPage.tsx
```

**Testing**:
- [ ] Unit tests for handlers
- [ ] Integration tests for endpoints
- [ ] Playwright E2E tests for client CRUD flow

---

### Phase 4: Provider Integration (Week 3-4)

#### Blockchain Provider (Moralis)

```
Common/Providers/
├── IBlockchainDataProvider.cs
├── MoralisProvider.cs
└── Models/
    ├── WalletBalance.cs
    └── TokenTransfer.cs
```

**Tasks**:
- [ ] Research Moralis SDK documentation (use context7)
- [ ] Define `IBlockchainDataProvider` interface
- [ ] Implement `MoralisProvider`
- [ ] Create configuration section
- [ ] Test with sample wallet addresses

#### OpenFinance Provider (Pluggy)

```
Common/Providers/
├── IOpenFinanceProvider.cs
├── PluggyProvider.cs
└── Models/
    ├── AccountSummary.cs
    ├── AccountBalance.cs
    └── AccountTransaction.cs
```

**Tasks**:
- [ ] Research Pluggy SDK documentation (use context7)
- [ ] Define `IOpenFinanceProvider` interface
- [ ] Implement `PluggyProvider`
- [ ] Create configuration section
- [ ] Test with sandbox accounts

#### Wallet Management Slices

```
Features/Wallets/
├── Add/
├── GetList/
├── GetBalances/
└── Sync/
```

#### Account Management Slices

```
Features/Accounts/
├── GetList/
├── CreateConnectToken/
├── HandleCallback/
└── Sync/
```

---

### Phase 5: Allocation System (Week 5)

**Critical Feature: Client Asset Allocations**

#### Backend Slices

```
Features/Allocations/
├── Create/
│   ├── CreateAllocationCommand.cs
│   ├── CreateAllocationValidator.cs (prevent over-allocation)
│   ├── CreateAllocationHandler.cs
│   └── CreateAllocationEndpoint.cs
├── Update/
├── GetByClient/
├── End/
└── Validate/
    └── AllocationValidator.cs (check total allocations)
```

#### Frontend Slices

```
features/allocations/
├── api/
├── hooks/
├── components/
│   ├── AllocationForm.tsx
│   ├── AllocationList.tsx
│   └── AllocationChart.tsx
└── routes/
```

**Business Rules**:
- Total allocations for an asset cannot exceed 100% (if percentage-based)
- Validate no overlapping date ranges for same client-asset pair
- Calculate client balances based on allocations

---

### Phase 6: Transaction Management (Week 6)

#### Backend Slices

```
Features/Transactions/
├── GetList/
├── GetById/
├── CreateManual/
├── GetAuditTrail/
└── Sync/
    ├── SyncWalletTransactionsCommand.cs
    └── SyncAccountTransactionsCommand.cs
```

#### Frontend Slices

```
features/transactions/
├── api/
├── hooks/
├── components/
│   ├── TransactionList.tsx
│   ├── TransactionDetail.tsx
│   ├── ManualTransactionForm.tsx
│   └── AuditTrail.tsx
└── routes/
```

**Audit Trail**:
- Every transaction change logged to `TransactionAudit`
- Store old and new data as JSONB
- Track who made the change and when

---

### Phase 7: Background Jobs (Week 7)

#### Hangfire Jobs

```
BackgroundJobs/
├── MoralisSyncJob.cs       # Sync wallet balances
├── PluggySyncJob.cs        # Sync account balances
├── PortfolioCalculationJob.cs  # Calculate client portfolios
└── RebalancingAlertJob.cs  # Check for allocation drift
```

**Job Configuration**:
- Store intervals in `SystemConfiguration` table
- Allow admin to change intervals via UI
- Implement retry logic with exponential backoff
- Log job execution results

---

### Phase 8: Portfolio Consolidation (Week 8-9)

#### Backend Slices

```
Features/Portfolio/
├── GetOverview/
│   └── GetPortfolioOverviewQuery.cs
├── GetConsolidated/
│   └── GetConsolidatedPortfolioQuery.cs
├── GetClientPortfolio/
│   └── GetClientPortfolioQuery.cs
└── Recalculate/
    └── RecalculatePortfolioCommand.cs
```

**Portfolio Calculation Logic**:
```csharp
For each client:
  1. Get all active allocations
  2. For each allocation:
     - If wallet: Get WalletBalances × allocation %
     - If account: Get AccountBalances × allocation %
  3. Sum total value (crypto + traditional)
  4. Calculate ROI, P&L
  5. Store in PerformanceMetrics
```

#### Frontend Slices

```
features/portfolio/
├── components/
│   ├── PortfolioOverview.tsx
│   ├── AssetAllocation.tsx
│   ├── PerformanceChart.tsx
│   └── ClientPortfolio.tsx
└── routes/
    └── PortfolioPage.tsx
```

---

### Phase 9: Analytics & Export (Week 10)

#### Backend Slices

```
Features/Analytics/
├── GetPerformance/
└── GetAllocationDrift/

Features/Export/
├── ExportPortfolioPdf/
├── ExportTransactionsExcel/
└── ExportPerformanceExcel/
```

#### Frontend Slices

```
features/analytics/
└── routes/
    └── AnalyticsPage.tsx

features/dashboard/
└── routes/
    └── DashboardPage.tsx
```

---

### Phase 10: Alerts & Monitoring (Week 11)

#### Backend Slices

```
Features/Alerts/
├── GetList/
├── Acknowledge/
└── Resolve/
```

#### Frontend Slices

```
features/alerts/
├── components/
│   ├── AlertBadge.tsx
│   └── AlertList.tsx
└── routes/
```

---

### Phase 11: Testing & Quality (Week 12-13)

#### Backend Testing
- [ ] Unit tests for all handlers (target: 80% coverage)
- [ ] Integration tests for all endpoints
- [ ] Test provider abstractions with mocks
- [ ] Test background jobs
- [ ] Load testing with sample data

#### Frontend Testing
- [ ] Component tests with React Testing Library
- [ ] Playwright E2E tests for critical flows:
  - Client creation → Allocation → Portfolio view
  - Wallet/Account connection
  - Transaction history
  - Export functionality
- [ ] Accessibility testing
- [ ] Performance testing (Lighthouse)

#### Quality Checks
- [ ] Code review checklist
- [ ] Security audit (OWASP Top 10)
- [ ] Performance profiling
- [ ] Database query optimization

---

## 🔧 Technical Implementation Details

### Backend: Program.cs Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddCarter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register providers
builder.Services.AddScoped<IBlockchainDataProvider, MoralisProvider>();
builder.Services.AddScoped<IOpenFinanceProvider, PluggyProvider>();

// Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("Supabase")));
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapCarter(); // Auto-discovers all ICarterModule endpoints
app.UseHangfireDashboard("/hangfire");

app.Run();
```

### Frontend: API Client Setup

```typescript
// shared/lib/api-client.ts
import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    // Handle errors globally
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Frontend: Query Client Setup

```typescript
// shared/lib/queryClient.ts
import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
    },
  },
});
```

---

## 📊 Database Schema (Use Supabase MCP)

See complete schema in `database-schema.sql`

**Critical Tables**:
1. Clients
2. CustodyWallets
3. TraditionalAccounts
4. **ClientAssetAllocations** (heart of the system)
5. WalletBalances
6. AccountBalances
7. Transactions
8. TransactionAudit
9. PriceHistory
10. PerformanceMetrics
11. RebalancingAlerts
12. SystemConfiguration

---

## 🧪 Testing Strategy

### Backend Unit Tests (xUnit + FluentAssertions)

```csharp
public class CreateClientHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesClient()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var handler = new CreateClientHandler(context);
        var command = new CreateClientCommand("John Doe", "john@test.com", null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var client = await context.Clients.FindAsync(result.Value);
        client.Should().NotBeNull();
        client!.Name.Should().Be("John Doe");
    }
}
```

### Frontend E2E Tests (Playwright)

```typescript
// tests/e2e/clients.spec.ts
import { test, expect } from '@playwright/test';

test('create new client', async ({ page }) => {
  await page.goto('/clients');
  await page.click('button:has-text("Add Client")');

  await page.fill('input[name="name"]', 'Test Client');
  await page.fill('input[name="email"]', 'test@example.com');
  await page.click('button:has-text("Save")');

  await expect(page.locator('text=Test Client')).toBeVisible();
});
```

---

## 🚀 Deployment Readiness

### Prerequisites
- [ ] Environment variables configured
- [ ] Database migrations applied
- [ ] API keys obtained (Moralis, Pluggy)
- [ ] Supabase project set up
- [ ] Hangfire dashboard secured

### Future Enhancements
- Client portal with authentication
- Multi-user RBAC
- Brazilian tax reporting
- NFT tracking
- Automated rebalancing execution

---

**Status**: Planning Complete ✅
**Next**: Begin Phase 1 implementation
**Last Updated**: 2025-10-12
