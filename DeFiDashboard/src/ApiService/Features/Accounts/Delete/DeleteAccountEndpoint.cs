using Carter;
using MediatR;

namespace ApiService.Features.Accounts.Delete;

public class DeleteAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/accounts/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new DeleteAccountCommand(id);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteAccount")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
