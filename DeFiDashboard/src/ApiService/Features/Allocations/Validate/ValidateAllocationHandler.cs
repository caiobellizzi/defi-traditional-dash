using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.Validate;

public class ValidateAllocationHandler : IRequestHandler<ValidateAllocationCommand, Result<AllocationValidationResult>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ValidateAllocationHandler> _logger;

    public ValidateAllocationHandler(
        ApplicationDbContext context,
        ILogger<ValidateAllocationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AllocationValidationResult>> Handle(
        ValidateAllocationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            decimal? currentTotalPercentage = null;
            decimal? newTotalPercentage = null;

            // Verify client exists
            var clientExists = await _context.Clients
                .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

            if (!clientExists)
            {
                errors.Add("Client not found");
            }

            // Verify asset exists
            if (request.AssetType == "Wallet")
            {
                var walletExists = await _context.CustodyWallets
                    .AnyAsync(w => w.Id == request.AssetId, cancellationToken);

                if (!walletExists)
                {
                    errors.Add("Wallet not found");
                }
            }
            else
            {
                var accountExists = await _context.TraditionalAccounts
                    .AnyAsync(a => a.Id == request.AssetId, cancellationToken);

                if (!accountExists)
                {
                    errors.Add("Account not found");
                }
            }

            // Check for duplicate active allocations for same client+asset
            var duplicateExists = await _context.ClientAssetAllocations
                .AnyAsync(a => a.ClientId == request.ClientId
                    && a.AssetType == request.AssetType
                    && a.AssetId == request.AssetId
                    && a.EndDate == null
                    && (!request.ExcludeAllocationId.HasValue || a.Id != request.ExcludeAllocationId.Value),
                    cancellationToken);

            if (duplicateExists)
            {
                errors.Add("An active allocation already exists for this client and asset. End the existing allocation first.");
            }

            // Validate percentage allocations don't exceed 100%
            if (request.AllocationType == "Percentage")
            {
                currentTotalPercentage = await _context.ClientAssetAllocations
                    .Where(a => a.AssetType == request.AssetType
                        && a.AssetId == request.AssetId
                        && a.EndDate == null
                        && a.AllocationType == "Percentage"
                        && (!request.ExcludeAllocationId.HasValue || a.Id != request.ExcludeAllocationId.Value))
                    .SumAsync(a => a.AllocationValue, cancellationToken);

                newTotalPercentage = currentTotalPercentage + request.AllocationValue;

                if (newTotalPercentage > 100)
                {
                    errors.Add(
                        $"Total percentage allocation would exceed 100%. Current: {currentTotalPercentage}%, " +
                        $"Requested: {request.AllocationValue}%, New Total: {newTotalPercentage}%");
                }
                else if (newTotalPercentage > 90)
                {
                    warnings.Add(
                        $"Total percentage allocation is high ({newTotalPercentage}%). " +
                        $"Current: {currentTotalPercentage}%, Requested: {request.AllocationValue}%");
                }
            }

            // Check for overlapping date ranges (same client, same asset)
            var hasOverlap = await _context.ClientAssetAllocations
                .AnyAsync(a => a.ClientId == request.ClientId
                    && a.AssetType == request.AssetType
                    && a.AssetId == request.AssetId
                    && a.StartDate <= request.StartDate
                    && (a.EndDate == null || a.EndDate >= request.StartDate)
                    && (!request.ExcludeAllocationId.HasValue || a.Id != request.ExcludeAllocationId.Value),
                    cancellationToken);

            if (hasOverlap)
            {
                warnings.Add("Allocation dates overlap with existing allocation for this client and asset");
            }

            var validationResult = new AllocationValidationResult(
                !errors.Any(),
                errors,
                warnings,
                currentTotalPercentage,
                newTotalPercentage
            );

            return Result<AllocationValidationResult>.Success(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating allocation");
            return Result<AllocationValidationResult>.Failure(
                "An error occurred while validating the allocation");
        }
    }
}
