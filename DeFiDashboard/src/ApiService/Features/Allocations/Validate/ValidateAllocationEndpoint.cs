using Carter;
using MediatR;

namespace ApiService.Features.Allocations.Validate;

public class ValidateAllocationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/allocations/validate", async (
            ValidateAllocationRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new ValidateAllocationCommand(
                request.ClientId,
                request.AssetType,
                request.AssetId,
                request.AllocationType,
                request.AllocationValue,
                request.StartDate,
                request.ExcludeAllocationId
            );

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ValidateAllocation")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}

public record ValidateAllocationRequest(
    Guid ClientId,
    string AssetType,
    Guid AssetId,
    string AllocationType,
    decimal AllocationValue,
    DateTime StartDate,
    Guid? ExcludeAllocationId = null
);
