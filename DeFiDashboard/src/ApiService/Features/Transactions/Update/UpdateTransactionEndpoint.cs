using Carter;
using MediatR;

namespace ApiService.Features.Transactions.Update;

public class UpdateTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/transactions/{id:guid}", async (
            Guid id,
            UpdateTransactionRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateTransactionCommand(
                id,
                request.TransactionHash,
                request.Chain,
                request.Direction,
                request.FromAddress,
                request.ToAddress,
                request.TokenSymbol,
                request.Amount,
                request.AmountUsd,
                request.Fee,
                request.FeeUsd,
                request.Description,
                request.Category,
                request.TransactionDate,
                request.Reason
            );

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateTransaction")
        .WithTags("Transactions")
        .WithOpenApi();
    }
}

public record UpdateTransactionRequest(
    string? TransactionHash,
    string? Chain,
    string Direction,
    string? FromAddress,
    string? ToAddress,
    string? TokenSymbol,
    decimal Amount,
    decimal? AmountUsd,
    decimal? Fee,
    decimal? FeeUsd,
    string? Description,
    string? Category,
    DateTime TransactionDate,
    string? Reason
);
