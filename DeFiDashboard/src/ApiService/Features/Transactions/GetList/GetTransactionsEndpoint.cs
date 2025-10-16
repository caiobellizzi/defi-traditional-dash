using Carter;
using MediatR;

namespace ApiService.Features.Transactions.GetList;

public class GetTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions", async (
            ISender sender,
            int pageNumber = 1,
            int pageSize = 20,
            string? transactionType = null,
            Guid? assetId = null,
            string? direction = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? tokenSymbol = null,
            string? status = null,
            CancellationToken ct = default) =>
        {
            var query = new GetTransactionsQuery(
                pageNumber,
                pageSize,
                transactionType,
                assetId,
                direction,
                fromDate,
                toDate,
                tokenSymbol,
                status
            );

            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("GetTransactions")
        .WithTags("Transactions")
        .WithOpenApi()
        .Produces<PagedResult<TransactionDto>>(200);
    }
}
