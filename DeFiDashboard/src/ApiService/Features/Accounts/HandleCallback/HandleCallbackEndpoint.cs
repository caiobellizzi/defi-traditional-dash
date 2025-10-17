using Carter;
using MediatR;

namespace ApiService.Features.Accounts.HandleCallback;

public class HandleCallbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts/callback", async (
            CallbackRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new HandleCallbackCommand(request.ItemId, request.ExecutionStatus);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("HandlePluggyCallback")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<CallbackResultDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record CallbackRequest(
    string ItemId,
    string? ExecutionStatus
);
