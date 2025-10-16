using Carter;
using MediatR;

namespace ApiService.Features.Allocations.End;

public class EndAllocationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/allocations/{id:guid}/end", async (
            Guid id,
            EndAllocationRequest? request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new EndAllocationCommand(id, request?.EndDate);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("EndAllocation")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}

public record EndAllocationRequest(DateTime? EndDate);
