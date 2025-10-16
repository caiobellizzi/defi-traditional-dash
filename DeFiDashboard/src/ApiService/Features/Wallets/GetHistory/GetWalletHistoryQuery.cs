using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.GetHistory;

public record GetWalletHistoryQuery(
    Guid WalletId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? TokenSymbol = null
) : IRequest<Result<IEnumerable<BalanceHistoryDto>>>;

public record BalanceHistoryDto
{
    public DateTime Date { get; init; }
    public string? TokenSymbol { get; init; }
    public string? Chain { get; init; }
    public decimal Balance { get; init; }
    public decimal? BalanceUsd { get; init; }
}
