using Carter;
using MediatR;

namespace ApiService.Features.Wallets.GetTransactions;

public class GetWalletTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wallets/{id:guid}/transactions", async (
            Guid id,
            ISender sender,
            DateTime? fromDate,
            DateTime? toDate,
            string? direction,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetWalletTransactionsQuery(
                id,
                fromDate,
                toDate,
                direction,
                pageNumber,
                pageSize
            );

            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetWalletTransactions")
        .WithTags("Wallets")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
