# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**DeFi-Traditional Finance Dashboard** - A comprehensive wealth management system for fund advisors managing custody assets across both DeFi (crypto) and traditional finance (bank accounts, investments) on behalf of multiple clients.

### Business Model

- **Custody Model**: Wallets and accounts belong to the fund/advisor, not individual clients
- **Allocation System**: Clients have allocated shares (percentage or fixed amount) of custody assets
- **Consolidated Tracking**: One wallet can be split across multiple client portfolios
- **Read-Only Monitoring**: No private key management required (monitoring only)

## Architecture Style

### ⭐ Vertical Slice Architecture (VSA)

This project uses **Vertical Slice Architecture** for both backend and frontend, organizing code by **features/use cases** rather than technical layers.

**Why VSA?**
- Each feature is self-contained and independently deployable
- Reduces coupling between features
- Easier to understand (all related code in one place)
- Better aligns with business requirements
- Simplifies testing (test entire feature slice)

**Key Principle**: When adding a new feature, create a new vertical slice containing ALL code needed for that feature.

## Technology Stack

### Orchestration
- **.NET Aspire** - Cloud-native orchestration and observability
  - Orchestrates backend API + frontend + database
  - Built-in observability (logs, traces, metrics)
  - Service discovery and configuration
  - Health checks and monitoring
  - Local development experience identical to production

### Backend
- **.NET 9** with **Aspire** for orchestration
- **MediatR** for CQRS pattern (Commands/Queries)
- **Entity Framework Core 9** for ORM
- **FluentValidation** for request validation
- **Supabase PostgreSQL** for database
- **Hangfire** for background jobs
- **Carter** for minimal API endpoints
- **Mapster** for object mapping
- **Serilog** for logging
- **Moralis SDK** for blockchain data (abstracted via `IBlockchainDataProvider`)
- **Pluggy SDK** for OpenFinance/traditional accounts (abstracted via `IOpenFinanceProvider`)

### Frontend
- **React 18.3+** with **TypeScript**
- **Vite 5+** for build tooling
- **Integrated with Aspire** via Node.js hosting (hot-reload enabled)
- **Feature-based organization** (vertical slices)
- **shadcn/ui + Tailwind CSS** for UI
- **React Router v6** for routing
- **TanStack Query** for server state
- **Zustand** for client state (optional)
- **Recharts** for analytics charts
- **TanStack Table** for data tables
- **React Hook Form + Zod** for forms

## Backend Architecture (Vertical Slice)

### Project Structure

```
backend/
├── DeFiDashboard.sln
├── src/
│   ├── ApiService/                     # Main API project
│   │   ├── Features/                   # 🔥 Feature slices (vertical)
│   │   │   ├── Clients/
│   │   │   │   ├── Create/
│   │   │   │   │   ├── CreateClientCommand.cs
│   │   │   │   │   ├── CreateClientHandler.cs
│   │   │   │   │   ├── CreateClientValidator.cs
│   │   │   │   │   └── CreateClientEndpoint.cs
│   │   │   │   ├── GetById/
│   │   │   │   │   ├── GetClientByIdQuery.cs
│   │   │   │   │   ├── GetClientByIdHandler.cs
│   │   │   │   │   └── GetClientByIdEndpoint.cs
│   │   │   │   ├── GetList/
│   │   │   │   ├── Update/
│   │   │   │   ├── Delete/
│   │   │   │   └── GetPortfolio/
│   │   │   ├── Wallets/
│   │   │   │   ├── Add/
│   │   │   │   ├── GetList/
│   │   │   │   ├── Sync/
│   │   │   │   └── GetBalances/
│   │   │   ├── Accounts/
│   │   │   │   ├── GetList/
│   │   │   │   ├── CreateConnectToken/
│   │   │   │   ├── HandleCallback/
│   │   │   │   └── Sync/
│   │   │   ├── Allocations/
│   │   │   │   ├── Create/
│   │   │   │   ├── Update/
│   │   │   │   ├── GetByClient/
│   │   │   │   └── End/
│   │   │   ├── Transactions/
│   │   │   │   ├── GetList/
│   │   │   │   ├── GetById/
│   │   │   │   ├── CreateManual/
│   │   │   │   └── GetAuditTrail/
│   │   │   ├── Portfolio/
│   │   │   │   ├── GetOverview/
│   │   │   │   ├── GetConsolidated/
│   │   │   │   ├── GetClientPortfolio/
│   │   │   │   └── Recalculate/
│   │   │   ├── Analytics/
│   │   │   │   ├── GetPerformance/
│   │   │   │   └── GetAllocationDrift/
│   │   │   ├── Export/
│   │   │   │   ├── ExportPortfolioPdf/
│   │   │   │   ├── ExportTransactionsExcel/
│   │   │   │   └── ExportPerformanceExcel/
│   │   │   └── Alerts/
│   │   │       ├── GetList/
│   │   │       ├── Acknowledge/
│   │   │       └── Resolve/
│   │   ├── Common/                     # Shared infrastructure
│   │   │   ├── Database/
│   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   ├── Entities/          # EF Core entities
│   │   │   │   └── Configurations/    # EF configurations
│   │   │   ├── Providers/
│   │   │   │   ├── IBlockchainDataProvider.cs
│   │   │   │   ├── MoralisProvider.cs
│   │   │   │   ├── IOpenFinanceProvider.cs
│   │   │   │   └── PluggyProvider.cs
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   └── Extensions/
│   │   ├── BackgroundJobs/
│   │   │   ├── MoralisSyncJob.cs
│   │   │   ├── PluggySyncJob.cs
│   │   │   ├── PortfolioCalculationJob.cs
│   │   │   └── RebalancingAlertJob.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── AppHost/                        # Aspire AppHost
│       └── Program.cs
└── tests/
    ├── ApiService.Tests/
    └── ApiService.IntegrationTests/
```

### Vertical Slice Pattern (Backend)

Each feature slice contains:

1. **Command/Query**: The request DTO
2. **Handler**: MediatR handler with business logic
3. **Validator**: FluentValidation rules
4. **Endpoint**: Carter minimal API endpoint
5. **DTOs**: Request/response models (if needed)

**Example: Create Client Feature**

```csharp
// Features/Clients/Create/CreateClientCommand.cs
public record CreateClientCommand(
    string Name,
    string Email,
    string? Document,
    string? PhoneNumber,
    string? Notes
) : IRequest<Result<Guid>>;

// Features/Clients/Create/CreateClientValidator.cs
public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Document).MaximumLength(50);
    }
}

// Features/Clients/Create/CreateClientHandler.cs
public class CreateClientHandler : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;

    public CreateClientHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreateClientCommand request,
        CancellationToken ct)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Document = request.Document,
            PhoneNumber = request.PhoneNumber,
            Notes = request.Notes,
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);

        return Result.Success(client.Id);
    }
}

// Features/Clients/Create/CreateClientEndpoint.cs
public class CreateClientEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/clients", async (
            CreateClientCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/clients/{result.Value}", result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateClient")
        .WithTags("Clients")
        .WithOpenApi();
    }
}
```

### CQRS Pattern

- **Commands**: Change state (Create, Update, Delete) → Return `Result<T>`
- **Queries**: Read state (GetById, GetList) → Return `Result<TDto>`

```csharp
// Command example
public record CreateClientCommand(...) : IRequest<Result<Guid>>;

// Query example
public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;
```

### MediatR Pipeline Behaviors

Global behaviors applied to all requests:

```csharp
// Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

## Frontend Architecture (Feature-Based)

### Project Structure

```
frontend/
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── routes.tsx
│   ├── features/                      # 🔥 Feature slices (vertical)
│   │   ├── clients/
│   │   │   ├── api/
│   │   │   │   └── clientsApi.ts
│   │   │   ├── components/
│   │   │   │   ├── ClientList.tsx
│   │   │   │   ├── ClientDetail.tsx
│   │   │   │   ├── ClientForm.tsx
│   │   │   │   └── ClientPortfolio.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useClients.ts
│   │   │   │   ├── useClient.ts
│   │   │   │   └── useClientPortfolio.ts
│   │   │   ├── types/
│   │   │   │   └── client.types.ts
│   │   │   └── routes/
│   │   │       ├── ClientsPage.tsx
│   │   │       └── ClientDetailPage.tsx
│   │   ├── wallets/
│   │   │   ├── api/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   ├── types/
│   │   │   └── routes/
│   │   ├── accounts/
│   │   ├── allocations/
│   │   ├── transactions/
│   │   ├── portfolio/
│   │   ├── analytics/
│   │   ├── alerts/
│   │   └── dashboard/
│   ├── shared/                        # Shared components/utilities
│   │   ├── components/
│   │   │   ├── ui/                   # shadcn/ui
│   │   │   ├── layout/
│   │   │   ├── DataTable/
│   │   │   ├── StatCard/
│   │   │   └── Charts/
│   │   ├── lib/
│   │   │   ├── api-client.ts
│   │   │   ├── utils.ts
│   │   │   └── constants.ts
│   │   ├── hooks/
│   │   └── types/
│   └── styles/
│       └── globals.css
├── package.json
├── vite.config.ts
└── tsconfig.json
```

### Feature Slice Pattern (Frontend)

Each feature contains:

1. **API**: API calls for the feature
2. **Components**: Feature-specific components
3. **Hooks**: TanStack Query hooks for data fetching
4. **Types**: TypeScript types
5. **Routes**: Route components

**Example: Clients Feature**

```typescript
// features/clients/api/clientsApi.ts
import { apiClient } from '@/shared/lib/api-client';
import { Client, CreateClientDto, ClientPortfolio } from '../types/client.types';

export const clientsApi = {
  getAll: () => apiClient.get<Client[]>('/api/clients'),
  getById: (id: string) => apiClient.get<Client>(`/api/clients/${id}`),
  create: (data: CreateClientDto) => apiClient.post<string>('/api/clients', data),
  update: (id: string, data: CreateClientDto) =>
    apiClient.put<Client>(`/api/clients/${id}`, data),
  delete: (id: string) => apiClient.delete(`/api/clients/${id}`),
  getPortfolio: (id: string) =>
    apiClient.get<ClientPortfolio>(`/api/clients/${id}/portfolio`),
};

// features/clients/hooks/useClients.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clientsApi } from '../api/clientsApi';

export const useClients = () => {
  return useQuery({
    queryKey: ['clients'],
    queryFn: clientsApi.getAll,
  });
};

export const useCreateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: clientsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};

// features/clients/components/ClientList.tsx
export const ClientList = () => {
  const { data: clients, isLoading } = useClients();
  const createClient = useCreateClient();

  // Component implementation...
};
```

## Database Schema

### Critical Table: ClientAssetAllocations

**This is the heart of the allocation system.**

```sql
CREATE TABLE ClientAssetAllocations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ClientId UUID NOT NULL REFERENCES Clients(Id) ON DELETE CASCADE,
    AssetType VARCHAR(20) NOT NULL, -- 'Wallet' or 'Account'
    AssetId UUID NOT NULL,
    AllocationType VARCHAR(20) NOT NULL, -- 'Percentage' or 'FixedAmount'
    AllocationValue DECIMAL(18, 8) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE, -- NULL means active
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_allocation_value CHECK (
        (AllocationType = 'Percentage' AND AllocationValue BETWEEN 0 AND 100) OR
        (AllocationType = 'FixedAmount' AND AllocationValue >= 0)
    ),

    UNIQUE NULLS NOT DISTINCT (ClientId, AssetType, AssetId, EndDate)
);
```

### Core Tables

1. **Clients**: Fund beneficiaries (~50 initially)
2. **CustodyWallets**: Blockchain wallets owned by the fund
3. **TraditionalAccounts**: Bank/investment accounts from Pluggy
4. **ClientAssetAllocations**: Maps client portions of custody assets (CRITICAL)
5. **WalletBalances**: Current token balances from Moralis
6. **AccountBalances**: Current account balances from Pluggy
7. **Transactions**: All transaction history (DeFi + traditional)
8. **TransactionAudit**: Audit trail for all transaction changes
9. **PriceHistory**: Historical price data for performance calculations
10. **PerformanceMetrics**: Calculated ROI, P&L per client
11. **RebalancingAlerts**: Automated alerts for allocation drift
12. **SystemConfiguration**: System settings (sync intervals, etc.)

See PLANNING.md for complete database schema.

## Development Commands

### 🚀 Running with Aspire (Recommended)

**Run entire stack (Backend + Frontend + Observability)**:
```bash
cd DeFiDashboard.AppHost
dotnet run

# This starts:
# - ApiService (https://localhost:7xxx)
# - Frontend (http://localhost:5173) with hot-reload
# - Aspire Dashboard (https://localhost:17243)
# - PostgreSQL (if using local container)
```

**Aspire Dashboard** provides:
- Real-time logs for all services
- Distributed tracing
- Metrics and performance monitoring
- Health checks
- Environment variable inspection
- Service dependencies visualization

### Backend (.NET 9)

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run migrations
dotnet ef database update --project src/ApiService

# Run API only (without Aspire)
dotnet run --project src/ApiService

# Run tests
dotnet test

# Create new migration
dotnet ef migrations add MigrationName --project src/ApiService

# Watch mode (auto-reload, API only)
dotnet watch --project src/ApiService
```

### Frontend (React + Vite)

```bash
cd frontend

# Install dependencies
npm install

# Development server (standalone, without Aspire)
npm run dev

# Production build
npm run build

# Preview production build
npm run preview

# Run tests
npm run test

# Lint code
npm run lint

# Type checking
npm run type-check
```

**Note**: When running with Aspire, the frontend automatically connects to the API via environment variables. When running standalone, ensure `VITE_API_BASE_URL` is set in `.env`.

## Adding a New Feature (Vertical Slice)

### Backend

1. Create feature folder: `Features/[FeatureName]/[Action]/`
2. Add Command/Query: `[Action][FeatureName]Command.cs`
3. Add Validator: `[Action][FeatureName]Validator.cs`
4. Add Handler: `[Action][FeatureName]Handler.cs`
5. Add Endpoint: `[Action][FeatureName]Endpoint.cs`
6. Register endpoint in `Program.cs` (Carter auto-discovers)

**Example: Add "Archive Client" feature**

```bash
mkdir -p Features/Clients/Archive
touch Features/Clients/Archive/ArchiveClientCommand.cs
touch Features/Clients/Archive/ArchiveClientValidator.cs
touch Features/Clients/Archive/ArchiveClientHandler.cs
touch Features/Clients/Archive/ArchiveClientEndpoint.cs
```

### Frontend

1. Create feature folder if needed: `features/[feature-name]/`
2. Add API function: `features/[feature-name]/api/`
3. Add hook: `features/[feature-name]/hooks/`
4. Add component: `features/[feature-name]/components/`
5. Add route: `features/[feature-name]/routes/`

## Provider Abstraction

All external providers MUST be abstracted:

```csharp
// Common/Providers/IBlockchainDataProvider.cs
public interface IBlockchainDataProvider
{
    Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(
        string walletAddress, CancellationToken ct);
    Task<IEnumerable<TokenTransfer>> GetWalletTransactionsAsync(
        string walletAddress, DateTime? fromDate, DateTime? toDate, CancellationToken ct);
    Task<decimal?> GetTokenPriceAsync(string tokenAddress, string chain, CancellationToken ct);
    string ProviderName { get; }
}

// Common/Providers/IOpenFinanceProvider.cs
public interface IOpenFinanceProvider
{
    Task<string> CreateConnectTokenAsync(CancellationToken ct);
    Task<IEnumerable<AccountSummary>> GetAccountsAsync(string itemId, CancellationToken ct);
    Task<IEnumerable<AccountBalance>> GetAccountBalanceAsync(string accountId, CancellationToken ct);
    Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(
        string accountId, DateTime from, DateTime to, CancellationToken ct);
    string ProviderName { get; }
}
```

## Background Jobs (Hangfire)

Jobs are defined in `BackgroundJobs/`:

```csharp
// BackgroundJobs/MoralisSyncJob.cs
public class MoralisSyncJob
{
    private readonly ApplicationDbContext _context;
    private readonly IBlockchainDataProvider _provider;

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync()
    {
        // 1. Get all active wallets
        // 2. Sync balances from Moralis
        // 3. Update WalletBalances table
        // 4. Record price history
    }
}

// Program.cs registration
RecurringJob.AddOrUpdate<MoralisSyncJob>(
    "moralis-sync",
    job => job.ExecuteAsync(),
    "*/5 * * * *"); // Every 5 minutes
```

## Testing Strategy

### Backend Tests

```csharp
// tests/ApiService.Tests/Features/Clients/Create/CreateClientHandlerTests.cs
public class CreateClientHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesClient()
    {
        // Arrange
        var context = CreateDbContext();
        var handler = new CreateClientHandler(context);
        var command = new CreateClientCommand("John Doe", "john@example.com", null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var client = await context.Clients.FindAsync(result.Value);
        client.Should().NotBeNull();
        client.Name.Should().Be("John Doe");
    }
}
```

### Frontend Tests

```typescript
// features/clients/components/__tests__/ClientList.test.tsx
describe('ClientList', () => {
  it('displays list of clients', async () => {
    render(<ClientList />);

    await waitFor(() => {
      expect(screen.getByText('Client 1')).toBeInTheDocument();
    });
  });
});
```

## Common Patterns

### Result Pattern (Backend)

```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(Error error) => new() { IsSuccess = false, Error = error };
}
```

### API Error Handling (Frontend)

```typescript
// shared/lib/api-client.ts
export const apiClient = {
  async get<T>(url: string): Promise<T> {
    const response = await fetch(url);
    if (!response.ok) throw new ApiError(response);
    return response.json();
  },
  // ... other methods
};
```

## Environment Variables

```bash
# Backend (appsettings.json or environment)
SUPABASE_CONNECTION_STRING="postgresql://..."
MORALIS_API_KEY="..."
PLUGGY_CLIENT_ID="..."
PLUGGY_CLIENT_SECRET="..."
HANGFIRE_DASHBOARD_USER="admin"
HANGFIRE_DASHBOARD_PASSWORD="..."

# Frontend (.env)
VITE_API_BASE_URL="http://localhost:5000"
VITE_PLUGGY_CONNECT_URL="..."
```

## Key Principles

1. **Feature-First**: Organize by feature, not by technical layer
2. **CQRS**: Separate commands (write) from queries (read)
3. **Validation**: Use FluentValidation for all commands
4. **Provider Abstraction**: All external APIs behind interfaces
5. **Audit Trail**: Log all state changes
6. **Allocation-Based**: Clients have shares of custody assets
7. **Background Jobs**: Periodic sync via Hangfire
8. **Type Safety**: TypeScript strict mode, C# nullable reference types

## Troubleshooting

### MediatR Handler Not Found
- Ensure handler implements `IRequestHandler<TRequest, TResponse>`
- Check DI registration in `Program.cs`
- Verify assembly is scanned: `services.AddMediatR(typeof(Program).Assembly)`

### Carter Endpoint Not Discovered
- Ensure endpoint implements `ICarterModule`
- Check `app.MapCarter()` is called in `Program.cs`
- Verify endpoint class is public

### TanStack Query Not Updating
- Check `queryKey` is correctly defined
- Ensure `invalidateQueries` is called after mutations
- Verify `queryClient` is provided in React tree

---

**Last Updated**: 2025-10-12
**Version**: 2.0.0 (Vertical Slice Architecture + .NET 9)
**Architecture**: Vertical Slice Architecture with CQRS
