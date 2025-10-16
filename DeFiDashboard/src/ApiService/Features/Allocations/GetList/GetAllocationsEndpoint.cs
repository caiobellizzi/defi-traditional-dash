using Carter;
using MediatR;

namespace ApiService.Features.Allocations.GetList;

public class GetAllocationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/allocations", async (
            Guid? clientId,
            string? assetType,
            Guid? assetId,
            bool activeOnly,
            int pageNumber,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAllocationsQuery(
                clientId,
                assetType,
                assetId,
                activeOnly,
                pageNumber > 0 ? pageNumber : 1,
                pageSize > 0 ? pageSize : 50
            );

            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAllocations")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}
