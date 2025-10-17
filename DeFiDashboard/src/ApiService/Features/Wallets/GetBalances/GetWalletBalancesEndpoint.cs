using Carter;
using MediatR;

namespace ApiService.Features.Wallets.GetBalances;

public class GetWalletBalancesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wallets/{id:guid}/balances", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetWalletBalancesQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetWalletBalances")
        .WithTags("Wallets")
        .WithOpenApi()
        .Produces<IEnumerable<WalletBalanceDto>>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
