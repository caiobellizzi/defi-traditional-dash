using Carter;
using MediatR;

namespace ApiService.Features.Analytics.GetRiskMetrics;

public class GetRiskMetricsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analytics/risk-metrics", async (
            ISender sender,
            Guid? clientId,
            int periodDays = 30,
            CancellationToken ct = default) =>
        {
            var query = new GetRiskMetricsQuery(clientId, periodDays);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetRiskMetrics")
        .WithTags("Analytics")
        .WithOpenApi()
        .Produces<RiskMetricsDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
