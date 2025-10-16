#!/bin/bash

# DeFi Dashboard - Initialize User Secrets
# This script sets up .NET User Secrets for secure configuration management

set -e  # Exit on error

PROJECT_PATH="src/ApiService/ApiService.csproj"
SECRETS_ID="21fd5093-e126-4503-afeb-700d45bd6223"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo ""
echo "====================================================================="
echo -e "${BLUE}  DeFi Dashboard - User Secrets Initialization${NC}"
echo "====================================================================="
echo ""
echo "This script will configure .NET User Secrets for secure storage of:"
echo "  • Database connection strings"
echo "  • API keys (Moralis, Pluggy)"
echo "  • Other sensitive configuration"
echo ""
echo -e "Secrets will be stored in: ${YELLOW}~/.microsoft/usersecrets/${SECRETS_ID}/${NC}"
echo ""

# Check if project exists
if [ ! -f "$PROJECT_PATH" ]; then
    echo -e "${RED}✗ Error: Project file not found at $PROJECT_PATH${NC}"
    echo "  Please run this script from the DeFiDashboard root directory"
    exit 1
fi

echo -e "${GREEN}✓ Project file found${NC}"
echo ""

# Verify user secrets is initialized
if ! grep -q "UserSecretsId" "$PROJECT_PATH"; then
    echo -e "${YELLOW}⚠ UserSecretsId not found in project file. Initializing...${NC}"
    dotnet user-secrets init --project "$PROJECT_PATH"
fi

echo "---------------------------------------------------------------------"
echo "1. DATABASE CONFIGURATION"
echo "---------------------------------------------------------------------"
echo ""
echo "Enter your Supabase PostgreSQL connection details:"
echo ""

read -p "  Database Host (e.g., db.yourproject.supabase.co): " db_host
read -p "  Database Name [postgres]: " db_name
db_name=${db_name:-postgres}
read -p "  Database Username [postgres]: " db_user
db_user=${db_user:-postgres}
read -sp "  Database Password: " db_password
echo ""
read -p "  Use SSL [Require]: " ssl_mode
ssl_mode=${ssl_mode:-Require}

if [ ! -z "$db_host" ] && [ ! -z "$db_password" ]; then
    DB_CONNECTION="Host=${db_host};Database=${db_name};Username=${db_user};Password=${db_password};SSL Mode=${ssl_mode};Trust Server Certificate=true"

    dotnet user-secrets set "ConnectionStrings:Supabase" "$DB_CONNECTION" --project "$PROJECT_PATH" > /dev/null
    echo -e "${GREEN}✓ Database connection string configured${NC}"
else
    echo -e "${YELLOW}⚠ Skipped database configuration (missing required fields)${NC}"
fi

echo ""
echo "---------------------------------------------------------------------"
echo "2. MORALIS API CONFIGURATION (Blockchain Data)"
echo "---------------------------------------------------------------------"
echo ""
echo "Get your API key from: https://admin.moralis.io/"
echo ""

read -p "  Moralis API Key (or press Enter to skip): " moralis_key

if [ ! -z "$moralis_key" ]; then
    dotnet user-secrets set "ExternalProviders:Moralis:ApiKey" "$moralis_key" --project "$PROJECT_PATH" > /dev/null
    dotnet user-secrets set "ExternalProviders:Moralis:BaseUrl" "https://deep-index.moralis.io/api/v2.2" --project "$PROJECT_PATH" > /dev/null
    echo -e "${GREEN}✓ Moralis API key configured${NC}"
else
    echo -e "${YELLOW}⚠ Skipped Moralis configuration${NC}"
fi

echo ""
echo "---------------------------------------------------------------------"
echo "3. PLUGGY API CONFIGURATION (OpenFinance/Banking)"
echo "---------------------------------------------------------------------"
echo ""
echo "Get your credentials from: https://dashboard.pluggy.ai/"
echo ""

read -p "  Pluggy Client ID (or press Enter to skip): " pluggy_id

if [ ! -z "$pluggy_id" ]; then
    read -sp "  Pluggy Client Secret: " pluggy_secret
    echo ""

    dotnet user-secrets set "ExternalProviders:Pluggy:ClientId" "$pluggy_id" --project "$PROJECT_PATH" > /dev/null
    dotnet user-secrets set "ExternalProviders:Pluggy:ClientSecret" "$pluggy_secret" --project "$PROJECT_PATH" > /dev/null
    dotnet user-secrets set "ExternalProviders:Pluggy:BaseUrl" "https://api.pluggy.ai" --project "$PROJECT_PATH" > /dev/null
    echo -e "${GREEN}✓ Pluggy credentials configured${NC}"
else
    echo -e "${YELLOW}⚠ Skipped Pluggy configuration${NC}"
fi

echo ""
echo "====================================================================="
echo -e "${GREEN}  SETUP COMPLETE!${NC}"
echo "====================================================================="
echo ""
echo "Your secrets have been securely stored and will be used in Development."
echo ""
echo -e "${BLUE}Next Steps:${NC}"
echo ""
echo "  1. View configured secrets:"
echo "     ${YELLOW}dotnet user-secrets list --project $PROJECT_PATH${NC}"
echo ""
echo "  2. Test database connection:"
echo "     ${YELLOW}dotnet ef database update --project $PROJECT_PATH${NC}"
echo ""
echo "  3. Start the application:"
echo "     ${YELLOW}cd DeFiDashboard.AppHost && dotnet run${NC}"
echo ""
echo "  4. Test API endpoints:"
echo "     ${YELLOW}/tmp/api-test.sh${NC}"
echo ""
echo -e "${BLUE}Useful Commands:${NC}"
echo ""
echo "  • List all secrets:"
echo "    ${YELLOW}dotnet user-secrets list --project $PROJECT_PATH${NC}"
echo ""
echo "  • Update a specific secret:"
echo "    ${YELLOW}dotnet user-secrets set 'KEY' 'VALUE' --project $PROJECT_PATH${NC}"
echo ""
echo "  • Remove a secret:"
echo "    ${YELLOW}dotnet user-secrets remove 'KEY' --project $PROJECT_PATH${NC}"
echo ""
echo "  • Clear all secrets:"
echo "    ${YELLOW}dotnet user-secrets clear --project $PROJECT_PATH${NC}"
echo ""
echo "  • Secrets location:"
echo "    ${YELLOW}~/.microsoft/usersecrets/${SECRETS_ID}/secrets.json${NC}"
echo ""
echo "====================================================================="
echo ""

# Ask if user wants to view secrets
read -p "Would you like to view the configured secrets now? (y/N): " view_secrets

if [[ "$view_secrets" =~ ^[Yy]$ ]]; then
    echo ""
    echo "Configured secrets:"
    echo "-------------------------------------------------------------------"
    dotnet user-secrets list --project "$PROJECT_PATH"
    echo "-------------------------------------------------------------------"
    echo ""
fi

echo -e "${GREEN}Done!${NC} You can now run the application."
echo ""
