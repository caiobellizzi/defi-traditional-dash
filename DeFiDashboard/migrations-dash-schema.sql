-- Create dash schema and set it as default for this session
CREATE SCHEMA IF NOT EXISTS dash;
SET search_path TO dash;

-- Enable pgcrypto extension for gen_random_uuid() (built-in to PostgreSQL 13+)
-- Note: Supabase has this enabled by default, but we ensure it's available
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create migrations history table
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE clients (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        name character varying(200) NOT NULL,
        email character varying(200) NOT NULL,
        document character varying(50),
        phone_number character varying(20),
        status character varying(20) NOT NULL DEFAULT 'Active',
        notes text,
        created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by uuid,
        updated_by uuid,
        CONSTRAINT "PK_clients" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE custody_wallets (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        wallet_address character varying(100) NOT NULL,
        label character varying(200),
        blockchain_provider character varying(50) NOT NULL DEFAULT 'Moralis',
        supported_chains text[],
        status character varying(20) NOT NULL DEFAULT 'Active',
        notes text,
        created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_custody_wallets" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE "PriceHistories" (
        "Id" uuid NOT NULL,
        "Symbol" text NOT NULL,
        "Chain" text,
        "PriceUsd" numeric NOT NULL,
        "Timestamp" timestamp with time zone NOT NULL,
        "Source" text,
        CONSTRAINT "PK_PriceHistories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE system_configuration (
        key character varying(100) NOT NULL,
        value text NOT NULL,
        description text,
        updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_system_configuration" PRIMARY KEY (key)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE traditional_accounts (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        pluggy_item_id character varying(100),
        pluggy_account_id character varying(100),
        account_type character varying(50),
        institution_name character varying(200),
        account_number character varying(100),
        label character varying(200),
        open_finance_provider character varying(50) NOT NULL DEFAULT 'Pluggy',
        status character varying(20) NOT NULL DEFAULT 'Active',
        last_sync_at timestamp with time zone,
        sync_status character varying(50),
        sync_error_message text,
        created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_traditional_accounts" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE transactions (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        transaction_type character varying(20) NOT NULL,
        asset_id uuid NOT NULL,
        transaction_hash character varying(200),
        external_id character varying(200),
        chain character varying(50),
        direction character varying(10) NOT NULL,
        from_address character varying(200),
        to_address character varying(200),
        token_symbol character varying(20),
        amount numeric(36,18) NOT NULL,
        amount_usd numeric(18,2),
        fee numeric(36,18),
        fee_usd numeric(18,2),
        description text,
        category character varying(100),
        transaction_date timestamp with time zone NOT NULL,
        is_manual_entry boolean NOT NULL DEFAULT FALSE,
        status character varying(20) NOT NULL DEFAULT 'Confirmed',
        created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_transactions" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE client_asset_allocations (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        client_id uuid NOT NULL,
        asset_type character varying(20) NOT NULL,
        asset_id uuid NOT NULL,
        allocation_type character varying(20) NOT NULL,
        allocation_value numeric(18,8) NOT NULL,
        start_date timestamp with time zone NOT NULL,
        end_date timestamp with time zone,
        notes text,
        created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_client_asset_allocations" PRIMARY KEY (id),
        CONSTRAINT "FK_client_asset_allocations_clients_client_id" FOREIGN KEY (client_id) REFERENCES clients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE "PerformanceMetrics" (
        "Id" uuid NOT NULL,
        "ClientId" uuid NOT NULL,
        "CalculationDate" timestamp with time zone NOT NULL,
        "TotalValueUsd" numeric NOT NULL,
        "CryptoValueUsd" numeric,
        "TraditionalValueUsd" numeric,
        "Roi" numeric,
        "ProfitLoss" numeric,
        "MetricsData" jsonb,
        "CalculatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PerformanceMetrics" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PerformanceMetrics_clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES clients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE "RebalancingAlerts" (
        "Id" uuid NOT NULL,
        "ClientId" uuid,
        "AlertType" text NOT NULL,
        "Severity" text NOT NULL,
        "Message" text NOT NULL,
        "AlertData" jsonb,
        "Status" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "AcknowledgedAt" timestamp with time zone,
        "AcknowledgedBy" uuid,
        "ResolvedAt" timestamp with time zone,
        "ResolvedBy" uuid,
        CONSTRAINT "PK_RebalancingAlerts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RebalancingAlerts_clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES clients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE wallet_balances (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        wallet_id uuid NOT NULL,
        chain character varying(50) NOT NULL,
        token_address character varying(100),
        token_symbol character varying(20) NOT NULL,
        token_name character varying(100),
        token_decimals integer,
        balance numeric(36,18) NOT NULL DEFAULT 0.0,
        balance_usd numeric(18,2),
        last_updated timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_wallet_balances" PRIMARY KEY (id),
        CONSTRAINT "FK_wallet_balances_custody_wallets_wallet_id" FOREIGN KEY (wallet_id) REFERENCES custody_wallets (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE account_balances (
        id uuid NOT NULL DEFAULT gen_random_uuid(),
        account_id uuid NOT NULL,
        balance_type character varying(50) NOT NULL,
        currency character varying(3) NOT NULL DEFAULT 'BRL',
        amount numeric(18,2) NOT NULL DEFAULT 0.0,
        last_updated timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT "PK_account_balances" PRIMARY KEY (id),
        CONSTRAINT "FK_account_balances_traditional_accounts_account_id" FOREIGN KEY (account_id) REFERENCES traditional_accounts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE TABLE "TransactionAudits" (
        "Id" uuid NOT NULL,
        "TransactionId" uuid,
        "Action" text NOT NULL,
        "ChangedBy" uuid,
        "ChangedAt" timestamp with time zone NOT NULL,
        "OldData" jsonb,
        "NewData" jsonb,
        "Reason" text,
        CONSTRAINT "PK_TransactionAudits" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TransactionAudits_transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES transactions (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

-- Create indexes
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX account_balances_account_id_balance_type_key ON account_balances (account_id, balance_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_account_balances_account ON account_balances (account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_account_balances_type ON account_balances (balance_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX client_asset_allocations_client_id_asset_type_asset_id_end_d_key ON client_asset_allocations (client_id, asset_type, asset_id, end_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_allocations_active ON client_asset_allocations (client_id) WHERE end_date IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_allocations_asset ON client_asset_allocations (asset_type, asset_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_allocations_client ON client_asset_allocations (client_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_allocations_date_range ON client_asset_allocations (start_date, end_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX clients_document_key ON clients (document);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_clients_created_at ON clients (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_clients_email ON clients (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_clients_status ON clients (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_wallets_address ON custody_wallets (wallet_address);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_wallets_provider ON custody_wallets (blockchain_provider);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_wallets_status ON custody_wallets (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX "IX_PerformanceMetrics_ClientId" ON "PerformanceMetrics" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX "IX_RebalancingAlerts_ClientId" ON "RebalancingAlerts" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_accounts_pluggy_id ON traditional_accounts (pluggy_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_accounts_status ON traditional_accounts (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_accounts_type ON traditional_accounts (account_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX "IX_TransactionAudits_TransactionId" ON "TransactionAudits" ("TransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_transactions_asset ON transactions (transaction_type, asset_id, transaction_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_transactions_date ON transactions (transaction_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_transactions_external_id ON transactions (external_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_transactions_hash ON transactions (transaction_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_transactions_status ON transactions (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_balances_chain ON wallet_balances (chain);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_balances_symbol ON wallet_balances (token_symbol);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_balances_updated ON wallet_balances (last_updated);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE INDEX idx_balances_wallet ON wallet_balances (wallet_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    CREATE UNIQUE INDEX wallet_balances_wallet_id_chain_coalesce_key ON wallet_balances (wallet_id, chain, token_address);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012051546_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251012051546_InitialCreate', '9.0.9');
    END IF;
END $EF$;

COMMIT;

-- Summary: All tables created in 'dash' schema
SELECT 'Schema "dash" created successfully with 12 tables:' AS status;
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'dash'
ORDER BY table_name;
