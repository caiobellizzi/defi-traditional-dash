using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiService.Migrations
{
    /// <inhheritdoc />
    public partial class AddExportJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportJobs",
                schema: "dash",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExportType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Parameters = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_Status",
                schema: "dash",
                table: "ExportJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_CreatedAt",
                schema: "dash",
                table: "ExportJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_ExpiresAt",
                schema: "dash",
                table: "ExportJobs",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportJobs",
                schema: "dash");
        }
    }
}
