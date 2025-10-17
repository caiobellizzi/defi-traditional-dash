using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.Update;

public record UpdateAllocationCommand(
    Guid Id,
    string AllocationType, // "Percentage" or "FixedAmount"
    decimal AllocationValue,
    DateTime StartDate,
    string? Notes
) : IRequest<Result<bool>>;
