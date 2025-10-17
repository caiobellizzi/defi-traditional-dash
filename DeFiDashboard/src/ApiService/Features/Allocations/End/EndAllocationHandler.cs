using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.End;

public class EndAllocationHandler : IRequestHandler<EndAllocationCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EndAllocationHandler> _logger;

    public EndAllocationHandler(
        ApplicationDbContext context,
        ILogger<EndAllocationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        EndAllocationCommand request,
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

            // Check if already ended
            if (allocation.EndDate != null)
            {
                return Result<bool>.Failure("Allocation is already ended");
            }

            // Set end date (use provided date or today)
            var endDate = request.EndDate ?? DateTime.UtcNow.Date;

            // Validate end date is not before start date
            if (endDate < allocation.StartDate)
            {
                return Result<bool>.Failure("End date cannot be before start date");
            }

            // Validate end date is not in the future
            if (endDate > DateTime.UtcNow.Date)
            {
                return Result<bool>.Failure("End date cannot be in the future");
            }

            allocation.EndDate = endDate;
            allocation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Ended allocation {AllocationId} for client {ClientId} with end date {EndDate}",
                allocation.Id, allocation.ClientId, endDate);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending allocation {AllocationId}", request.Id);
            return Result<bool>.Failure("An error occurred while ending the allocation");
        }
    }
}
