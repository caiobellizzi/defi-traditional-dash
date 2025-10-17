using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.GetBalances;

public record GetAccountBalancesQuery(Guid AccountId) : IRequest<Result<IEnumerable<AccountBalanceDto>>>;

public record AccountBalanceDto
{
    public Guid Id { get; init; }
    public string BalanceType { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime LastUpdated { get; init; }
}
