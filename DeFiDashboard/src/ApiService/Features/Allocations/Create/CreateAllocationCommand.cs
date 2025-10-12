using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.Create;

public record CreateAllocationCommand(
    Guid ClientId,
    string AssetType, // "Wallet" or "Account"
    Guid AssetId,
    string AllocationType, // "Percentage" or "FixedAmount"
    decimal AllocationValue,
    DateTime StartDate,
    string? Notes
) : IRequest<Result<Guid>>;
