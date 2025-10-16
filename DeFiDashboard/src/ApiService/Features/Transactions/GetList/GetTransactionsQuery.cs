using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Transactions.GetList;

public record GetTransactionsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? TransactionType = null, // 'Wallet' or 'Account'
    Guid? AssetId = null,
    string? Direction = null, // 'IN', 'OUT', 'INTERNAL'
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? TokenSymbol = null,
    string? Status = null
) : IRequest<Result<PagedResult<TransactionDto>>>;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record TransactionDto(
    Guid Id,
    string TransactionType,
    Guid AssetId,
    string? TransactionHash,
    string? ExternalId,
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
    bool IsManualEntry,
    string Status,
    DateTime CreatedAt
);
