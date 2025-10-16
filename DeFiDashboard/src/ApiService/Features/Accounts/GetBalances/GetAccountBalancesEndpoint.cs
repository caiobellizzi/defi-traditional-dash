using Carter;
using MediatR;

namespace ApiService.Features.Accounts.GetBalances;

public class GetAccountBalancesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}/balances", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAccountBalancesQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetAccountBalances")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<IEnumerable<AccountBalanceDto>>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
