using Carter;
using MediatR;

namespace ApiService.Features.Portfolio.GetClientPortfolio;

public class GetClientPortfolioEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/clients/{clientId:guid}/portfolio", async (
            Guid clientId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetClientPortfolioQuery(clientId);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetClientPortfolio")
        .WithTags("Portfolio")
        .WithOpenApi();
    }
}
