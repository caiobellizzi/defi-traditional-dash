using Carter;
using MediatR;

namespace ApiService.Features.Portfolio.GetConsolidated;

public class GetConsolidatedPortfolioEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/portfolio/consolidated", async (
            ISender sender,
            string? assetType,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetConsolidatedPortfolioQuery(assetType, pageNumber, pageSize);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetConsolidatedPortfolio")
        .WithTags("Portfolio")
        .WithOpenApi()
        .Produces<ConsolidatedPortfolioDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
