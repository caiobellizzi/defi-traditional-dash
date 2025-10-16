using Carter;
using MediatR;

namespace ApiService.Features.Portfolio.GetComposition;

public class GetPortfolioCompositionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/portfolio/composition", async (
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetPortfolioCompositionQuery();
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetPortfolioComposition")
        .WithTags("Portfolio")
        .WithOpenApi()
        .Produces<PortfolioCompositionDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
