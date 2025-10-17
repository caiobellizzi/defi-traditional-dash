using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.GetBalances;

public record GetWalletBalancesQuery(Guid WalletId) : IRequest<Result<IEnumerable<WalletBalanceDto>>>;

public record WalletBalanceDto
{
    public Guid Id { get; init; }
    public string Chain { get; init; } = string.Empty;
    public string? TokenAddress { get; init; }
    public string TokenSymbol { get; init; } = string.Empty;
    public string? TokenName { get; init; }
    public int? TokenDecimals { get; init; }
    public decimal Balance { get; init; }
    public decimal? BalanceUsd { get; init; }
    public DateTime LastUpdated { get; init; }
}
