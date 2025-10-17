using ApiService.Common.DTOs;
using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.Sync;

public record SyncAccountCommand(Guid AccountId) : IRequest<Result<SyncResultDto>>;
