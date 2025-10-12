using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;

namespace ApiService.Features.Clients.GetById;

public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;
