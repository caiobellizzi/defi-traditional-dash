using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.Delete;

public record DeleteAccountCommand(Guid Id) : IRequest<Result<bool>>;
