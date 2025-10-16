using Carter;
using MediatR;

namespace ApiService.Features.Transactions.Delete;

public class DeleteTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/transactions/{id:guid}", async (
            Guid id,
            string? reason,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new DeleteTransactionCommand(id, reason);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteTransaction")
        .WithTags("Transactions")
        .WithOpenApi();
    }
}
