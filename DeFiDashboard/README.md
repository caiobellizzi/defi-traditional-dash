# DeFi-Traditional Finance Dashboard

A comprehensive wealth management system for fund advisors managing custody assets across both DeFi (cryptocurrency) and traditional finance (bank accounts, investments) on behalf of multiple clients.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.1-61DAFB)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## Features

### Core Capabilities

- **Multi-Client Portfolio Management**: Track portfolios for 50+ clients from a single platform
- **Custody Model**: Fund-owned wallets and accounts with client allocations
- **Multi-Chain Support**: Monitor Ethereum, Polygon, BSC, Arbitrum, Optimism, Avalanche
- **Traditional Finance Integration**: Bank accounts and investments via Pluggy
- **Real-Time Synchronization**: Automatic balance and transaction updates (5-15 minute intervals)
- **Live Updates**: SignalR WebSocket integration for instant portfolio updates
- **Flexible Allocation System**: Percentage-based or fixed-amount allocations
- **Portfolio Analytics**: ROI, P&L, performance metrics, and historical tracking
- **Smart Alert System**: Automated notifications with real-time delivery
- **Professional Reporting**: PDF and Excel exports with async job processing
- **Database Seeding**: Sample data automatically loaded for quick development

### Technical Highlights

- **Vertical Slice Architecture**: Feature-based organization for both backend and frontend
- **CQRS Pattern**: MediatR for clean command/query separation
- **Aspire Orchestration**: Built-in observability with logs, traces, and metrics
- **Type Safety**: C# nullable reference types + TypeScript strict mode
- **Provider Abstraction**: Moralis and Pluggy abstracted behind interfaces
- **Background Jobs**: Hangfire for periodic syncing and calculations
- **Security Hardened**: XSS protection, rate limiting, input validation
- **Performance Optimized**: AsNoTracking queries (30-40% faster), strategic database indexes
- **Modern UI**: React 19 + shadcn/ui + Tailwind CSS with dark mode
- **Real-Time Communication**: SignalR for instant updates and notifications

## Architecture

### Technology Stack

#### Backend
- **.NET 9** with **Aspire** for cloud-native orchestration
- **Entity Framework Core 9** with PostgreSQL
- **MediatR** for CQRS
- **FluentValidation** for request validation
- **Carter** for minimal API endpoints
- **Hangfire** for background jobs with PostgreSQL storage
- **SignalR** for real-time WebSocket communication
- **HtmlSanitizer** for XSS protection
- **QuestPDF** for PDF generation
- **ClosedXML** for Excel exports
- **Serilog** for structured logging

#### Frontend
- **React 19.1** with **TypeScript 5.x**
- **Vite 5+** for lightning-fast builds with HMR
- **TanStack Query v5** for server state management
- **React Router v7** for client-side routing
- **@microsoft/signalr** for real-time updates
- **shadcn/ui + Tailwind CSS** for beautiful, accessible UI
- **Recharts** for analytics visualizations
- **React Hook Form + Zod** for type-safe forms
- **Playwright** for E2E testing

#### Infrastructure
- **Supabase PostgreSQL** for database
- **Aspire Dashboard** for observability
- **Moralis SDK** for blockchain data
- **Pluggy SDK** for OpenFinance integration

### Project Structure

```
DeFiDashboard/
├── src/
│   ├── ApiService/                 # Backend API
│   │   ├── Features/              # Vertical slices (CQRS)
│   │   │   ├── Clients/
│   │   │   ├── Wallets/
│   │   │   ├── Allocations/
│   │   │   ├── Portfolio/
│   │   │   ├── Transactions/
│   │   │   └── Alerts/
│   │   ├── Common/                # Shared infrastructure
│   │   └── Program.cs
│   └── AppHost/                   # Aspire orchestration
├── frontend/                      # React SPA
│   ├── src/
│   │   ├── pages/
│   │   ├── components/
│   │   ├── hooks/
│   │   └── api/
│   └── package.json
├── tests/
│   ├── ApiService.Tests/          # Unit tests
│   └── ApiService.IntegrationTests/  # Integration tests
├── API-DOCUMENTATION.md           # Complete API reference
├── USER-GUIDE.md                  # End-user documentation
├── DEPLOYMENT.md                  # Deployment instructions
└── CLAUDE.md                      # Development guidelines
```

## Getting Started

### Prerequisites

- **.NET 9 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Node.js 18+** ([Download](https://nodejs.org/))
- **PostgreSQL** (or [Supabase](https://supabase.com) account)
- **Moralis API Key** ([Get API Key](https://moralis.io/))
- **Pluggy Credentials** ([Get Credentials](https://pluggy.ai/))

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd defi-traditional-dash/DeFiDashboard
   ```

2. **Configure database**

   Edit `src/ApiService/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "defi-db": "Host=localhost;Database=defi_dashboard;Username=postgres;Password=your_password"
     },
     "Moralis": {
       "ApiKey": "your_moralis_api_key"
     },
     "Pluggy": {
       "ClientId": "your_pluggy_client_id",
       "ClientSecret": "your_pluggy_client_secret"
     }
   }
   ```

3. **Run database migrations**
   ```bash
   cd src/ApiService
   dotnet ef database update
   ```

4. **Start with Aspire (Recommended - Auto setup!)**
   ```bash
   cd DeFiDashboard.AppHost
   dotnet run
   ```

   **This automatically**:
   - ✅ Starts PostgreSQL container
   - ✅ Runs all database migrations
   - ✅ Seeds sample data (5 clients, 2 wallets)
   - ✅ Starts API + Frontend + Hangfire
   - ✅ Opens Aspire Dashboard

5. **Access the application**
   - **Frontend**: http://localhost:5173
   - **API**: https://localhost:7185
   - **Swagger API Docs**: https://localhost:7185/swagger
   - **Hangfire Dashboard**: https://localhost:7185/hangfire (dev only)
   - **Aspire Dashboard**: https://localhost:17128 (logs, metrics, traces)

## Usage

### Managing Clients

1. Navigate to **Clients** page
2. Click **Add Client** to create a new client
3. Fill in client details (name, email, document, phone)
4. Click **Create Client**

### Adding Custody Wallets

1. Navigate to **Wallets** page
2. Click **Add Wallet**
3. Enter wallet address and select blockchain
4. System will automatically sync balances

### Creating Allocations

1. Navigate to client detail page
2. Click **Add Allocation**
3. Select wallet/account and allocation type:
   - **Percentage**: Client owns X% of asset (e.g., 30%)
   - **Fixed Amount**: Client owns fixed amount (e.g., 1000 USDC)
4. System validates total allocations don't exceed 100%

### Viewing Portfolio

- **Dashboard**: Overall portfolio overview with analytics
- **Client Detail**: Individual client portfolio with performance metrics
- **Consolidated View**: All assets with allocation breakdown

### Exporting Reports

- **Portfolio PDF**: Professional client portfolio report
- **Transaction Excel**: Transaction history export
- **Performance Excel**: Performance metrics across clients

## Development

### Running Tests

```bash
# Unit tests
cd tests/ApiService.Tests
dotnet test

# Integration tests
cd tests/ApiService.IntegrationTests
dotnet test

# Frontend E2E tests
cd frontend
npm run test:e2e
```

### Development Workflow

1. **Backend**: Create a new feature slice in `Features/` directory
   - Command/Query
   - Handler
   - Validator
   - Endpoint

2. **Frontend**: Add feature components in `src/pages/` or `src/components/`
   - Use TanStack Query hooks for data fetching
   - Follow TypeScript strict mode

3. **Testing**: Write unit tests for handlers and integration tests for endpoints

### Code Quality

```bash
# Backend
dotnet format
dotnet build /warnaserror

# Frontend
npm run lint
npm run type-check
```

## Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for complete deployment instructions.

### Quick Deploy Options

- **Azure**: App Service + Static Web Apps
- **AWS**: Elastic Beanstalk + S3 + CloudFront
- **Docker**: Docker Compose or Kubernetes

### Environment Variables

**Backend**:
```bash
ConnectionStrings__defi-db="<postgres-connection-string>"
Moralis__ApiKey="<moralis-api-key>"
Pluggy__ClientId="<pluggy-client-id>"
Pluggy__ClientSecret="<pluggy-client-secret>"
```

**Frontend**:
```bash
VITE_API_BASE_URL="https://your-api-domain.com/api"
```

## Documentation

- [**API Reference**](./docs/API-REFERENCE.md) - Complete REST API documentation (65+ endpoints)
- [**Architecture**](./docs/ARCHITECTURE.md) - System design, patterns, and database schema
- [**Developer Guide**](./docs/DEVELOPER-GUIDE.md) - Development setup, workflows, and best practices
- [**User Guide**](./docs/USER-GUIDE.md) - End-user feature walkthroughs and tutorials
- [**CLAUDE.md**](./CLAUDE.md) - AI-powered development guidelines

## Key Concepts

### Custody Model

- Wallets and accounts are **owned by the fund/advisor**
- Clients have **allocations** to custody assets
- No private key management required (read-only monitoring)

### Allocation System

- **Percentage Allocation**: Client owns X% of an asset
  - Example: 30% of Main ETH Wallet
  - Total percentage allocations per asset cannot exceed 100%
- **Fixed Amount Allocation**: Client owns a fixed amount
  - Example: 5000 USDC from USDC Wallet
  - Useful for stablecoins or specific amounts

### Portfolio Calculation

Client portfolio value = Sum of (Asset Balance × Allocation Percentage/Amount × USD Price)

Example:
- Main ETH Wallet: 10 ETH at $3,333/ETH = $33,333
- Client allocation: 30%
- Client value: $33,333 × 30% = $10,000

## Background Jobs

The system automatically syncs data:

- **Wallet Sync** (every 5 minutes):
  - Wallet balances from Moralis
  - Token transfers and transactions
  - Real-time price updates

- **Account Sync** (every 15 minutes):
  - Bank account balances via Pluggy
  - Investment values
  - Traditional finance transactions

- **Portfolio Calculation** (every hour):
  - Recalculate all client portfolios
  - Update performance metrics (ROI, P&L)
  - Historical tracking

- **Alert Generation** (every 30 minutes):
  - Detect allocation drift
  - Significant balance changes
  - Rebalancing recommendations
  - Real-time SignalR notifications

- **Export Processing** (on-demand):
  - Async PDF generation
  - Excel report creation
  - Background job queue

- **Export Cleanup** (daily at 3 AM UTC):
  - Remove expired exports
  - Free up storage space

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow Vertical Slice Architecture
- Write unit tests for handlers
- Use CQRS pattern (MediatR)
- Add FluentValidation for commands
- Document API endpoints
- Follow TypeScript strict mode

## Roadmap

### v1.1 (Q1 2026)
- [ ] Authentication & Authorization (Azure AD B2C)
- [ ] Role-based access control (Admin, Advisor, Read-Only)
- [ ] Webhook support for real-time notifications
- [ ] Mobile responsive improvements

### v1.2 (Q2 2026)
- [ ] Multi-currency support (EUR, GBP, etc.)
- [ ] Custom reporting templates
- [ ] Automated rebalancing suggestions
- [ ] Client portal (read-only access)

### v2.0 (Q3 2026)
- [ ] AI-powered portfolio optimization
- [ ] Risk assessment tools
- [ ] Tax reporting integration
- [ ] Multi-tenant support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Documentation**: See `/docs` directory
- **Issues**: Create an issue in the repository
- **Email**: support@defi-dashboard.com (placeholder)
- **Discord**: (placeholder)

## Acknowledgments

- **Moralis** for blockchain data infrastructure
- **Pluggy** for OpenFinance integration
- **.NET Aspire** for cloud-native orchestration
- **shadcn/ui** for beautiful UI components
- **Supabase** for managed PostgreSQL

---

Built with by fund advisors, for fund advisors.

**Version**: 1.0.0
**Last Updated**: 2025-10-16
