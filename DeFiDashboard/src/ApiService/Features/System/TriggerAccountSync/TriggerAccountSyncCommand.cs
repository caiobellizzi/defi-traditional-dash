using ApiService.Features.Alerts.GetList;
using ApiService.Features.System.TriggerWalletSync;
using MediatR;

namespace ApiService.Features.System.TriggerAccountSync;

public record TriggerAccountSyncCommand : IRequest<Result<SyncJobDto>>;
