using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.Validate;

public record ValidateAllocationCommand(
    Guid ClientId,
    string AssetType, // "Wallet" or "Account"
    Guid AssetId,
    string AllocationType, // "Percentage" or "FixedAmount"
    decimal AllocationValue,
    DateTime StartDate,
    Guid? ExcludeAllocationId = null // For update scenarios
) : IRequest<Result<AllocationValidationResult>>;

public record AllocationValidationResult(
    bool IsValid,
    IEnumerable<string> Errors,
    IEnumerable<string> Warnings,
    decimal? CurrentTotalPercentage = null,
    decimal? NewTotalPercentage = null
);
