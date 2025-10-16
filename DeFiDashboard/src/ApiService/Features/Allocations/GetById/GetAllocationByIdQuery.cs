using ApiService.Features.Allocations.GetByClient;
using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.GetById;

public record GetAllocationByIdQuery(
    Guid Id
) : IRequest<Result<AllocationDto>>;
