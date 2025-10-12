using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Clients.Update;

public record UpdateClientCommand(
    Guid Id,
    string Name,
    string Email,
    string? Document,
    string? PhoneNumber,
    string? Notes,
    string Status
) : IRequest<Result<bool>>;
