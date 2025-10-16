using Carter;
using MediatR;

namespace ApiService.Features.Wallets.Sync;

public class SyncWalletEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/wallets/{id:guid}/sync", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new SyncWalletCommand(id);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("SyncWallet")
        .WithTags("Wallets")
        .WithOpenApi()
        .Produces<SyncResultDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
