using Carter;
using MediatR;

namespace ApiService.Features.Allocations.GetConflicts;

public class GetAllocationConflictsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/allocations/conflicts", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAllocationConflictsQuery();
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAllocationConflicts")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}
