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

public record SyncJobDto
{
    public Guid JobId { get; init; }
    public string JobType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
}
