using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.Sync;

public record SyncWalletCommand(Guid WalletId) : IRequest<Result<SyncResultDto>>;

public record SyncResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime SyncedAt { get; init; }
    public int BalancesUpdated { get; init; }
    public int TransactionsAdded { get; init; }
}
