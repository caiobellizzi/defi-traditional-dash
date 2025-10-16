using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.Delete;

public record DeleteAllocationCommand(
    Guid Id
) : IRequest<Result<bool>>;
