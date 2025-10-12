-- ============================================================================
-- DeFi-Traditional Finance Dashboard - Database Schema
-- ============================================================================
-- Database: PostgreSQL 15+ (Supabase)
-- Version: 1.0.0
-- Date: 2025-10-12
--
-- IMPORTANT: Use Supabase MCP to execute this schema
-- ============================================================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- CLIENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS clients (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200) UNIQUE NOT NULL,
    document VARCHAR(50) UNIQUE, -- CPF/CNPJ
    phone_number VARCHAR(20),
    status VARCHAR(20) DEFAULT 'Active' NOT NULL, -- Active, Inactive, Suspended
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_by UUID,
    updated_by UUID,

    CONSTRAINT chk_client_status CHECK (status IN ('Active', 'Inactive', 'Suspended'))
);

-- Indexes for clients
CREATE INDEX IF NOT EXISTS idx_clients_status ON clients(status);
CREATE INDEX IF NOT EXISTS idx_clients_email ON clients(email);
CREATE INDEX IF NOT EXISTS idx_clients_created_at ON clients(created_at DESC);

-- ============================================================================
-- CUSTODY WALLETS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS custody_wallets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    wallet_address VARCHAR(100) NOT NULL UNIQUE,
    label VARCHAR(200),
    blockchain_provider VARCHAR(50) DEFAULT 'Moralis' NOT NULL,
    supported_chains TEXT[], -- ['ethereum', 'polygon', 'bsc']
    status VARCHAR(20) DEFAULT 'Active' NOT NULL,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT chk_wallet_status CHECK (status IN ('Active', 'Inactive', 'Archived'))
);

-- Indexes for custody_wallets
CREATE INDEX IF NOT EXISTS idx_wallets_address ON custody_wallets(wallet_address);
CREATE INDEX IF NOT EXISTS idx_wallets_status ON custody_wallets(status);
CREATE INDEX IF NOT EXISTS idx_wallets_provider ON custody_wallets(blockchain_provider);

-- ============================================================================
-- TRADITIONAL ACCOUNTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS traditional_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    pluggy_item_id VARCHAR(100),
    pluggy_account_id VARCHAR(100) UNIQUE,
    account_type VARCHAR(50), -- BANK, INVESTMENT, CREDIT_CARD
    institution_name VARCHAR(200),
    account_number VARCHAR(100),
    label VARCHAR(200),
    open_finance_provider VARCHAR(50) DEFAULT 'Pluggy' NOT NULL,
    status VARCHAR(20) DEFAULT 'Active' NOT NULL,
    last_sync_at TIMESTAMP WITH TIME ZONE,
    sync_status VARCHAR(50), -- Success, Failed, Pending
    sync_error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT chk_account_status CHECK (status IN ('Active', 'Inactive', 'Archived')),
    CONSTRAINT chk_account_type CHECK (account_type IN ('BANK', 'INVESTMENT', 'CREDIT_CARD'))
);

-- Indexes for traditional_accounts
CREATE INDEX IF NOT EXISTS idx_accounts_pluggy_id ON traditional_accounts(pluggy_account_id);
CREATE INDEX IF NOT EXISTS idx_accounts_status ON traditional_accounts(status);
CREATE INDEX IF NOT EXISTS idx_accounts_type ON traditional_accounts(account_type);

-- ============================================================================
-- CLIENT ASSET ALLOCATIONS TABLE (CRITICAL)
-- ============================================================================
CREATE TABLE IF NOT EXISTS client_asset_allocations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES clients(id) ON DELETE CASCADE,
    asset_type VARCHAR(20) NOT NULL, -- 'Wallet' or 'Account'
    asset_id UUID NOT NULL, -- References custody_wallets.id or traditional_accounts.id
    allocation_type VARCHAR(20) NOT NULL, -- 'Percentage' or 'FixedAmount'
    allocation_value DECIMAL(18, 8) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE, -- NULL means active allocation
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    -- Constraints
    CONSTRAINT chk_asset_type CHECK (asset_type IN ('Wallet', 'Account')),
    CONSTRAINT chk_allocation_type CHECK (allocation_type IN ('Percentage', 'FixedAmount')),
    CONSTRAINT chk_allocation_value CHECK (
        (allocation_type = 'Percentage' AND allocation_value BETWEEN 0 AND 100) OR
        (allocation_type = 'FixedAmount' AND allocation_value >= 0)
    ),
    CONSTRAINT chk_start_before_end CHECK (end_date IS NULL OR end_date >= start_date),

    -- Unique constraint: no overlapping active allocations for same client-asset
    UNIQUE NULLS NOT DISTINCT (client_id, asset_type, asset_id, end_date)
);

-- Indexes for client_asset_allocations
CREATE INDEX IF NOT EXISTS idx_allocations_client ON client_asset_allocations(client_id);
CREATE INDEX IF NOT EXISTS idx_allocations_asset ON client_asset_allocations(asset_type, asset_id);
CREATE INDEX IF NOT EXISTS idx_allocations_active ON client_asset_allocations(client_id) WHERE end_date IS NULL;
CREATE INDEX IF NOT EXISTS idx_allocations_date_range ON client_asset_allocations(start_date, end_date);

-- ============================================================================
-- WALLET BALANCES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS wallet_balances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    wallet_id UUID NOT NULL REFERENCES custody_wallets(id) ON DELETE CASCADE,
    chain VARCHAR(50) NOT NULL, -- ethereum, polygon, bsc, etc.
    token_address VARCHAR(100), -- NULL for native tokens (ETH, BNB, etc.)
    token_symbol VARCHAR(20) NOT NULL,
    token_name VARCHAR(100),
    token_decimals INT,
    balance DECIMAL(36, 18) NOT NULL DEFAULT 0,
    balance_usd DECIMAL(18, 2),
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT chk_balance_positive CHECK (balance >= 0),
    UNIQUE (wallet_id, chain, COALESCE(token_address, ''))
);

-- Indexes for wallet_balances
CREATE INDEX IF NOT EXISTS idx_balances_wallet ON wallet_balances(wallet_id);
CREATE INDEX IF NOT EXISTS idx_balances_chain ON wallet_balances(chain);
CREATE INDEX IF NOT EXISTS idx_balances_symbol ON wallet_balances(token_symbol);
CREATE INDEX IF NOT EXISTS idx_balances_updated ON wallet_balances(last_updated DESC);

-- ============================================================================
-- ACCOUNT BALANCES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS account_balances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id UUID NOT NULL REFERENCES traditional_accounts(id) ON DELETE CASCADE,
    balance_type VARCHAR(50) NOT NULL, -- AVAILABLE, CURRENT, LIMIT
    currency VARCHAR(3) DEFAULT 'BRL' NOT NULL,
    amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT chk_balance_type CHECK (balance_type IN ('AVAILABLE', 'CURRENT', 'LIMIT')),
    UNIQUE (account_id, balance_type)
);

-- Indexes for account_balances
CREATE INDEX IF NOT EXISTS idx_account_balances_account ON account_balances(account_id);
CREATE INDEX IF NOT EXISTS idx_account_balances_type ON account_balances(balance_type);

-- ============================================================================
-- TRANSACTIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_type VARCHAR(20) NOT NULL, -- 'Wallet' or 'Account'
    asset_id UUID NOT NULL, -- wallet_id or account_id
    transaction_hash VARCHAR(200), -- For blockchain transactions
    external_id VARCHAR(200), -- Pluggy transaction ID or manual reference
    chain VARCHAR(50), -- For wallet transactions
    direction VARCHAR(10) NOT NULL, -- 'IN', 'OUT', 'INTERNAL'
    from_address VARCHAR(200),
    to_address VARCHAR(200),
    token_symbol VARCHAR(20),
    amount DECIMAL(36, 18) NOT NULL,
    amount_usd DECIMAL(18, 2),
    fee DECIMAL(36, 18),
    fee_usd DECIMAL(18, 2),
    description TEXT,
    category VARCHAR(100), -- Pluggy category or custom
    transaction_date TIMESTAMP WITH TIME ZONE NOT NULL,
    is_manual_entry BOOLEAN DEFAULT FALSE NOT NULL,
    status VARCHAR(20) DEFAULT 'Confirmed' NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT chk_transaction_type CHECK (transaction_type IN ('Wallet', 'Account')),
    CONSTRAINT chk_transaction_direction CHECK (direction IN ('IN', 'OUT', 'INTERNAL')),
    CONSTRAINT chk_transaction_status CHECK (status IN ('Pending', 'Confirmed', 'Failed', 'Cancelled'))
);

-- Indexes for transactions
CREATE INDEX IF NOT EXISTS idx_transactions_asset ON transactions(transaction_type, asset_id, transaction_date DESC);
CREATE INDEX IF NOT EXISTS idx_transactions_date ON transactions(transaction_date DESC);
CREATE INDEX IF NOT EXISTS idx_transactions_hash ON transactions(transaction_hash);
CREATE INDEX IF NOT EXISTS idx_transactions_external_id ON transactions(external_id);
CREATE INDEX IF NOT EXISTS idx_transactions_status ON transactions(status);

-- ============================================================================
-- TRANSACTION AUDIT TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS transaction_audit (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_id UUID REFERENCES transactions(id) ON DELETE SET NULL,
    action VARCHAR(50) NOT NULL, -- CREATE, UPDATE, DELETE
    changed_by UUID, -- User ID who made the change
    changed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    old_data JSONB,
    new_data JSONB,
    reason TEXT,

    CONSTRAINT chk_audit_action CHECK (action IN ('CREATE', 'UPDATE', 'DELETE'))
);

-- Indexes for transaction_audit
CREATE INDEX IF NOT EXISTS idx_audit_transaction ON transaction_audit(transaction_id);
CREATE INDEX IF NOT EXISTS idx_audit_date ON transaction_audit(changed_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_changed_by ON transaction_audit(changed_by);

-- ============================================================================
-- PRICE HISTORY TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS price_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol VARCHAR(20) NOT NULL,
    chain VARCHAR(50),
    price_usd DECIMAL(18, 8) NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    source VARCHAR(50), -- Moralis, CoinGecko, etc.

    CONSTRAINT chk_price_positive CHECK (price_usd >= 0),
    UNIQUE (symbol, COALESCE(chain, ''), timestamp)
);

-- Indexes for price_history
CREATE INDEX IF NOT EXISTS idx_price_symbol_time ON price_history(symbol, chain, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_price_timestamp ON price_history(timestamp DESC);

-- ============================================================================
-- PERFORMANCE METRICS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS performance_metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES clients(id) ON DELETE CASCADE,
    calculation_date DATE NOT NULL,
    total_value_usd DECIMAL(18, 2) NOT NULL DEFAULT 0,
    crypto_value_usd DECIMAL(18, 2) DEFAULT 0,
    traditional_value_usd DECIMAL(18, 2) DEFAULT 0,
    roi DECIMAL(10, 4), -- Return on Investment percentage
    profit_loss DECIMAL(18, 2),
    metrics_data JSONB, -- Detailed breakdown
    calculated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

    UNIQUE (client_id, calculation_date)
);

-- Indexes for performance_metrics
CREATE INDEX IF NOT EXISTS idx_metrics_client_date ON performance_metrics(client_id, calculation_date DESC);
CREATE INDEX IF NOT EXISTS idx_metrics_calculation_date ON performance_metrics(calculation_date DESC);

-- ============================================================================
-- REBALANCING ALERTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS rebalancing_alerts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID REFERENCES clients(id) ON DELETE CASCADE,
    alert_type VARCHAR(50) NOT NULL, -- THRESHOLD_BREACH, ALLOCATION_DRIFT, etc.
    severity VARCHAR(20) NOT NULL, -- INFO, WARNING, CRITICAL
    message TEXT NOT NULL,
    alert_data JSONB, -- Additional alert details
    status VARCHAR(20) DEFAULT 'Active' NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    acknowledged_at TIMESTAMP WITH TIME ZONE,
    acknowledged_by UUID,
    resolved_at TIMESTAMP WITH TIME ZONE,
    resolved_by UUID,

    CONSTRAINT chk_alert_severity CHECK (severity IN ('INFO', 'WARNING', 'CRITICAL')),
    CONSTRAINT chk_alert_status CHECK (status IN ('Active', 'Acknowledged', 'Resolved'))
);

-- Indexes for rebalancing_alerts
CREATE INDEX IF NOT EXISTS idx_alerts_client ON rebalancing_alerts(client_id);
CREATE INDEX IF NOT EXISTS idx_alerts_status ON rebalancing_alerts(status);
CREATE INDEX IF NOT EXISTS idx_alerts_created ON rebalancing_alerts(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_alerts_severity ON rebalancing_alerts(severity);

-- ============================================================================
-- SYSTEM CONFIGURATION TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS system_configuration (
    key VARCHAR(100) PRIMARY KEY,
    value TEXT NOT NULL,
    description TEXT,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- ============================================================================
-- DEFAULT CONFIGURATION DATA
-- ============================================================================
INSERT INTO system_configuration (key, value, description) VALUES
    ('moralis_sync_interval', '300', 'Moralis wallet sync interval in seconds (default: 5 minutes)'),
    ('pluggy_sync_interval', '600', 'Pluggy account sync interval in seconds (default: 10 minutes)'),
    ('portfolio_calculation_interval', '300', 'Portfolio recalculation interval in seconds (default: 5 minutes)'),
    ('rebalancing_check_interval', '3600', 'Rebalancing alert check interval in seconds (default: 1 hour)'),
    ('default_currency', 'BRL', 'Default currency for reporting'),
    ('allocation_drift_threshold', '5.0', 'Percentage drift threshold for rebalancing alerts'),
    ('price_history_retention_days', '365', 'Days to retain price history data')
ON CONFLICT (key) DO NOTHING;

-- ============================================================================
-- HELPER FUNCTIONS
-- ============================================================================

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Triggers for automatic updated_at updates
CREATE TRIGGER update_clients_updated_at
    BEFORE UPDATE ON clients
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_custody_wallets_updated_at
    BEFORE UPDATE ON custody_wallets
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_traditional_accounts_updated_at
    BEFORE UPDATE ON traditional_accounts
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_client_asset_allocations_updated_at
    BEFORE UPDATE ON client_asset_allocations
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_transactions_updated_at
    BEFORE UPDATE ON transactions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_system_configuration_updated_at
    BEFORE UPDATE ON system_configuration
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- VIEWS FOR COMMON QUERIES
-- ============================================================================

-- View: Active client allocations with asset details
CREATE OR REPLACE VIEW v_active_allocations AS
SELECT
    caa.id,
    caa.client_id,
    c.name AS client_name,
    caa.asset_type,
    caa.asset_id,
    CASE
        WHEN caa.asset_type = 'Wallet' THEN cw.wallet_address
        WHEN caa.asset_type = 'Account' THEN ta.account_number
    END AS asset_identifier,
    caa.allocation_type,
    caa.allocation_value,
    caa.start_date,
    caa.end_date
FROM client_asset_allocations caa
JOIN clients c ON caa.client_id = c.id
LEFT JOIN custody_wallets cw ON caa.asset_type = 'Wallet' AND caa.asset_id = cw.id
LEFT JOIN traditional_accounts ta ON caa.asset_type = 'Account' AND caa.asset_id = ta.id
WHERE caa.end_date IS NULL
AND c.status = 'Active';

-- View: Latest wallet balances
CREATE OR REPLACE VIEW v_latest_wallet_balances AS
SELECT
    wb.wallet_id,
    cw.wallet_address,
    cw.label AS wallet_label,
    wb.chain,
    wb.token_symbol,
    wb.token_name,
    wb.balance,
    wb.balance_usd,
    wb.last_updated
FROM wallet_balances wb
JOIN custody_wallets cw ON wb.wallet_id = cw.id
WHERE cw.status = 'Active';

-- View: Latest account balances
CREATE OR REPLACE VIEW v_latest_account_balances AS
SELECT
    ab.account_id,
    ta.account_number,
    ta.institution_name,
    ta.label AS account_label,
    ab.balance_type,
    ab.currency,
    ab.amount,
    ab.last_updated
FROM account_balances ab
JOIN traditional_accounts ta ON ab.account_id = ta.id
WHERE ta.status = 'Active';

-- ============================================================================
-- COMMENTS
-- ============================================================================
COMMENT ON TABLE clients IS 'Fund beneficiaries (clients who have allocations)';
COMMENT ON TABLE custody_wallets IS 'Blockchain wallets owned by the fund/advisor';
COMMENT ON TABLE traditional_accounts IS 'Traditional finance accounts (bank, investment, credit cards) from Pluggy';
COMMENT ON TABLE client_asset_allocations IS 'Maps client portions of custody assets - CRITICAL TABLE';
COMMENT ON TABLE wallet_balances IS 'Current token balances for custody wallets (synced from Moralis)';
COMMENT ON TABLE account_balances IS 'Current balances for traditional accounts (synced from Pluggy)';
COMMENT ON TABLE transactions IS 'Transaction history for both DeFi and traditional assets';
COMMENT ON TABLE transaction_audit IS 'Audit trail for all transaction changes';
COMMENT ON TABLE price_history IS 'Historical price data for ROI/P&L calculations';
COMMENT ON TABLE performance_metrics IS 'Calculated performance metrics (ROI, P&L) per client';
COMMENT ON TABLE rebalancing_alerts IS 'Automated alerts for allocation drift and rebalancing needs';
COMMENT ON TABLE system_configuration IS 'System-wide configuration settings';

-- ============================================================================
-- SCHEMA VERSION
-- ============================================================================
CREATE TABLE IF NOT EXISTS schema_version (
    version VARCHAR(20) PRIMARY KEY,
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,
    description TEXT
);

INSERT INTO schema_version (version, description) VALUES
    ('1.0.0', 'Initial schema creation with all core tables')
ON CONFLICT (version) DO NOTHING;

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================
