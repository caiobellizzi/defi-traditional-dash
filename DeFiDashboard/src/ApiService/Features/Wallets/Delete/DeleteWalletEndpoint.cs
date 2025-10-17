using Carter;
using MediatR;

namespace ApiService.Features.Wallets.Delete;

public class DeleteWalletEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/wallets/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new DeleteWalletCommand(id);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteWallet")
        .WithTags("Wallets")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
