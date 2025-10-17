using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.GetConflicts;

public record GetAllocationConflictsQuery() : IRequest<Result<IEnumerable<AllocationConflictDto>>>;

public record AllocationConflictDto(
    string AssetType,
    Guid AssetId,
    string AssetIdentifier,
    decimal TotalPercentage,
    int AllocationCount,
    IEnumerable<ConflictingAllocationDto> Allocations
);

public record ConflictingAllocationDto(
    Guid AllocationId,
    Guid ClientId,
    string ClientName,
    decimal AllocationValue,
    DateTime StartDate
);
