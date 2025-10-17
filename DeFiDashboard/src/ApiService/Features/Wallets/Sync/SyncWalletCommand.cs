using ApiService.Common.DTOs;
using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.Sync;

public record SyncWalletCommand(Guid WalletId) : IRequest<Result<SyncResultDto>>;
