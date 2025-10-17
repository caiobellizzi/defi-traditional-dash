# DeFi-Traditional Finance Dashboard - User Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Core Concepts](#core-concepts)
4. [User Interface](#user-interface)
5. [Managing Clients](#managing-clients)
6. [Managing Wallets](#managing-wallets)
7. [Managing Allocations](#managing-allocations)
8. [Viewing Portfolio](#viewing-portfolio)
9. [Transaction History](#transaction-history)
10. [Alerts & Notifications](#alerts--notifications)
11. [Exporting Reports](#exporting-reports)
12. [Best Practices](#best-practices)
13. [Troubleshooting](#troubleshooting)

---

## Introduction

The **DeFi-Traditional Finance Dashboard** is a comprehensive wealth management platform designed for fund advisors who manage custody assets across both DeFi (cryptocurrency) and traditional finance (bank accounts, investments) on behalf of multiple clients.

### Key Features

- **Custody Model**: Wallets and accounts belong to the fund/advisor, not individual clients
- **Allocation System**: Clients have allocated shares (percentage or fixed amount) of custody assets
- **Multi-Chain Support**: Monitor Ethereum, Polygon, BSC, Arbitrum, Optimism, Avalanche, and more
- **Real-Time Tracking**: Automatic synchronization of balances and transactions
- **Portfolio Analytics**: Calculate client portfolios based on allocations
- **Performance Metrics**: Track ROI, P&L, and historical performance
- **Alert System**: Automated notifications for rebalancing needs and significant events
- **Professional Reporting**: Export client portfolios and performance reports

---

## Getting Started

### Prerequisites

Before you begin, ensure you have:

1. **.NET 9 SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
2. **Node.js 18+** installed ([Download](https://nodejs.org/))
3. **PostgreSQL database** (or Supabase account)
4. **Moralis API key** for blockchain data ([Get API Key](https://moralis.io/))
5. **Pluggy API credentials** for traditional finance integration ([Get Credentials](https://pluggy.ai/))

### Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd defi-traditional-dash/DeFiDashboard
   ```

2. **Configure database connection**:

   Edit `src/ApiService/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "defi-db": "Host=localhost;Database=defi_dashboard;Username=postgres;Password=your_password"
     }
   }
   ```

3. **Configure API keys**:

   Add to `appsettings.json` or use environment variables:
   ```json
   {
     "Moralis": {
       "ApiKey": "your_moralis_api_key"
     },
     "Pluggy": {
       "ClientId": "your_pluggy_client_id",
       "ClientSecret": "your_pluggy_client_secret"
     }
   }
   ```

4. **Run database migrations**:
   ```bash
   cd src/ApiService
   dotnet ef database update
   ```

5. **Start the application with Aspire**:
   ```bash
   cd DeFiDashboard.AppHost
   dotnet run
   ```

6. **Access the dashboard**:
   - Frontend: http://localhost:5173
   - Backend API: https://localhost:7xxx (check Aspire dashboard)
   - Aspire Dashboard: https://localhost:17243

---

## Core Concepts

### 1. Custody Model

In this system:
- **Custody Wallets** and **Traditional Accounts** are owned by the fund/advisor
- Clients do NOT have direct access to wallets or accounts
- The fund manages all assets on behalf of clients

### 2. Allocations

Clients have **allocations** to custody assets, which determine their share of each asset:

- **Percentage Allocation**: Client owns X% of an asset (e.g., 30% of ETH Wallet #1)
- **Fixed Amount Allocation**: Client owns a fixed amount (e.g., 1000 USDC from USDC Wallet)

**Important**:
- For percentage allocations, total allocations across all clients for a single asset cannot exceed 100%
- Allocations have start dates and can be ended (end date)
- Only active allocations (no end date) are used for portfolio calculations

### 3. Portfolio Calculation

A client's portfolio value is calculated by:
1. Finding all active allocations for the client
2. For each allocation:
   - Get current asset balance
   - Apply allocation percentage/amount
   - Convert to USD value
3. Sum all allocated asset values

### 4. Synchronization

The system automatically syncs data from external providers:

- **Moralis** (every 5 minutes):
  - Wallet balances across all supported chains
  - Token transfers and transactions
  - Current token prices

- **Pluggy** (every 15 minutes):
  - Bank account balances
  - Investment account values
  - Traditional finance transactions

---

## User Interface

### Dashboard Overview

The main dashboard provides a high-level overview:

- **Total Portfolio Value**: Combined value of all client portfolios
- **Total Clients**: Number of active clients
- **Asset Breakdown**: Distribution between crypto and traditional assets
- **Top Assets**: Most valuable assets by USD value
- **Recent Alerts**: Latest system notifications
- **Portfolio Chart**: Historical portfolio value over time

### Navigation

- **Dashboard**: Overview and analytics
- **Clients**: Manage clients and view individual portfolios
- **Wallets**: Manage custody wallets and balances
- **Allocations**: View and manage client allocations
- **Transactions**: Transaction history and audit trail
- **Alerts**: System notifications and alerts
- **Reports**: Export functionality

---

## Managing Clients

### Adding a New Client

1. Navigate to **Clients** page
2. Click **Add Client** button
3. Fill in client details:
   - **Name** (required): Full name
   - **Email** (required): Must be unique
   - **Document** (optional): CPF, SSN, or tax ID (must be unique)
   - **Phone Number** (optional): Contact number
   - **Notes** (optional): Additional information
4. Click **Create Client**

### Viewing Client Portfolio

1. Navigate to **Clients** page
2. Click on a client name
3. View detailed portfolio:
   - Total value (USD)
   - All active allocations
   - Asset breakdown
   - Performance metrics
   - Recent transactions

### Editing Client Information

1. Navigate to **Clients** page
2. Click **Edit** button next to client name
3. Modify client details
4. Click **Update Client**

### Deactivating/Archiving a Client

1. Navigate to **Clients** page
2. Click **Edit** button next to client name
3. Change **Status** to "Inactive" or "Archived"
4. Click **Update Client**

**Note**: Clients with active allocations cannot be deleted. You must first end all allocations.

---

## Managing Wallets

### Adding a Custody Wallet

1. Navigate to **Wallets** page
2. Click **Add Wallet** button
3. Enter wallet details:
   - **Address** (required): Blockchain wallet address
   - **Chain** (required): Select blockchain (Ethereum, Polygon, etc.)
   - **Label** (required): Descriptive name
   - **Notes** (optional): Additional information
4. Click **Add Wallet**

The system will immediately begin monitoring this wallet and syncing balances.

### Viewing Wallet Balances

1. Navigate to **Wallets** page
2. Click on a wallet to view:
   - All token balances
   - USD values
   - Current prices
   - Allocation breakdown (which clients are allocated to this wallet)
   - Recent transactions

### Updating Wallet Information

1. Navigate to **Wallets** page
2. Click **Edit** button next to wallet
3. Modify label or notes (address cannot be changed)
4. Click **Update Wallet**

### Removing a Wallet

1. Navigate to **Wallets** page
2. Ensure no active allocations exist for this wallet
3. Click **Delete** button
4. Confirm deletion

**Warning**: Deleting a wallet removes all historical balance and transaction data.

---

## Managing Allocations

### Creating a New Allocation

1. Navigate to **Clients** page
2. Click on a client name
3. Click **Add Allocation** button
4. Select allocation details:
   - **Asset Type**: Wallet or Account
   - **Asset**: Select specific wallet/account
   - **Allocation Type**: Percentage or Fixed Amount
   - **Allocation Value**:
     - For Percentage: 0-100 (system validates total doesn't exceed 100%)
     - For Fixed Amount: Any positive number
   - **Start Date**: When allocation becomes active
   - **Notes** (optional): Reason for allocation
5. Click **Create Allocation**

### Example: Percentage Allocation

**Scenario**: Allocate 30% of Main ETH Wallet to Client John Doe

1. Client: John Doe
2. Asset Type: Wallet
3. Asset: Main ETH Wallet (0x742d35Cc...)
4. Allocation Type: Percentage
5. Allocation Value: 30
6. Start Date: 2025-10-16

**Result**: John Doe will own 30% of all tokens in this wallet. If the wallet holds 10 ETH, John owns 3 ETH.

### Example: Fixed Amount Allocation

**Scenario**: Allocate 5000 USDC from USDC Wallet to Client Jane Smith

1. Client: Jane Smith
2. Asset Type: Wallet
3. Asset: USDC Wallet
4. Allocation Type: Fixed Amount
5. Allocation Value: 5000
6. Start Date: 2025-10-16

**Result**: Jane Smith will own exactly 5000 USDC from this wallet, regardless of total wallet balance.

### Ending an Allocation

1. Navigate to client portfolio
2. Find the allocation to end
3. Click **End Allocation** button
4. Allocation will be marked with end date (today)
5. Allocation becomes inactive and no longer affects portfolio calculations

**Note**: Ended allocations are kept for historical records.

### Viewing All Allocations

Navigate to **Allocations** page to see:
- All active allocations across all clients
- Allocation breakdown by asset
- Validation warnings (e.g., total percentage > 100%)

---

## Viewing Portfolio

### Overall Portfolio Overview

Navigate to **Dashboard** to see:
- **Total Portfolio Value**: USD value of all client portfolios combined
- **Asset Distribution**: Pie chart showing crypto vs traditional
- **Top Holdings**: Most valuable tokens/assets
- **Performance Chart**: Historical value over time

### Client Portfolio

Navigate to **Clients** > Click on client name to see:

- **Total Value**: Client's total portfolio value in USD
- **Allocations**: All active allocations with current values
- **Asset Breakdown**: Distribution across different assets
- **Performance Metrics**:
  - Total Return (USD)
  - Total Return (%)
  - Period: All-time, YTD, 1 Year, etc.
- **Recent Transactions**: Latest transactions affecting this client
- **Portfolio History Chart**: Value over time

### Consolidated View

Navigate to **Portfolio** > **Consolidated View** to see:
- All assets grouped by type (Crypto, Traditional)
- Total value per asset
- Allocation breakdown per asset
- Unallocated amounts (assets not yet allocated to clients)

---

## Transaction History

### Viewing All Transactions

Navigate to **Transactions** page to see:
- Paginated list of all transactions
- Filter by:
  - Wallet
  - Client
  - Type (Transfer, Swap, Deposit, Withdrawal)
  - Status (Confirmed, Pending, Failed)
  - Date range
- Sort by date, amount, or value

### Transaction Details

Click on a transaction to view:
- **Transaction Hash**: Link to blockchain explorer
- **From/To Addresses**
- **Token/Asset**: Symbol and contract address
- **Amount**: Token amount
- **USD Value**: Value at time of transaction
- **Gas Fee**: Network fee paid
- **Status**: Confirmation status
- **Block Number**: Blockchain block
- **Timestamp**: When transaction occurred
- **Notes**: Manual notes (if added)

### Adding Manual Transactions

For off-chain or manual transactions:

1. Navigate to **Transactions** page
2. Click **Add Manual Transaction** button
3. Enter transaction details:
   - Wallet/Account
   - Type
   - Amount
   - Token/Asset
   - USD Value
   - Date/Time
   - Notes
4. Click **Create Transaction**

Manual transactions are tracked separately and marked as "Manual" type.

---

## Alerts & Notifications

### Alert Types

The system generates alerts for:

1. **Rebalancing Needed**
   - Triggered when client allocation drifts beyond threshold
   - Default threshold: 5% drift
   - Example: Client has 30% allocation but current balance shows 35.5%

2. **Large Transaction**
   - Triggered for transactions above configured threshold
   - Default threshold: $10,000 USD
   - Helps detect unusual activity

3. **Price Alert**
   - Triggered when asset price changes significantly
   - Configurable per asset
   - Example: ETH price drops more than 10% in 1 hour

4. **System Error**
   - Triggered on sync failures or system issues
   - Requires immediate attention

### Managing Alerts

Navigate to **Alerts** page to:

1. **View All Alerts**
   - Sorted by severity (Critical, Warning, Info)
   - Filter by type, status, or date

2. **Acknowledge Alert**
   - Marks alert as "Acknowledged"
   - Indicates you've seen the alert
   - Alert remains in list until resolved

3. **Resolve Alert**
   - Marks alert as "Resolved"
   - Indicates issue has been addressed
   - Alert is archived

### Alert Dashboard Widget

The dashboard shows:
- **Unread Alerts**: Alerts you haven't acknowledged
- **Critical Alerts**: Alerts requiring immediate attention
- **Alert Summary**: Count by type

---

## Exporting Reports

### Portfolio Report (PDF)

Export a professional client portfolio report:

1. Navigate to **Clients** page
2. Click on client name
3. Click **Export Portfolio** button
4. Configure report options:
   - Include transactions: Yes/No
   - Include performance: Yes/No
   - Date range: Select period
5. Click **Generate Report**
6. Wait for report generation (async job)
7. Download PDF when ready

**Report Contents**:
- Client information
- Portfolio summary
- Asset breakdown with current values
- Allocation details
- Performance metrics (if included)
- Transaction history (if included)

### Transaction Report (Excel)

Export transaction history to Excel:

1. Navigate to **Transactions** page
2. Apply filters (optional):
   - Client
   - Wallet
   - Date range
3. Click **Export to Excel** button
4. Wait for export generation
5. Download Excel file when ready

**Excel Contents**:
- Transaction hash
- Date/Time
- From/To addresses
- Token/Asset
- Amount
- USD Value
- Gas fees
- Status

### Performance Report (Excel)

Export performance metrics to Excel:

1. Navigate to **Portfolio** page
2. Click **Export Performance** button
3. Select options:
   - Clients to include (all or specific)
   - Date range
   - Metrics to include
4. Click **Generate Report**
5. Download Excel file when ready

**Excel Contents**:
- Client name
- Initial value
- Current value
- Total return (USD)
- Total return (%)
- ROI
- Best/worst performing assets

---

## Best Practices

### 1. Regular Reconciliation

- Review client portfolios weekly
- Check for allocation drift
- Verify wallet balances match blockchain data
- Reconcile traditional account balances with bank statements

### 2. Allocation Management

- **Never exceed 100%** total percentage allocations per asset
- Use **fixed amount allocations** for stablecoins (less drift)
- Use **percentage allocations** for volatile assets (maintains proportion)
- Document allocation changes in notes field

### 3. Client Communication

- Export portfolio reports monthly for clients
- Notify clients of significant transactions (> $10k)
- Explain allocation changes when rebalancing
- Set clear expectations about portfolio updates (sync frequency)

### 4. Security

- Use **read-only** wallet addresses (no private keys in system)
- Regularly review access logs
- Enable two-factor authentication (when available)
- Never share API keys or database credentials

### 5. Data Backup

- Regularly export data
- Keep offline backups of client allocations
- Document allocation history
- Maintain audit trail for compliance

### 6. Performance Monitoring

- Review Aspire dashboard for system health
- Monitor sync job success rates
- Check for failed transactions
- Review alert trends

---

## Troubleshooting

### Wallet balances not updating

**Possible Causes**:
- Moralis API key invalid or expired
- Sync job failing
- Blockchain network issues

**Solutions**:
1. Check Aspire dashboard logs for sync job errors
2. Verify Moralis API key in configuration
3. Manually trigger wallet sync from UI
4. Check Moralis dashboard for API usage limits

---

### Client portfolio shows $0 value

**Possible Causes**:
- No active allocations for client
- Wallet balances haven't synced yet
- Allocations have end dates (inactive)

**Solutions**:
1. Navigate to client detail page
2. Check "Allocations" section
3. Verify allocations have no end date
4. Check wallet balances are synced
5. Wait for next sync cycle (5 minutes)

---

### Allocation creation fails: "Exceeds 100%"

**Cause**: Total percentage allocations for the asset would exceed 100%.

**Solution**:
1. Navigate to **Allocations** page
2. Filter by the specific asset
3. Review existing allocations
4. Either:
   - Reduce existing allocation percentages
   - End inactive allocations
   - Use fixed amount allocation instead

---

### Cannot delete client

**Cause**: Client has active allocations.

**Solution**:
1. Navigate to client portfolio
2. End all active allocations
3. Wait a few seconds for database update
4. Try deleting client again

---

### Transaction shows "Pending" for hours

**Possible Causes**:
- Blockchain network congestion
- Low gas fee (if you control the transaction)
- Blockchain reorganization

**Solutions**:
1. Check blockchain explorer (Etherscan, etc.)
2. Verify transaction status on-chain
3. If failed on-chain, transaction will sync as "Failed" on next cycle
4. If successful on-chain but still shows Pending, manually refresh transaction status

---

### Export job fails or never completes

**Possible Causes**:
- Large data set (thousands of transactions)
- Server resource constraints
- Background job service not running

**Solutions**:
1. Check Aspire dashboard for Hangfire status
2. Review background job logs
3. Try smaller date range for export
4. Restart application if necessary

---

### Traditional account balances not updating

**Possible Causes**:
- Pluggy API credentials invalid
- Bank connection expired (requires re-authentication)
- Bank API downtime

**Solutions**:
1. Check Aspire dashboard logs for Pluggy sync errors
2. Verify Pluggy credentials in configuration
3. Re-authenticate with bank using Pluggy Connect widget
4. Check Pluggy dashboard for connection status

---

## Support & Resources

- **Documentation**:
  - [API Documentation](./API-DOCUMENTATION.md)
  - [Deployment Guide](./DEPLOYMENT.md)
  - [Architecture Guide](./CLAUDE.md)

- **Technical Support**:
  - Create an issue in GitHub repository
  - Email: support@defi-dashboard.com (placeholder)

- **Community**:
  - Discord: (placeholder)
  - Forum: (placeholder)

---

## Glossary

- **Custody Wallet**: Blockchain wallet owned by the fund/advisor
- **Allocation**: Client's share of a custody asset (percentage or fixed amount)
- **Active Allocation**: Allocation with no end date, used in portfolio calculations
- **Percentage Allocation**: Client owns X% of an asset
- **Fixed Amount Allocation**: Client owns a fixed amount of an asset
- **Rebalancing**: Adjusting allocations to match target percentages
- **Allocation Drift**: Difference between target allocation and actual allocation due to price changes
- **Sync Job**: Background process that updates balances and transactions from external providers
- **Traditional Account**: Bank account or investment account from traditional finance

---

**Last Updated**: 2025-10-16
**Version**: 1.0.0
