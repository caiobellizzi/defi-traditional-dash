using ApiService.Common.Database;
using ApiService.Common.Utilities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.Update;

public class UpdateAllocationHandler : IRequestHandler<UpdateAllocationCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateAllocationHandler> _logger;

    public UpdateAllocationHandler(
        ApplicationDbContext context,
        ILogger<UpdateAllocationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateAllocationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var allocation = await _context.ClientAssetAllocations
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (allocation == null)
            {
                return Result<bool>.Failure("Allocation not found");
            }

            // Business Rule: Only active allocations can be updated
            if (allocation.EndDate != null)
            {
                return Result<bool>.Failure("Cannot update ended allocations. Create a new allocation instead.");
            }

            // Validate percentage allocations don't exceed 100%
            if (request.AllocationType == "Percentage")
            {
                var totalPercentage = await _context.ClientAssetAllocations
                    .Where(a => a.AssetType == allocation.AssetType
                        && a.AssetId == allocation.AssetId
                        && a.EndDate == null
                        && a.AllocationType == "Percentage"
                        && a.Id != allocation.Id) // Exclude current allocation
                    .SumAsync(a => a.AllocationValue, cancellationToken);

                if (totalPercentage + request.AllocationValue > 100)
                {
                    return Result<bool>.Failure(
                        $"Total percentage allocation would exceed 100%. Current total: {totalPercentage}%, Requested: {request.AllocationValue}%");
                }
            }

            // Update allocation
            allocation.AllocationType = request.AllocationType;
            allocation.AllocationValue = request.AllocationValue;
            allocation.StartDate = request.StartDate;
            allocation.Notes = InputSanitizer.Sanitize(request.Notes);
            allocation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated allocation {AllocationId} for client {ClientId}",
                allocation.Id, allocation.ClientId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating allocation {AllocationId}", request.Id);
            return Result<bool>.Failure("An error occurred while updating the allocation");
        }
    }
}
