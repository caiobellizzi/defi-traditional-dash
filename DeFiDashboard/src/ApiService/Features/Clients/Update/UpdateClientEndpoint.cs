using System;
using Carter;
using MediatR;

namespace ApiService.Features.Clients.Update;

public class UpdateClientEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/clients/{id:guid}", async (
            Guid id,
            UpdateClientRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateClientCommand(
                id,
                request.Name,
                request.Email,
                request.Document,
                request.PhoneNumber,
                request.Notes,
                request.Status
            );

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
        .WithName("UpdateClient")
        .WithTags("Clients")
        .WithOpenApi()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}

public record UpdateClientRequest(
    string Name,
    string Email,
    string? Document,
    string? PhoneNumber,
    string? Notes,
    string Status
);
