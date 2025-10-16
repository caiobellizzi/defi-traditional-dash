using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.End;

public record EndAllocationCommand(
    Guid Id,
    DateTime? EndDate = null // If null, use DateTime.UtcNow.Date
) : IRequest<Result<bool>>;
