using Carter;
using MediatR;

namespace ApiService.Features.Portfolio.GetOverview;

public class GetPortfolioOverviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/portfolio/overview", async (
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetPortfolioOverviewQuery();
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetPortfolioOverview")
        .WithTags("Portfolio")
        .WithOpenApi()
        .Produces<PortfolioOverviewDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
