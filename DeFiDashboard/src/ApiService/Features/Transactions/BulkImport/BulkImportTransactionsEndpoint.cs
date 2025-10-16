using Carter;

namespace ApiService.Features.Transactions.BulkImport;

public class BulkImportTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions/bulk-import", (
            IFormFile file,
            CancellationToken ct) =>
        {
            // TODO: Implement bulk import from CSV/Excel
            // This is a placeholder for future implementation

            return Results.StatusCode(501); // Not Implemented
        })
        .WithName("BulkImportTransactions")
        .WithTags("Transactions")
        .WithOpenApi()
        .DisableAntiforgery(); // For file upload
    }
}
