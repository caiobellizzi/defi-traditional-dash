using Carter;
using MediatR;

namespace ApiService.Features.Allocations.Create;

public class CreateAllocationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/allocations", async (
            CreateAllocationCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/allocations/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateAllocation")
        .WithTags("Allocations")
        .WithOpenApi();
    }
}
