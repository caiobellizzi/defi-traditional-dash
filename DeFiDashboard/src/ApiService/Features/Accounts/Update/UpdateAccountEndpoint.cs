using Carter;
using MediatR;

namespace ApiService.Features.Accounts.Update;

public class UpdateAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/accounts/{id:guid}", async (
            Guid id,
            UpdateAccountRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateAccountCommand(id, request.Label);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateAccount")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record UpdateAccountRequest(string? Label);
