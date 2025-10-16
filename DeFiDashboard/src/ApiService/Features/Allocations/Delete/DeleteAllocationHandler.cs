using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.Delete;

public class DeleteAllocationHandler : IRequestHandler<DeleteAllocationCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteAllocationHandler> _logger;

    public DeleteAllocationHandler(
        ApplicationDbContext context,
        ILogger<DeleteAllocationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteAllocationCommand request,
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

            // Log warning for hard delete
            _logger.LogWarning(
                "Hard deleting allocation {AllocationId} for client {ClientId}. Consider using End endpoint instead.",
                allocation.Id, allocation.ClientId);

            _context.ClientAssetAllocations.Remove(allocation);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted allocation {AllocationId} for client {ClientId}",
                allocation.Id, allocation.ClientId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting allocation {AllocationId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the allocation");
        }
    }
}
