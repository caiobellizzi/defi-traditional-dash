using Carter;
using MediatR;

namespace ApiService.Features.Wallets.GetHistory;

public class GetWalletHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wallets/{id:guid}/history", async (
            Guid id,
            ISender sender,
            DateTime? fromDate,
            DateTime? toDate,
            string? tokenSymbol,
            CancellationToken ct = default) =>
        {
            var query = new GetWalletHistoryQuery(id, fromDate, toDate, tokenSymbol);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetWalletHistory")
        .WithTags("Wallets")
        .WithOpenApi()
        .Produces<IEnumerable<BalanceHistoryDto>>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
