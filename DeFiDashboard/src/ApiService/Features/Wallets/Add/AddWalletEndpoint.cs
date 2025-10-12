using Carter;
using MediatR;

namespace ApiService.Features.Wallets.Add;

public class AddWalletEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/wallets", async (
            AddWalletCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/wallets/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("AddWallet")
        .WithTags("Wallets")
        .WithOpenApi();
    }
}
