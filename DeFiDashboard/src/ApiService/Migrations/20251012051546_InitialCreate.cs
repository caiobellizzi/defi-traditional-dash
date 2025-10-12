using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    document = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custody_wallets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    wallet_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    blockchain_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Moralis"),
                    supported_chains = table.Column<string[]>(type: "text[]", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custody_wallets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Chain = table.Column<string>(type: "text", nullable: true),
                    PriceUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_configuration",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configuration", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "traditional_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    pluggy_item_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pluggy_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    institution_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    open_finance_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pluggy"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sync_error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traditional_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    transaction_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    chain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    from_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    to_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    token_symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(36,18)", precision: 36, scale: 18, nullable: false),
                    amount_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fee = table.Column<decimal>(type: "numeric(36,18)", precision: 36, scale: 18, nullable: true),
                    fee_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_manual_entry = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Confirmed"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_asset_allocations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allocation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    allocation_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_asset_allocations", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_asset_allocations_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalValueUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    CryptoValueUsd = table.Column<decimal>(type: "numeric", nullable: true),
                    TraditionalValueUsd = table.Column<decimal>(type: "numeric", nullable: true),
                    Roi = table.Column<decimal>(type: "numeric", nullable: true),
                    ProfitLoss = table.Column<decimal>(type: "numeric", nullable: true),
                    MetricsData = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RebalancingAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlertType = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    AlertData = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RebalancingAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RebalancingAlerts_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    token_symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    token_decimals = table.Column<int>(type: "integer", nullable: true),
                    balance = table.Column<decimal>(type: "numeric(36,18)", precision: 36, scale: 18, nullable: false, defaultValue: 0m),
                    balance_usd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_balances", x => x.id);
                    table.ForeignKey(
                        name: "FK_wallet_balances_custody_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalTable: "custody_wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_balances", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_balances_traditional_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "traditional_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldData = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    NewData = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionAudits_transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "account_balances_account_id_balance_type_key",
                table: "account_balances",
                columns: new[] { "account_id", "balance_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_account_balances_account",
                table: "account_balances",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_account_balances_type",
                table: "account_balances",
                column: "balance_type");

            migrationBuilder.CreateIndex(
                name: "client_asset_allocations_client_id_asset_type_asset_id_end_d_key",
                table: "client_asset_allocations",
                columns: new[] { "client_id", "asset_type", "asset_id", "end_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_allocations_active",
                table: "client_asset_allocations",
                column: "client_id",
                filter: "end_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_allocations_asset",
                table: "client_asset_allocations",
                columns: new[] { "asset_type", "asset_id" });

            migrationBuilder.CreateIndex(
                name: "idx_allocations_date_range",
                table: "client_asset_allocations",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "clients_document_key",
                table: "clients",
                column: "document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_clients_created_at",
                table: "clients",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_clients_email",
                table: "clients",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_clients_status",
                table: "clients",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_wallets_address",
                table: "custody_wallets",
                column: "wallet_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_wallets_provider",
                table: "custody_wallets",
                column: "blockchain_provider");

            migrationBuilder.CreateIndex(
                name: "idx_wallets_status",
                table: "custody_wallets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_ClientId",
                table: "PerformanceMetrics",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RebalancingAlerts_ClientId",
                table: "RebalancingAlerts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "idx_accounts_pluggy_id",
                table: "traditional_accounts",
                column: "pluggy_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_accounts_status",
                table: "traditional_accounts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_accounts_type",
                table: "traditional_accounts",
                column: "account_type");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionAudits_TransactionId",
                table: "TransactionAudits",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_asset",
                table: "transactions",
                columns: new[] { "transaction_type", "asset_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "idx_transactions_date",
                table: "transactions",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_external_id",
                table: "transactions",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_hash",
                table: "transactions",
                column: "transaction_hash");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_status",
                table: "transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_balances_chain",
                table: "wallet_balances",
                column: "chain");

            migrationBuilder.CreateIndex(
                name: "idx_balances_symbol",
                table: "wallet_balances",
                column: "token_symbol");

            migrationBuilder.CreateIndex(
                name: "idx_balances_updated",
                table: "wallet_balances",
                column: "last_updated");

            migrationBuilder.CreateIndex(
                name: "idx_balances_wallet",
                table: "wallet_balances",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "wallet_balances_wallet_id_chain_coalesce_key",
                table: "wallet_balances",
                columns: new[] { "wallet_id", "chain", "token_address" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_balances");

            migrationBuilder.DropTable(
                name: "client_asset_allocations");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "RebalancingAlerts");

            migrationBuilder.DropTable(
                name: "system_configuration");

            migrationBuilder.DropTable(
                name: "TransactionAudits");

            migrationBuilder.DropTable(
                name: "wallet_balances");

            migrationBuilder.DropTable(
                name: "traditional_accounts");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "custody_wallets");
        }
    }
}
