using Carter;
using MediatR;

namespace ApiService.Features.Alerts.GetList;

public class GetAlertsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/alerts", async (
            ISender sender,
            string? severity,
            string? status,
            string? alertType,
            Guid? clientId,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetAlertsQuery(severity, status, alertType, clientId, pageNumber, pageSize);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAlerts")
        .WithTags("Alerts")
        .WithOpenApi()
        .Produces<PagedResult<AlertDto>>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
