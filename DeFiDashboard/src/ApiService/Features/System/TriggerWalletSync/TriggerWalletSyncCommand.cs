using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.TriggerWalletSync;

public record TriggerWalletSyncCommand : IRequest<Result<SyncJobDto>>;
