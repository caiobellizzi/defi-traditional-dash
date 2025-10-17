using Carter;
using MediatR;

namespace ApiService.Features.Accounts.GetTransactions;

public class GetAccountTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}/transactions", async (
            Guid id,
            ISender sender,
            DateTime? fromDate,
            DateTime? toDate,
            string? category,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetAccountTransactionsQuery(
                id,
                fromDate,
                toDate,
                category,
                pageNumber,
                pageSize
            );

            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetAccountTransactions")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
