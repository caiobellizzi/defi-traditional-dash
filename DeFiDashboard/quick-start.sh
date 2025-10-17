#!/bin/bash

echo "🚀 DeFi-Traditional Finance Dashboard - Quick Start"
echo ""

# Check prerequisites
echo "🔍 Checking prerequisites..."
command -v dotnet >/dev/null 2>&1 || { echo "❌ .NET SDK not found. Install from https://dotnet.microsoft.com"; exit 1; }
command -v node >/dev/null 2>&1 || { echo "❌ Node.js not found. Install from https://nodejs.org"; exit 1; }

# Check versions
DOTNET_VERSION=$(dotnet --version)
NODE_VERSION=$(node --version)
echo "✅ .NET SDK: $DOTNET_VERSION"
echo "✅ Node.js: $NODE_VERSION"
echo ""

# Copy config if needed
if [ ! -f "src/ApiService/appsettings.Development.json" ]; then
    echo "📝 Creating appsettings.Development.json from template..."
    cp src/ApiService/appsettings.Development.json.template src/ApiService/appsettings.Development.json
    echo ""
    echo "⚠️  IMPORTANT: Edit src/ApiService/appsettings.Development.json with your credentials:"
    echo "   - Moralis API Key (https://moralis.io)"
    echo "   - Pluggy Client ID & Secret (https://pluggy.ai)"
    echo "   - Database connection string (if not using defaults)"
    echo ""
    read -p "Press Enter to continue after updating credentials, or Ctrl+C to exit..."
fi

# Copy frontend env if needed
if [ ! -f "frontend/.env" ]; then
    echo "📝 Creating frontend/.env from template..."
    cp frontend/.env.example frontend/.env
    echo "✅ Frontend environment file created"
    echo ""
fi

# Install frontend dependencies
if [ ! -d "frontend/node_modules" ]; then
    echo "📦 Installing frontend dependencies..."
    cd frontend && npm install && cd ..
    echo "✅ Frontend dependencies installed"
    echo ""
else
    echo "✅ Frontend dependencies already installed"
    echo ""
fi

# Check EF Core tools
echo "🔧 Checking Entity Framework tools..."
if ! dotnet ef --version >/dev/null 2>&1; then
    echo "📦 Installing Entity Framework Core tools..."
    dotnet tool install --global dotnet-ef
    echo ""
fi

# Run migrations
echo "🗄️  Running database migrations..."
cd src/ApiService
dotnet ef database update
DB_EXIT_CODE=$?
cd ../..

if [ $DB_EXIT_CODE -ne 0 ]; then
    echo ""
    echo "❌ Database migration failed!"
    echo "   Please check:"
    echo "   - PostgreSQL is running"
    echo "   - Connection string in appsettings.Development.json is correct"
    echo "   - Database 'defi_dashboard' exists (or create it: createdb defi_dashboard)"
    echo ""
    exit 1
fi

echo ""
echo "✅ Setup complete!"
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "To start the application:"
echo ""
echo "  Option 1: Using Aspire (Recommended)"
echo "    cd DeFiDashboard.AppHost && dotnet run"
echo ""
echo "  Option 2: Separate terminals"
echo "    Terminal 1: cd src/ApiService && dotnet run"
echo "    Terminal 2: cd frontend && npm run dev"
echo ""
echo "Access points:"
echo "  - Frontend:        http://localhost:5173"
echo "  - API:             https://localhost:7185"
echo "  - Swagger:         https://localhost:7185/swagger"
echo "  - Hangfire:        https://localhost:7185/hangfire"
echo "  - Aspire Dashboard: https://localhost:17243"
echo ""
echo "For detailed setup instructions, see:"
echo "  ENVIRONMENT-SETUP.md"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
