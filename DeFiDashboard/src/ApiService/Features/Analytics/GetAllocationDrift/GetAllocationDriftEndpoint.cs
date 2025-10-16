using Carter;
using MediatR;

namespace ApiService.Features.Analytics.GetAllocationDrift;

public class GetAllocationDriftEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analytics/allocation-drift", async (
            ISender sender,
            decimal threshold = 5.0m,
            CancellationToken ct = default) =>
        {
            var query = new GetAllocationDriftQuery(threshold);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAllocationDrift")
        .WithTags("Analytics")
        .WithOpenApi()
        .Produces<AllocationDriftDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
