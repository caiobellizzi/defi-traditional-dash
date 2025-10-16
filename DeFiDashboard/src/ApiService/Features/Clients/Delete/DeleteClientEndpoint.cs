using System;
using Carter;
using MediatR;

namespace ApiService.Features.Clients.Delete;

public class DeleteClientEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/clients/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new DeleteClientCommand(id);
            var result = await sender.Send(command, ct);

            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            if (!string.IsNullOrEmpty(result.Error) &&
                result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteClient")
        .WithTags("Clients")
        .WithOpenApi()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
