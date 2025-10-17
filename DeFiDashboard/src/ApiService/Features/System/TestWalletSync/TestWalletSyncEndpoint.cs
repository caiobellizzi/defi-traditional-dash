using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Features.System.TestWalletSync;

public class TestWalletSyncEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/system/test-wallet-sync", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new TestWalletSyncCommand();
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("TestWalletSync")
        .WithTags("System")
        .WithOpenApi()
        .WithDescription("Manually trigger wallet sync job for testing purposes");
    }
}
