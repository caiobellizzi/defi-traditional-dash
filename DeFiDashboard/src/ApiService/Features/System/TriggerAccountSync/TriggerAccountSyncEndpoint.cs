using Carter;
using MediatR;

namespace ApiService.Features.System.TriggerAccountSync;

public class TriggerAccountSyncEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/system/sync/accounts", async (
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new TriggerAccountSyncCommand();
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Accepted($"/api/export/jobs/{result.Value!.JobId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("TriggerAccountSync")
        .WithTags("System")
        .WithOpenApi()
        .Produces<SyncJobDto>(StatusCodes.Status202Accepted)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
