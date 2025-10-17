using Carter;
using MediatR;

namespace ApiService.Features.Allocations.Update;

public class UpdateAllocationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/allocations/{id:guid}", async (
            Guid id,
            UpdateAllocationRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateAllocationCommand(
                id,
                request.AllocationType,
                request.AllocationValue,
                request.StartDate,
                request.Notes
            );

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateAllocation")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}

public record UpdateAllocationRequest(
    string AllocationType,
    decimal AllocationValue,
    DateTime StartDate,
    string? Notes
);
