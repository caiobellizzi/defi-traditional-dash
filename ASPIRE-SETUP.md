# .NET Aspire Setup Guide

Quick reference for setting up and running the DeFi-Traditional Finance Dashboard with .NET Aspire orchestration.

---

## 🚀 Quick Start

### Prerequisites

```bash
# Verify installations
dotnet --version          # Should be 9.0 or higher
node --version            # Should be 18.0 or higher
npm --version             # Should be 10.0 or higher
```

### Install Aspire Workload

```bash
# Install .NET Aspire workload
dotnet workload install aspire

# Verify installation
dotnet workload list
```

---

## 📦 Project Setup

### Step 1: Create Aspire Solution

```bash
# Create new directory
mkdir DeFiDashboard
cd DeFiDashboard

# Create Aspire starter solution
dotnet new aspire -n DeFiDashboard

# This creates:
# ├── DeFiDashboard.sln
# ├── DeFiDashboard.AppHost/          # Orchestrator
# └── DeFiDashboard.ServiceDefaults/  # Shared config
```

### Step 2: Add API Service

```bash
# Create API project
dotnet new webapi -n ApiService -o src/ApiService

# Add to solution
dotnet sln add src/ApiService

# Add reference to ServiceDefaults
cd src/ApiService
dotnet add reference ../../DeFiDashboard.ServiceDefaults

# Install required packages
dotnet add package MediatR
dotnet add package Carter
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Mapster
dotnet add package Serilog.AspNetCore
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.PostgreSql

cd ../..
```

### Step 3: Add Frontend (Vite + React)

```bash
# Create Vite React TypeScript project
npm create vite@latest frontend -- --template react-ts

# Install dependencies
cd frontend
npm install

# Install required libraries
npm install @tanstack/react-query @tanstack/react-table react-router-dom axios
npm install react-hook-form zod @hookform/resolvers recharts
npm install -D tailwindcss postcss autoprefixer
npm install -D @playwright/test

# Initialize Tailwind
npx tailwindcss init -p

# Install shadcn/ui
npx shadcn-ui@latest init

cd ..
```

### Step 4: Configure AppHost

**Install Node.js hosting package**:
```bash
cd DeFiDashboard.AppHost
dotnet add package Aspire.Hosting.NodeJs
cd ..
```

**Edit `DeFiDashboard.AppHost/Program.cs`**:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL (can use local or Supabase)
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "defi_dashboard");

var database = postgres.AddDatabase("defi-db");

// Add API Service
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithReference(database)
    .WithEnvironment("ConnectionStrings__Supabase", database);

// Add Frontend with hot-reload
var frontend = builder.AddNpmApp("frontend", "../frontend")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

frontend.WithNpmCommand("dev");

builder.Build().Run();
```

### Step 5: Configure API Service

**Edit `src/ApiService/Program.cs`**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (observability, health checks)
builder.AddServiceDefaults();

// Add services
builder.Services.AddCarter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
app.MapDefaultEndpoints(); // Aspire health checks
app.UseSwagger();
app.UseSwaggerUI();
app.MapCarter();

app.Run();
```

### Step 6: Configure Frontend

**Edit `frontend/vite.config.ts`**:
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    host: true,        // Required for Aspire
    strictPort: true,
    watch: {
      usePolling: true, // Better compatibility
    },
  },
});
```

**Edit `frontend/package.json`**:
```json
{
  "scripts": {
    "dev": "vite --host",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

---

## 🏃 Running the Application

### Development Mode (Recommended)

```bash
# From solution root
cd DeFiDashboard.AppHost
dotnet run

# Aspire Dashboard opens automatically at: https://localhost:17243
# API runs at: https://localhost:7xxx
# Frontend runs at: http://localhost:5173
```

### What Happens When You Run

1. **Aspire Dashboard** launches in your browser
2. **ApiService** starts with observability enabled
3. **Frontend** starts with Vite dev server (hot-reload enabled)
4. **PostgreSQL** container starts (if using local PostgreSQL)
5. All services are connected and monitored

### Aspire Dashboard Features

Access at `https://localhost:17243`:

- **Resources**: View all running services
- **Console Logs**: Real-time logs from all services
- **Structured Logs**: Filterable, searchable logs
- **Traces**: Distributed tracing across services
- **Metrics**: Performance metrics and charts
- **Environment**: View environment variables
- **Health Checks**: Service health status

---

## 🔧 Common Tasks

### View Logs

**Aspire Dashboard** → **Console Logs** tab
- Filter by service (apiservice, frontend)
- Search logs
- Export logs

### Debug API

1. API Swagger UI: `https://localhost:7xxx/swagger`
2. View traces in Aspire Dashboard
3. Check logs in Console tab

### Debug Frontend

1. Frontend runs at: `http://localhost:5173`
2. Hot-reload enabled automatically
3. View console logs in Aspire Dashboard
4. Browser DevTools still work normally

### Add Environment Variable

**In AppHost Program.cs**:
```csharp
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithEnvironment("MY_CUSTOM_VAR", "value");

var frontend = builder.AddNpmApp("frontend", "../frontend")
    .WithEnvironment("VITE_CUSTOM_VAR", "value");
```

### Connect to Supabase

**In AppHost Program.cs**:
```csharp
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithEnvironment("ConnectionStrings__Supabase",
        "Host=db.xxx.supabase.co;Database=postgres;Username=postgres;Password=xxx");
```

### Run Individual Services

**API only**:
```bash
cd src/ApiService
dotnet run
```

**Frontend only**:
```bash
cd frontend
npm run dev
```

**Note**: Running individually loses Aspire observability benefits.

---

## 📊 Service Communication

### API to Frontend

**Automatic via Aspire**:
- Frontend gets `VITE_API_BASE_URL` automatically
- No manual configuration needed

### Frontend to API

```typescript
// src/shared/lib/api-client.ts
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL, // Set by Aspire
});
```

---

## 🐳 Deployment

### Build for Production

```bash
# Build API
dotnet publish src/ApiService -c Release -o ./publish/api

# Build Frontend
cd frontend
npm run build
# Output in frontend/dist/
```

### Docker Support

Aspire can generate Docker containers:

```bash
# Publish with container support
dotnet publish DeFiDashboard.AppHost -c Release

# Deploy to container registry
# (Follow Aspire deployment documentation)
```

---

## 🔍 Troubleshooting

### Aspire Dashboard Not Opening

```bash
# Check Aspire is installed
dotnet workload list | grep aspire

# Reinstall if needed
dotnet workload install aspire
```

### Frontend Not Starting

**Check package.json**:
```json
{
  "scripts": {
    "dev": "vite --host"  // Must include --host
  }
}
```

**Check vite.config.ts**:
```typescript
server: {
  host: true,  // Required
  port: 5173,
  strictPort: true,
}
```

### Can't Connect to API from Frontend

**Check AppHost**:
```csharp
.WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("http"))
```

**Check frontend code uses env variable**:
```typescript
baseURL: import.meta.env.VITE_API_BASE_URL
```

### PostgreSQL Connection Issues

**Using Supabase**:
```csharp
// Don't use AddPostgres, use direct connection string
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithEnvironment("ConnectionStrings__Supabase",
        "Host=db.xxx.supabase.co;...");
```

**Using Local PostgreSQL**:
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "defi_dashboard");
```

### Hot-Reload Not Working

**Frontend**:
- Ensure `vite.config.ts` has `host: true`
- Ensure `package.json` has `"dev": "vite --host"`
- Try stopping and restarting Aspire

**API**:
```bash
# Use watch mode instead
cd src/ApiService
dotnet watch
```

---

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Node.js Integration](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-nodejs)
- [Aspire Deployment](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/overview)

---

## ✅ Verification Checklist

After setup, verify everything works:

- [ ] Aspire Dashboard opens at `https://localhost:17243`
- [ ] API appears in Resources list (green/running)
- [ ] Frontend appears in Resources list (green/running)
- [ ] Can access API Swagger at `https://localhost:7xxx/swagger`
- [ ] Can access Frontend at `http://localhost:5173`
- [ ] Console logs show both services logging
- [ ] Frontend hot-reload works (edit a component and save)
- [ ] API endpoint returns data when called from frontend

---

**Last Updated**: 2025-10-12
**Aspire Version**: .NET 9.0
