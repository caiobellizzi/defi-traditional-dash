using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dash");

            migrationBuilder.RenameTable(
                name: "wallet_balances",
                newName: "wallet_balances",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "transactions",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "TransactionAudits",
                newName: "TransactionAudits",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "traditional_accounts",
                newName: "traditional_accounts",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "system_configuration",
                newName: "system_configuration",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "RebalancingAlerts",
                newName: "RebalancingAlerts",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "PriceHistories",
                newName: "PriceHistories",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "PerformanceMetrics",
                newName: "PerformanceMetrics",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "custody_wallets",
                newName: "custody_wallets",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "clients",
                newName: "clients",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "client_asset_allocations",
                newName: "client_asset_allocations",
                newSchema: "dash");

            migrationBuilder.RenameTable(
                name: "account_balances",
                newName: "account_balances",
                newSchema: "dash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "wallet_balances",
                schema: "dash",
                newName: "wallet_balances");

            migrationBuilder.RenameTable(
                name: "transactions",
                schema: "dash",
                newName: "transactions");

            migrationBuilder.RenameTable(
                name: "TransactionAudits",
                schema: "dash",
                newName: "TransactionAudits");

            migrationBuilder.RenameTable(
                name: "traditional_accounts",
                schema: "dash",
                newName: "traditional_accounts");

            migrationBuilder.RenameTable(
                name: "system_configuration",
                schema: "dash",
                newName: "system_configuration");

            migrationBuilder.RenameTable(
                name: "RebalancingAlerts",
                schema: "dash",
                newName: "RebalancingAlerts");

            migrationBuilder.RenameTable(
                name: "PriceHistories",
                schema: "dash",
                newName: "PriceHistories");

            migrationBuilder.RenameTable(
                name: "PerformanceMetrics",
                schema: "dash",
                newName: "PerformanceMetrics");

            migrationBuilder.RenameTable(
                name: "custody_wallets",
                schema: "dash",
                newName: "custody_wallets");

            migrationBuilder.RenameTable(
                name: "clients",
                schema: "dash",
                newName: "clients");

            migrationBuilder.RenameTable(
                name: "client_asset_allocations",
                schema: "dash",
                newName: "client_asset_allocations");

            migrationBuilder.RenameTable(
                name: "account_balances",
                schema: "dash",
                newName: "account_balances");
        }
    }
}
