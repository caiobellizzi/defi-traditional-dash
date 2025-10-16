using Carter;
using MediatR;

namespace ApiService.Features.Accounts.GetList;

public class GetAccountsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts", async (
            ISender sender,
            string? status,
            string? institutionName,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetAccountsQuery(status, institutionName, pageNumber, pageSize);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAccounts")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
