using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Transactions.CreateManual;

public record CreateManualTransactionCommand(
    string TransactionType, // "Wallet" or "Account"
    Guid AssetId,
    string? TransactionHash,
    string? Chain,
    string Direction, // "IN", "OUT", "INTERNAL"
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
) : IRequest<Result<Guid>>;
