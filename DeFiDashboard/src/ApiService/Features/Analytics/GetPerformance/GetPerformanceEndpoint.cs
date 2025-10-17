using Carter;
using MediatR;

namespace ApiService.Features.Analytics.GetPerformance;

public class GetPerformanceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analytics/performance", async (
            ISender sender,
            Guid? clientId,
            DateTime? fromDate,
            DateTime? toDate,
            string granularity = "daily",
            CancellationToken ct = default) =>
        {
            var query = new GetPerformanceQuery(clientId, fromDate, toDate, granularity);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetPerformance")
        .WithTags("Analytics")
        .WithOpenApi()
        .Produces<PerformanceDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
