using Carter;
using MediatR;

namespace ApiService.Features.Alerts.GetSummary;

public class GetAlertSummaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/alerts/summary", async (
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetAlertSummaryQuery();
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAlertSummary")
        .WithTags("Alerts")
        .WithOpenApi()
        .Produces<AlertSummaryDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
