using Carter;
using MediatR;

namespace ApiService.Features.Analytics.GetCorrelation;

public class GetCorrelationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analytics/correlation", async (
            ISender sender,
            int periodDays = 30,
            CancellationToken ct = default) =>
        {
            var query = new GetCorrelationQuery(periodDays);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetCorrelation")
        .WithTags("Analytics")
        .WithOpenApi()
        .Produces<CorrelationDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
