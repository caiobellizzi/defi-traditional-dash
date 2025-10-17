using Carter;
using MediatR;

namespace ApiService.Features.Analytics.GetHistoricalPerformance;

public class GetHistoricalPerformanceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analytics/historical-performance", async (
            ISender sender,
            Guid? clientId,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default) =>
        {
            var query = new GetHistoricalPerformanceQuery(clientId, fromDate, toDate);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetHistoricalPerformance")
        .WithTags("Analytics")
        .WithOpenApi()
        .Produces<HistoricalPerformanceDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
