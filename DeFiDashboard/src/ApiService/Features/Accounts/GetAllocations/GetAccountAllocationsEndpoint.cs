using Carter;
using MediatR;

namespace ApiService.Features.Accounts.GetAllocations;

public class GetAccountAllocationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}/allocations", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAccountAllocationsQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetAccountAllocations")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<IEnumerable<AllocationDto>>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
