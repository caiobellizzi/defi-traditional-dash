# Supabase Database Connection Strategy

## Overview

**Project:** DeFi Dashboard
**Database:** Supabase PostgreSQL
**Project URL:** https://xqndfgrhyekxttvsddob.supabase.co
**Schema:** `dash`

## Supabase Credentials Provided

```bash
SUPABASE_URL=https://xqndfgrhyekxttvsddob.supabase.co
SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhxbmRmZ3JoeWVreHR0dnNkZG9iIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTg5NDc3NjgsImV4cCI6MjA3NDUyMzc2OH0.c02407Jy9XBr8g7nahlqD1ehs5F-TLbMkJJJrXCWiyM
SUPABASE_SERVICE_ROLE_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhxbmRmZ3JoeWVreHR0dnNkZG9iIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1ODk0Nzc2OCwiZXhwIjoyMDc0NTIzNzY4fQ.hMAs4WUmf9GaHj0w77bGfNg6BhVIof_KG2PCA1liRbo
SCHEMA=dash
```

---

## 🎯 Recommended Approach: Direct PostgreSQL Connection (Option 1)

### Why This is Best

✅ **Native .NET Integration** - Entity Framework Core works perfectly
✅ **Best Performance** - Direct database access, no HTTP overhead
✅ **Full LINQ Support** - Complex queries with compile-time safety
✅ **Transactions** - Full ACID transaction support
✅ **Existing Architecture** - No code changes needed
✅ **Connection Pooling** - Built-in with Npgsql

### What You Need

**Get Database Password from Supabase:**

1. Go to: https://supabase.com/dashboard/project/xqndfgrhyekxttvsddob/settings/database
2. Look for "Database Password" or "Reset Database Password"
3. Copy the `postgres` user password

### Connection String Formats

#### A. Direct Connection (Port 5432) - Recommended for Development

```bash
Host=db.xqndfgrhyekxttvsddob.supabase.co;
Port=5432;
Database=postgres;
Username=postgres;
Password=YOUR_POSTGRES_PASSWORD;
Search Path=dash;
SSL Mode=Require;
Trust Server Certificate=true;
Pooling=true;
Maximum Pool Size=20;
```

#### B. Connection Pooling via Pgbouncer (Port 6543) - Recommended for Production

```bash
Host=db.xqndfgrhyekxttvsddob.supabase.co;
Port=6543;
Database=postgres;
Username=postgres;
Password=YOUR_POSTGRES_PASSWORD;
Search Path=dash;
SSL Mode=Require;
Trust Server Certificate=true;
Pooling=true;
Maximum Pool Size=20;
```

**Pgbouncer Benefits:**
- Better connection management
- Reduced connection overhead
- Ideal for serverless/lambda functions
- Transaction mode (default) or session mode

### Configuration Steps

#### 1. Set User Secrets

```bash
# Using init-secrets.sh
./init-secrets.sh

# OR manually
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=db.xqndfgrhyekxttvsddob.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;Search Path=dash;SSL Mode=Require;Trust Server Certificate=true" --project src/ApiService/ApiService.csproj
```

#### 2. Update DbContext Configuration (if needed)

The current `ApplicationDbContext` should already work. Verify schema configuration:

```csharp
// src/ApiService/Common/Database/ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Set default schema
    modelBuilder.HasDefaultSchema("dash");

    // Apply configurations
    base.OnModelCreating(modelBuilder);
}
```

#### 3. Run Migrations

```bash
cd src/ApiService

# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate

# Apply to Supabase
dotnet ef database update
```

---

## 🔄 Alternative Approach: Supabase REST API (Option 2)

### When to Use This

- Need real-time subscriptions
- Want Row Level Security (RLS) enforcement
- Building client-side features
- Need Supabase Storage/Auth integration

### Not Recommended for Backend Because:

❌ No LINQ/EF Core support
❌ HTTP overhead for every query
❌ More complex transaction handling
❌ Limited query flexibility
❌ Requires significant code refactoring

### If You Still Want This Option

**Install Supabase Client:**

```bash
dotnet add package supabase-csharp
```

**Configuration:**

```csharp
// Program.cs
builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        "https://xqndfgrhyekxttvsddob.supabase.co",
        "SERVICE_ROLE_KEY",
        new SupabaseOptions { Schema = "dash" }
    )
);
```

---

## 🔐 Security Best Practices

### Key Management

| Key Type | Usage | Security Level | Where to Use |
|----------|-------|----------------|--------------|
| `ANON_KEY` | Client-side | Public | Frontend only, respects RLS |
| `SERVICE_ROLE_KEY` | Server-side | **HIGHLY SENSITIVE** | Backend only, bypasses RLS |
| `postgres` password | Direct DB | **HIGHLY SENSITIVE** | Backend only, full access |

### ⚠️ CRITICAL WARNINGS

1. **NEVER expose SERVICE_ROLE_KEY to client**
2. **NEVER commit credentials to git**
3. **ALWAYS use User Secrets for development**
4. **ALWAYS use environment variables for production**

### Recommended Storage

**Development:**
```bash
# .NET User Secrets (recommended)
dotnet user-secrets set "ConnectionStrings:Supabase" "..."
dotnet user-secrets set "Supabase:Url" "https://xqndfgrhyekxttvsddob.supabase.co"
dotnet user-secrets set "Supabase:ServiceRoleKey" "..."
```

**Production:**
```bash
# Environment Variables (Azure, AWS, Docker, etc.)
ConnectionStrings__Supabase="..."
Supabase__Url="..."
Supabase__ServiceRoleKey="..."
```

---

## 📋 Implementation Checklist

### Phase 1: Get Postgres Password

- [ ] Login to Supabase Dashboard
- [ ] Navigate to Settings > Database
- [ ] Copy or reset postgres password
- [ ] Store securely (password manager)

### Phase 2: Configure Connection

- [ ] Run `./init-secrets.sh`
- [ ] Enter Supabase connection details
- [ ] Test connection: `dotnet ef database update`

### Phase 3: Schema Setup

- [ ] Verify schema in DbContext (`dash`)
- [ ] Create/run migrations
- [ ] Verify tables created in Supabase

### Phase 4: Test Application

- [ ] Restart application
- [ ] Test API endpoints: `/tmp/api-test.sh`
- [ ] Verify data in Supabase Dashboard
- [ ] Check Aspire logs for any errors

---

## 🔧 Troubleshooting

### Connection Issues

**Error: "Connection refused"**
```bash
# Check firewall/network
ping db.xqndfgrhyekxttvsddob.supabase.co

# Try different port
# Direct: 5432
# Pgbouncer: 6543
```

**Error: "schema 'dash' does not exist"**
```sql
-- Create schema in Supabase SQL Editor
CREATE SCHEMA IF NOT EXISTS dash;
```

**Error: "SSL connection required"**
```bash
# Ensure connection string includes:
SSL Mode=Require;Trust Server Certificate=true;
```

### Performance Optimization

**Use Pgbouncer (Port 6543) for:**
- High connection churn
- Serverless deployments
- Multiple app instances

**Use Direct Connection (Port 5432) for:**
- Long-running transactions
- LISTEN/NOTIFY features
- Development/testing

---

## 🎯 Quick Start Commands

```bash
# 1. Initialize secrets with Supabase credentials
./init-secrets.sh

# 2. Apply migrations
dotnet ef database update --project src/ApiService

# 3. Restart application
pkill -f "dotnet run"; sleep 2
cd DeFiDashboard.AppHost && dotnet run &
cd frontend && npm run dev &

# 4. Test API
/tmp/api-test.sh
```

---

## 📚 Additional Resources

- [Supabase PostgreSQL Connection](https://supabase.com/docs/guides/database/connecting-to-postgres)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [EF Core with PostgreSQL](https://www.npgsql.org/efcore/)
- [Pgbouncer Guide](https://supabase.com/docs/guides/database/connecting-to-postgres#connection-pooler)

---

## 🚀 Recommended Configuration

**For your DeFi Dashboard, use:**

1. ✅ **Direct PostgreSQL Connection** (Port 5432 for dev, 6543 for prod)
2. ✅ **Entity Framework Core** (existing architecture)
3. ✅ **User Secrets** for development
4. ✅ **Schema: `dash`**
5. ✅ **Connection Pooling** enabled

This gives you the best performance, full .NET integration, and no code changes needed!

---

**Next Step:** Get your postgres password from Supabase and run `./init-secrets.sh`
