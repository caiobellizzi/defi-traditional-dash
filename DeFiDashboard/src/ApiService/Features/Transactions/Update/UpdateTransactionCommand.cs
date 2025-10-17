using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Transactions.Update;

public record UpdateTransactionCommand(
    Guid Id,
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
) : IRequest<Result<bool>>;
