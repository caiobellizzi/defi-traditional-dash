using Carter;
using MediatR;

namespace ApiService.Features.Clients.Create;

public class CreateClientEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/clients", async (
            CreateClientCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/clients/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateClient")
        .WithTags("Clients")
        .WithOpenApi()
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
