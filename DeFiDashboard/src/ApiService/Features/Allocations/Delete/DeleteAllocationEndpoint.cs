using Carter;
using MediatR;

namespace ApiService.Features.Allocations.Delete;

public class DeleteAllocationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/allocations/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new DeleteAllocationCommand(id);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true, warning = "Hard delete performed. Consider using End endpoint for soft delete." })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteAllocation")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}
