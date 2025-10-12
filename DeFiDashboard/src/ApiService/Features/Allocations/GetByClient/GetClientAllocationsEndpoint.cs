using Carter;
using MediatR;

namespace ApiService.Features.Allocations.GetByClient;

public class GetClientAllocationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/clients/{clientId:guid}/allocations", async (
            Guid clientId,
            ISender sender,
            bool activeOnly = true,
            CancellationToken ct = default) =>
        {
            var query = new GetClientAllocationsQuery(clientId, activeOnly);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetClientAllocations")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}
