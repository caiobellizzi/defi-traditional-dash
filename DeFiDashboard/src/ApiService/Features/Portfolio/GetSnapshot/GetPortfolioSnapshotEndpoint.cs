using Carter;
using MediatR;

namespace ApiService.Features.Portfolio.GetSnapshot;

public class GetPortfolioSnapshotEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/portfolio/snapshot", async (
            ISender sender,
            DateTime? date,
            CancellationToken ct = default) =>
        {
            var query = new GetPortfolioSnapshotQuery(date);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetPortfolioSnapshot")
        .WithTags("Portfolio")
        .WithOpenApi()
        .Produces<PortfolioSnapshotDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
