using Carter;
using MediatR;

namespace ApiService.Features.Wallets.Update;

public class UpdateWalletEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/wallets/{id:guid}", async (
            Guid id,
            UpdateWalletRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateWalletCommand(
                id,
                request.Label,
                request.SupportedChains,
                request.Notes
            );

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateWallet")
        .WithTags("Wallets")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record UpdateWalletRequest(
    string? Label,
    string[]? SupportedChains,
    string? Notes
);
