using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Clients.Delete;

public record DeleteClientCommand(Guid Id) : IRequest<Result<bool>>;
