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

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteClient")
        .WithTags("Clients")
        .WithOpenApi()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
