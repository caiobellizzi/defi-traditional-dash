using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.TriggerWalletSync;

public record TriggerWalletSyncCommand : IRequest<Result<SyncJobDto>>;

public record SyncJobDto
{
    public Guid JobId { get; init; }
    public string JobType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
}
