using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes for client_asset_allocations (foreign keys for joins)
            migrationBuilder.CreateIndex(
                name: "idx_client_asset_allocations_client_id",
                schema: "dash",
                table: "client_asset_allocations",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "idx_client_asset_allocations_asset_id",
                schema: "dash",
                table: "client_asset_allocations",
                column: "asset_id");

            // Add index for wallet_balances (wallet_id for aggregations)
            migrationBuilder.CreateIndex(
                name: "idx_wallet_balances_wallet_id",
                schema: "dash",
                table: "wallet_balances",
                column: "wallet_id");

            // Add index for account_balances (account_id for aggregations)
            migrationBuilder.CreateIndex(
                name: "idx_account_balances_account_id",
                schema: "dash",
                table: "account_balances",
                column: "account_id");

            // Add composite index for transactions (common query patterns)
            migrationBuilder.CreateIndex(
                name: "idx_transactions_asset_date",
                schema: "dash",
                table: "transactions",
                columns: new[] { "asset_id", "transaction_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_client_asset_allocations_client_id",
                schema: "dash",
                table: "client_asset_allocations");

            migrationBuilder.DropIndex(
                name: "idx_client_asset_allocations_asset_id",
                schema: "dash",
                table: "client_asset_allocations");

            migrationBuilder.DropIndex(
                name: "idx_wallet_balances_wallet_id",
                schema: "dash",
                table: "wallet_balances");

            migrationBuilder.DropIndex(
                name: "idx_account_balances_account_id",
                schema: "dash",
                table: "account_balances");

            migrationBuilder.DropIndex(
                name: "idx_transactions_asset_date",
                schema: "dash",
                table: "transactions");
        }
    }
}
