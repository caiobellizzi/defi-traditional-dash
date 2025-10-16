using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.Delete;

public record DeleteWalletCommand(Guid Id) : IRequest<Result<bool>>;
