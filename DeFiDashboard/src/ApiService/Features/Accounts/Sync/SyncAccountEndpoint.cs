using Carter;
using MediatR;

namespace ApiService.Features.Accounts.Sync;

public class SyncAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts/{id:guid}/sync", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new SyncAccountCommand(id);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("SyncAccount")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<SyncResultDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
