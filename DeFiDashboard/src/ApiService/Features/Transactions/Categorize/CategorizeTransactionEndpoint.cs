using Carter;
using MediatR;

namespace ApiService.Features.Transactions.Categorize;

public class CategorizeTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/transactions/{id:guid}/categorize", async (
            Guid id,
            CategorizeRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CategorizeTransactionCommand(id, request.Category);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CategorizeTransaction")
        .WithTags("Transactions")
        .WithOpenApi();
    }
}

public record CategorizeRequest(string? Category);
