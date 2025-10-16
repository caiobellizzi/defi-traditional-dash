using Carter;
using MediatR;

namespace ApiService.Features.Portfolio.Recalculate;

public class RecalculatePortfolioEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/portfolio/recalculate", async (
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new RecalculatePortfolioCommand();
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Accepted($"/api/jobs/{result.Value!.JobId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RecalculatePortfolio")
        .WithTags("Portfolio")
        .WithOpenApi()
        .Produces<RecalculatePortfolioResultDto>(StatusCodes.Status202Accepted)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
