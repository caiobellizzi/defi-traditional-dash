using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.Sync;

public record SyncAccountCommand(Guid AccountId) : IRequest<Result<SyncResultDto>>;

public record SyncResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime SyncedAt { get; init; }
    public int BalancesUpdated { get; init; }
    public int TransactionsAdded { get; init; }
}
