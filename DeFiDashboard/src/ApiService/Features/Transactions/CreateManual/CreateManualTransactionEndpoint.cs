using Carter;
using MediatR;

namespace ApiService.Features.Transactions.CreateManual;

public class CreateManualTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions", async (
            CreateManualTransactionCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/transactions/{result.Value}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateManualTransaction")
        .WithTags("Transactions")
        .WithOpenApi();
    }
}
