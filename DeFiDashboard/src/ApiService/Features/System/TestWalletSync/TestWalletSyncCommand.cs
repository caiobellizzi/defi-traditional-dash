using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.TestWalletSync;

public record TestWalletSyncCommand : IRequest<Result<TestWalletSyncResultDto>>;

public record TestWalletSyncResultDto
{
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public int WalletsProcessed { get; init; }
    public int TotalBalancesUpdated { get; init; }
    public int TotalTransactionsAdded { get; init; }
    public List<string> Errors { get; init; } = new();
}
