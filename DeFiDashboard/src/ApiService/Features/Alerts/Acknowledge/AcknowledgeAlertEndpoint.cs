using Carter;
using MediatR;

namespace ApiService.Features.Alerts.Acknowledge;

public class AcknowledgeAlertEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/alerts/{id:guid}/acknowledge", async (
            Guid id,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new AcknowledgeAlertCommand(id);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { acknowledged = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("AcknowledgeAlert")
        .WithTags("Alerts")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
