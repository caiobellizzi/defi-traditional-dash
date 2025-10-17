# DeFi-Traditional Finance Dashboard - Deployment Guide

## Table of Contents

1. [Deployment Overview](#deployment-overview)
2. [Prerequisites](#prerequisites)
3. [Environment Configuration](#environment-configuration)
4. [Database Setup](#database-setup)
5. [Azure Deployment](#azure-deployment)
6. [AWS Deployment](#aws-deployment)
7. [Docker Deployment](#docker-deployment)
8. [Monitoring & Observability](#monitoring--observability)
9. [Security Considerations](#security-considerations)
10. [Scaling Considerations](#scaling-considerations)
11. [Backup & Disaster Recovery](#backup--disaster-recovery)
12. [Troubleshooting](#troubleshooting)

---

## Deployment Overview

The DeFi-Traditional Finance Dashboard consists of:

1. **Backend API** (.NET 9 with Aspire)
2. **Frontend** (React 18 + Vite)
3. **PostgreSQL Database** (Supabase recommended)
4. **Background Jobs** (Hangfire)
5. **Observability** (OpenTelemetry via Aspire)

### Architecture Diagram

```
┌─────────────────┐
│   Users/Clients │
└────────┬────────┘
         │ HTTPS
         │
┌────────▼────────┐
│  Azure Front    │
│  Door / CDN     │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
┌───▼───┐ ┌──▼───┐
│ React │ │ .NET │
│ SPA   │ │ API  │
└───────┘ └──┬───┘
              │
         ┌────┼────┐
         │    │    │
     ┌───▼┐ ┌─▼──┐ ┌─▼────────┐
     │ DB │ │Jobs│ │ External │
     │ PG │ │HF │ │ APIs     │
     └────┘ └────┘ │ Moralis  │
                   │ Pluggy   │
                   └──────────┘
```

---

## Prerequisites

### Required Services

1. **PostgreSQL Database**
   - Supabase (recommended) - managed PostgreSQL with extensions
   - Azure Database for PostgreSQL
   - AWS RDS PostgreSQL
   - Self-hosted PostgreSQL 14+

2. **Hosting Platform**
   - Azure (App Service + Static Web Apps)
   - AWS (Elastic Beanstalk + S3 + CloudFront)
   - Docker containers (any platform)

3. **External API Keys**
   - **Moralis API Key**: For blockchain data ([Get API Key](https://moralis.io/))
   - **Pluggy Credentials**: For OpenFinance integration ([Get Credentials](https://pluggy.ai/))

4. **Optional Services**
   - Redis (for distributed caching and Hangfire storage)
   - Azure Application Insights (for monitoring)
   - SignalR Service (for real-time updates)
   - Blob Storage (for export files)

### Development Tools

- .NET 9 SDK
- Node.js 18+
- Docker Desktop (for containerized deployment)
- Azure CLI or AWS CLI

---

## Environment Configuration

### Backend Environment Variables

Create `appsettings.Production.json` or use environment variables:

```json
{
  "ConnectionStrings": {
    "defi-db": "Host=your-db-host;Database=defi_dashboard;Username=postgres;Password=<secure-password>;SSL Mode=Require"
  },
  "Moralis": {
    "ApiKey": "<moralis-api-key>",
    "BaseUrl": "https://deep-index.moralis.io/api/v2.2"
  },
  "Pluggy": {
    "ClientId": "<pluggy-client-id>",
    "ClientSecret": "<pluggy-client-secret>",
    "BaseUrl": "https://api.pluggy.ai"
  },
  "Hangfire": {
    "DashboardUsername": "admin",
    "DashboardPassword": "<secure-password>",
    "UseRedis": true,
    "RedisConnectionString": "<redis-connection-string>"
  },
  "Aspire": {
    "DashboardEnabled": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["https://your-frontend-domain.com"]
  }
}
```

### Frontend Environment Variables

Create `.env.production`:

```bash
VITE_API_BASE_URL=https://your-api-domain.com/api
VITE_PLUGGY_CONNECT_URL=https://connect.pluggy.ai
```

---

## Database Setup

### Option 1: Supabase (Recommended)

1. **Create Supabase Project**
   - Go to [supabase.com](https://supabase.com)
   - Create new project
   - Note connection string

2. **Configure Extensions**
   ```sql
   CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
   CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For full-text search
   ```

3. **Run Migrations**
   ```bash
   cd src/ApiService
   dotnet ef database update --connection "<connection-string>"
   ```

4. **Configure Row-Level Security (Optional)**
   ```sql
   -- Enable RLS on sensitive tables
   ALTER TABLE clients ENABLE ROW LEVEL SECURITY;
   ALTER TABLE custody_wallets ENABLE ROW LEVEL SECURITY;

   -- Create policies based on your auth strategy
   CREATE POLICY "Enable read access for authenticated users" ON clients
     FOR SELECT
     USING (auth.role() = 'authenticated');
   ```

### Option 2: Azure Database for PostgreSQL

1. **Create Azure PostgreSQL Server**
   ```bash
   az postgres flexible-server create \
     --resource-group defi-dashboard-rg \
     --name defi-dashboard-db \
     --location eastus \
     --admin-user dbadmin \
     --admin-password <secure-password> \
     --sku-name Standard_B2s \
     --tier Burstable \
     --storage-size 32 \
     --version 14
   ```

2. **Configure Firewall**
   ```bash
   az postgres flexible-server firewall-rule create \
     --resource-group defi-dashboard-rg \
     --name defi-dashboard-db \
     --rule-name AllowAzureServices \
     --start-ip-address 0.0.0.0 \
     --end-ip-address 0.0.0.0
   ```

3. **Run Migrations**
   ```bash
   cd src/ApiService
   dotnet ef database update
   ```

### Option 3: AWS RDS PostgreSQL

1. **Create RDS Instance**
   ```bash
   aws rds create-db-instance \
     --db-instance-identifier defi-dashboard-db \
     --db-instance-class db.t3.micro \
     --engine postgres \
     --engine-version 14.7 \
     --master-username dbadmin \
     --master-user-password <secure-password> \
     --allocated-storage 20 \
     --vpc-security-group-ids sg-xxxxxxxx \
     --db-name defi_dashboard
   ```

2. **Configure Security Groups**
   - Allow inbound traffic on port 5432 from application subnets

3. **Run Migrations**
   ```bash
   cd src/ApiService
   dotnet ef database update
   ```

---

## Azure Deployment

### Architecture

- **Backend**: Azure App Service (Linux)
- **Frontend**: Azure Static Web Apps
- **Database**: Supabase or Azure Database for PostgreSQL
- **Caching**: Azure Cache for Redis
- **Monitoring**: Azure Application Insights
- **Storage**: Azure Blob Storage (for exports)

### Step 1: Create Resource Group

```bash
az group create \
  --name defi-dashboard-rg \
  --location eastus
```

### Step 2: Deploy Backend to Azure App Service

1. **Create App Service Plan**
   ```bash
   az appservice plan create \
     --name defi-dashboard-plan \
     --resource-group defi-dashboard-rg \
     --sku B1 \
     --is-linux
   ```

2. **Create Web App**
   ```bash
   az webapp create \
     --name defi-dashboard-api \
     --resource-group defi-dashboard-rg \
     --plan defi-dashboard-plan \
     --runtime "DOTNET|9.0"
   ```

3. **Configure Environment Variables**
   ```bash
   az webapp config appsettings set \
     --name defi-dashboard-api \
     --resource-group defi-dashboard-rg \
     --settings \
       ConnectionStrings__defi-db="<connection-string>" \
       Moralis__ApiKey="<moralis-api-key>" \
       Pluggy__ClientId="<pluggy-client-id>" \
       Pluggy__ClientSecret="<pluggy-client-secret>"
   ```

4. **Deploy Application**
   ```bash
   cd src/ApiService
   dotnet publish -c Release -o ./publish
   cd publish
   zip -r app.zip .
   az webapp deployment source config-zip \
     --name defi-dashboard-api \
     --resource-group defi-dashboard-rg \
     --src app.zip
   ```

5. **Enable Always On**
   ```bash
   az webapp config set \
     --name defi-dashboard-api \
     --resource-group defi-dashboard-rg \
     --always-on true
   ```

### Step 3: Deploy Frontend to Azure Static Web Apps

1. **Build Frontend**
   ```bash
   cd frontend
   npm install
   npm run build
   ```

2. **Create Static Web App**
   ```bash
   az staticwebapp create \
     --name defi-dashboard-frontend \
     --resource-group defi-dashboard-rg \
     --location eastus \
     --sku Standard
   ```

3. **Deploy to Static Web Apps**
   ```bash
   # Get deployment token
   DEPLOYMENT_TOKEN=$(az staticwebapp secrets list \
     --name defi-dashboard-frontend \
     --resource-group defi-dashboard-rg \
     --query "properties.apiKey" -o tsv)

   # Deploy using Azure Static Web Apps CLI
   npm install -g @azure/static-web-apps-cli
   swa deploy ./dist \
     --deployment-token $DEPLOYMENT_TOKEN \
     --env production
   ```

4. **Configure API Backend**
   - In Azure Portal, navigate to Static Web App
   - Go to "Configuration"
   - Add API backend URL: `https://defi-dashboard-api.azurewebsites.net/api`

### Step 4: Configure Redis Cache (Optional)

```bash
az redis create \
  --name defi-dashboard-cache \
  --resource-group defi-dashboard-rg \
  --location eastus \
  --sku Basic \
  --vm-size c0

# Get connection string
az redis list-keys \
  --name defi-dashboard-cache \
  --resource-group defi-dashboard-rg
```

Update App Service settings with Redis connection string.

### Step 5: Configure Application Insights

```bash
az monitor app-insights component create \
  --app defi-dashboard-insights \
  --location eastus \
  --resource-group defi-dashboard-rg

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app defi-dashboard-insights \
  --resource-group defi-dashboard-rg \
  --query instrumentationKey -o tsv)

# Add to App Service
az webapp config appsettings set \
  --name defi-dashboard-api \
  --resource-group defi-dashboard-rg \
  --settings \
    APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSTRUMENTATION_KEY"
```

---

## AWS Deployment

### Architecture

- **Backend**: AWS Elastic Beanstalk (or ECS)
- **Frontend**: S3 + CloudFront
- **Database**: AWS RDS PostgreSQL
- **Caching**: Amazon ElastiCache for Redis
- **Monitoring**: CloudWatch
- **Storage**: Amazon S3 (for exports)

### Step 1: Deploy Backend to Elastic Beanstalk

1. **Create Elastic Beanstalk Application**
   ```bash
   eb init -p "64bit Amazon Linux 2023 v3.0.0 running .NET 9" defi-dashboard
   ```

2. **Create Environment**
   ```bash
   eb create defi-dashboard-prod \
     --instance-type t3.small \
     --database.engine postgres \
     --database.version 14.7
   ```

3. **Configure Environment Variables**
   ```bash
   eb setenv \
     ConnectionStrings__defi-db="<connection-string>" \
     Moralis__ApiKey="<moralis-api-key>" \
     Pluggy__ClientId="<pluggy-client-id>" \
     Pluggy__ClientSecret="<pluggy-client-secret>"
   ```

4. **Deploy Application**
   ```bash
   cd src/ApiService
   dotnet publish -c Release -o ./publish
   cd publish
   zip -r ../deploy.zip .
   eb deploy
   ```

### Step 2: Deploy Frontend to S3 + CloudFront

1. **Build Frontend**
   ```bash
   cd frontend
   npm install
   npm run build
   ```

2. **Create S3 Bucket**
   ```bash
   aws s3 mb s3://defi-dashboard-frontend
   aws s3 website s3://defi-dashboard-frontend \
     --index-document index.html \
     --error-document index.html
   ```

3. **Upload Files**
   ```bash
   aws s3 sync ./dist s3://defi-dashboard-frontend \
     --delete \
     --cache-control max-age=31536000
   ```

4. **Create CloudFront Distribution**
   ```bash
   aws cloudfront create-distribution \
     --origin-domain-name defi-dashboard-frontend.s3.amazonaws.com \
     --default-root-object index.html
   ```

5. **Configure Custom Domain** (Optional)
   - Use Route 53 for DNS
   - Configure SSL certificate with ACM

---

## Docker Deployment

### Option 1: Docker Compose (Single Server)

1. **Create `docker-compose.yml`**
   ```yaml
   version: '3.8'

   services:
     postgres:
       image: postgres:16-alpine
       environment:
         POSTGRES_DB: defi_dashboard
         POSTGRES_USER: postgres
         POSTGRES_PASSWORD: ${DB_PASSWORD}
       volumes:
         - postgres_data:/var/lib/postgresql/data
       ports:
         - "5432:5432"
       restart: unless-stopped

     redis:
       image: redis:7-alpine
       ports:
         - "6379:6379"
       restart: unless-stopped

     api:
       build:
         context: ./src/ApiService
         dockerfile: Dockerfile
       environment:
         ConnectionStrings__defi-db: "Host=postgres;Database=defi_dashboard;Username=postgres;Password=${DB_PASSWORD}"
         Moralis__ApiKey: ${MORALIS_API_KEY}
         Pluggy__ClientId: ${PLUGGY_CLIENT_ID}
         Pluggy__ClientSecret: ${PLUGGY_CLIENT_SECRET}
         Hangfire__RedisConnectionString: "redis:6379"
       depends_on:
         - postgres
         - redis
       ports:
         - "5000:8080"
       restart: unless-stopped

     frontend:
       build:
         context: ./frontend
         dockerfile: Dockerfile
       environment:
         VITE_API_BASE_URL: http://api:8080/api
       ports:
         - "80:80"
       restart: unless-stopped

   volumes:
     postgres_data:
   ```

2. **Create Backend Dockerfile** (`src/ApiService/Dockerfile`)
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   WORKDIR /app

   COPY *.csproj ./
   RUN dotnet restore

   COPY . ./
   RUN dotnet publish -c Release -o out

   FROM mcr.microsoft.com/dotnet/aspnet:9.0
   WORKDIR /app
   COPY --from=build /app/out .

   EXPOSE 8080
   ENTRYPOINT ["dotnet", "ApiService.dll"]
   ```

3. **Create Frontend Dockerfile** (`frontend/Dockerfile`)
   ```dockerfile
   FROM node:18-alpine AS build
   WORKDIR /app

   COPY package*.json ./
   RUN npm ci

   COPY . ./
   RUN npm run build

   FROM nginx:alpine
   COPY --from=build /app/dist /usr/share/nginx/html
   COPY nginx.conf /etc/nginx/conf.d/default.conf

   EXPOSE 80
   CMD ["nginx", "-g", "daemon off;"]
   ```

4. **Deploy**
   ```bash
   # Create .env file with secrets
   cat > .env << EOF
   DB_PASSWORD=<secure-password>
   MORALIS_API_KEY=<moralis-api-key>
   PLUGGY_CLIENT_ID=<pluggy-client-id>
   PLUGGY_CLIENT_SECRET=<pluggy-client-secret>
   EOF

   # Start services
   docker-compose up -d

   # Run migrations
   docker-compose exec api dotnet ef database update
   ```

### Option 2: Kubernetes (Production)

1. **Create Kubernetes Manifests**
   - See `/k8s` directory for complete manifests

2. **Deploy to Kubernetes**
   ```bash
   kubectl apply -f k8s/namespace.yaml
   kubectl apply -f k8s/secrets.yaml
   kubectl apply -f k8s/postgres.yaml
   kubectl apply -f k8s/redis.yaml
   kubectl apply -f k8s/api.yaml
   kubectl apply -f k8s/frontend.yaml
   kubectl apply -f k8s/ingress.yaml
   ```

---

## Monitoring & Observability

### Aspire Dashboard (Development/Staging)

Aspire provides built-in observability:

```bash
# Enable in production (optional)
dotnet run --project DeFiDashboard.AppHost
```

Access dashboard at: https://localhost:17243

**Features**:
- Real-time logs
- Distributed tracing
- Metrics and performance
- Health checks
- Service dependencies

### Application Insights (Azure)

Configure Application Insights for production monitoring:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=<key>;..."
  }
}
```

**Monitored Metrics**:
- Request rate
- Response time
- Failure rate
- Exception rate
- Custom metrics (portfolio value, sync success rate)

### CloudWatch (AWS)

Configure CloudWatch for AWS deployments:

```bash
# Install CloudWatch agent
aws cloudwatch put-metric-data \
  --namespace DeFiDashboard \
  --metric-name PortfolioSyncSuccess \
  --value 1 \
  --timestamp $(date -u +"%Y-%m-%dT%H:%M:%SZ")
```

### Custom Health Checks

Backend exposes health check endpoints:

- `/health` - Basic health check
- `/health/ready` - Readiness probe (database connected)
- `/health/live` - Liveness probe (application running)

Configure Kubernetes probes:
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
```

---

## Security Considerations

### 1. Secrets Management

**Azure**: Use Azure Key Vault
```bash
az keyvault create \
  --name defi-dashboard-kv \
  --resource-group defi-dashboard-rg \
  --location eastus

# Store secrets
az keyvault secret set \
  --vault-name defi-dashboard-kv \
  --name moralis-api-key \
  --value "<api-key>"

# Reference in App Service
az webapp config appsettings set \
  --name defi-dashboard-api \
  --resource-group defi-dashboard-rg \
  --settings \
    Moralis__ApiKey="@Microsoft.KeyVault(SecretUri=https://defi-dashboard-kv.vault.azure.net/secrets/moralis-api-key/)"
```

**AWS**: Use AWS Secrets Manager
```bash
aws secretsmanager create-secret \
  --name defi-dashboard/moralis-api-key \
  --secret-string "<api-key>"
```

**Kubernetes**: Use Kubernetes Secrets
```bash
kubectl create secret generic api-secrets \
  --from-literal=moralis-api-key=<api-key> \
  --from-literal=pluggy-client-id=<client-id> \
  --from-literal=pluggy-client-secret=<client-secret>
```

### 2. Network Security

- **Enable HTTPS only**
- **Configure CORS** to allow only your frontend domain
- **Use VNet/VPC** for database access
- **Enable firewall rules** on database
- **Use private endpoints** (Azure Private Link / AWS PrivateLink)

### 3. Authentication & Authorization

**Current Status**: Not implemented

**Recommended Implementation**:
- Azure AD B2C or AWS Cognito for user authentication
- JWT tokens for API authorization
- Role-based access control (Admin, Advisor, Read-Only)

### 4. Data Encryption

- **Database**: Enable transparent data encryption (TDE)
- **In-transit**: Use TLS 1.2+ for all connections
- **At-rest**: Enable encryption for backup storage

### 5. API Rate Limiting

Configure rate limiting middleware:

```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});
```

---

## Scaling Considerations

### Horizontal Scaling

**Backend API**:
- Use multiple app service instances or container replicas
- Configure load balancer (Azure Load Balancer / AWS ALB)
- Ensure stateless API (no session state in memory)
- Use Redis for distributed caching

**Database**:
- Enable read replicas for read-heavy workloads
- Use connection pooling (configure in EF Core)
- Implement database caching (Redis)

**Background Jobs**:
- Use Redis for Hangfire storage (distributed)
- Scale Hangfire servers independently
- Configure job concurrency limits

### Vertical Scaling

**Database**:
- Increase instance size (CPU, RAM)
- Increase storage capacity
- Enable Performance Insights

**App Service**:
- Upgrade to higher tier (S1, P1V2, etc.)
- Increase worker count

### Caching Strategy

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
    options.InstanceName = "DeFiDashboard:";
});

// Cache portfolio calculations
services.AddMemoryCache();
services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 1024;
    options.CompactionPercentage = 0.20;
});
```

**Cache Keys**:
- `portfolio:{clientId}` - TTL: 5 minutes
- `wallet:balances:{walletId}` - TTL: 5 minutes
- `prices:{tokenAddress}` - TTL: 1 minute

---

## Backup & Disaster Recovery

### Database Backups

**Azure**:
```bash
# Automated backups (enabled by default)
az postgres flexible-server backup list \
  --resource-group defi-dashboard-rg \
  --name defi-dashboard-db

# Manual backup
az postgres flexible-server backup create \
  --resource-group defi-dashboard-rg \
  --name defi-dashboard-db \
  --backup-name manual-backup-$(date +%Y%m%d)
```

**AWS**:
```bash
# Automated backups (enabled by default)
aws rds describe-db-snapshots \
  --db-instance-identifier defi-dashboard-db

# Manual snapshot
aws rds create-db-snapshot \
  --db-instance-identifier defi-dashboard-db \
  --db-snapshot-identifier manual-backup-$(date +%Y%m%d)
```

**Supabase**:
- Automatic daily backups
- Point-in-time recovery (PITR) available on Pro plan

### Backup Schedule

- **Daily**: Full database backup (retained for 30 days)
- **Weekly**: Application configuration backup
- **Monthly**: Long-term archive (retained for 1 year)

### Disaster Recovery Plan

1. **Recovery Time Objective (RTO)**: 4 hours
2. **Recovery Point Objective (RPO)**: 1 hour

**Recovery Steps**:
1. Provision new infrastructure (via IaC templates)
2. Restore database from latest backup
3. Deploy application from CI/CD pipeline
4. Verify data integrity
5. Update DNS to point to new infrastructure
6. Monitor for issues

---

## Troubleshooting

### Application Won't Start

**Check**:
1. Database connection string is correct
2. Database is accessible from app server
3. Migrations have been applied
4. Environment variables are set correctly

**Logs**:
```bash
# Azure
az webapp log tail \
  --name defi-dashboard-api \
  --resource-group defi-dashboard-rg

# AWS
eb logs

# Docker
docker-compose logs -f api
```

### High CPU/Memory Usage

**Causes**:
- Database query inefficiency
- Memory leak in application
- Background job consuming resources

**Solutions**:
1. Enable Application Insights or CloudWatch
2. Review slow queries in database logs
3. Scale up instance size
4. Optimize queries with indexes

### Background Jobs Not Running

**Check**:
1. Hangfire dashboard: `/hangfire` endpoint
2. Redis connection (if using Redis storage)
3. Recurring job configuration

**Fix**:
```bash
# Restart application
az webapp restart --name defi-dashboard-api --resource-group defi-dashboard-rg

# Check Hangfire logs
docker-compose exec api dotnet ApiService.dll --view-hangfire-logs
```

### Database Connection Pool Exhausted

**Symptoms**: "Connection pool exhausted" errors

**Solution**:
```csharp
// Increase connection pool size
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.MaxBatchSize(100);
    npgsqlOptions.CommandTimeout(30);
});
```

### CORS Errors

**Check**:
1. CORS policy in backend includes frontend domain
2. Frontend is using correct API base URL

**Fix**:
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-frontend-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## Post-Deployment Checklist

- [ ] Database migrations applied successfully
- [ ] All environment variables configured
- [ ] External API keys validated
- [ ] Health check endpoints responding
- [ ] Background jobs running (check Hangfire dashboard)
- [ ] Frontend can connect to backend API
- [ ] CORS configured correctly
- [ ] HTTPS enforced
- [ ] Database backups enabled
- [ ] Monitoring and alerts configured
- [ ] Log aggregation working
- [ ] Performance metrics visible
- [ ] DNS records updated
- [ ] SSL certificates valid
- [ ] Rate limiting configured
- [ ] Security headers configured
- [ ] Load testing completed
- [ ] Disaster recovery plan documented
- [ ] Team notified of deployment

---

**Last Updated**: 2025-10-16
**Version**: 1.0.0
