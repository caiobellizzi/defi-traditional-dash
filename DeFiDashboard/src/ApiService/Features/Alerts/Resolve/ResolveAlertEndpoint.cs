using Carter;
using MediatR;

namespace ApiService.Features.Alerts.Resolve;

public class ResolveAlertEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/alerts/{id:guid}/resolve", async (
            Guid id,
            ResolveAlertRequest request,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ResolveAlertCommand(id, request.Resolution);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { resolved = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ResolveAlert")
        .WithTags("Alerts")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record ResolveAlertRequest(string? Resolution);
