using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;

namespace ApiService.Features.Wallets.GetTransactions;

public record GetWalletTransactionsQuery(
    Guid WalletId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Direction = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<WalletTransactionDto>>>;

public record WalletTransactionDto
{
    public Guid Id { get; init; }
    public string? TransactionHash { get; init; }
    public string? Chain { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string? FromAddress { get; init; }
    public string? ToAddress { get; init; }
    public string? TokenSymbol { get; init; }
    public decimal Amount { get; init; }
    public decimal? AmountUsd { get; init; }
    public decimal? Fee { get; init; }
    public decimal? FeeUsd { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public DateTime TransactionDate { get; init; }
    public string Status { get; init; } = string.Empty;
}
