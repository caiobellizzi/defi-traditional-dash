using Carter;
using MediatR;

namespace ApiService.Features.Wallets.GetList;

public class GetWalletsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/wallets", async (
            ISender sender,
            string? status,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetWalletsQuery(status, pageNumber, pageSize);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetWallets")
        .WithTags("Wallets")
        .WithOpenApi();
    }
}
