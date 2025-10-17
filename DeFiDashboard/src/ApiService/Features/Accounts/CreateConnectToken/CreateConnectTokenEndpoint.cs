using Carter;
using MediatR;

namespace ApiService.Features.Accounts.CreateConnectToken;

public class CreateConnectTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts/connect-token", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreateConnectTokenCommand();
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateConnectToken")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<ConnectTokenDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
