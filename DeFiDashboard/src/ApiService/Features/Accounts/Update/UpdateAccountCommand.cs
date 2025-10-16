using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.Update;

public record UpdateAccountCommand(
    Guid Id,
    string? Label
) : IRequest<Result<bool>>;
