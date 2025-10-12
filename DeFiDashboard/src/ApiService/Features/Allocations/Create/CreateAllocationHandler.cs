using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.Create;

public class CreateAllocationHandler : IRequestHandler<CreateAllocationCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateAllocationHandler> _logger;

    public CreateAllocationHandler(ApplicationDbContext context, ILogger<CreateAllocationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateAllocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify client exists
            var clientExists = await _context.Clients
                .AnyAsync(c => c.Id == request.ClientId && c.Status == "Active", cancellationToken);

            if (!clientExists)
            {
                return Result<Guid>.Failure("Client not found or inactive");
            }

            // Verify asset exists
            var assetExists = request.AssetType == "Wallet"
                ? await _context.CustodyWallets.AnyAsync(w => w.Id == request.AssetId, cancellationToken)
                : await _context.TraditionalAccounts.AnyAsync(a => a.Id == request.AssetId, cancellationToken);

            if (!assetExists)
            {
                return Result<Guid>.Failure($"{request.AssetType} not found");
            }

            // Check for existing active allocation
            var existingAllocation = await _context.ClientAssetAllocations
                .AnyAsync(a => a.ClientId == request.ClientId
                    && a.AssetType == request.AssetType
                    && a.AssetId == request.AssetId
                    && a.EndDate == null, cancellationToken);

            if (existingAllocation)
            {
                return Result<Guid>.Failure("An active allocation already exists for this client and asset");
            }

            // For percentage allocations, check total doesn't exceed 100%
            if (request.AllocationType == "Percentage")
            {
                var totalAllocated = await _context.ClientAssetAllocations
                    .Where(a => a.AssetType == request.AssetType
                        && a.AssetId == request.AssetId
                        && a.AllocationType == "Percentage"
                        && a.EndDate == null)
                    .SumAsync(a => a.AllocationValue, cancellationToken);

                if (totalAllocated + request.AllocationValue > 100)
                {
                    return Result<Guid>.Failure(
                        $"Total percentage allocation would exceed 100% (current: {totalAllocated}%)");
                }
            }

            var allocation = new ClientAssetAllocation
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                AssetType = request.AssetType,
                AssetId = request.AssetId,
                AllocationType = request.AllocationType,
                AllocationValue = request.AllocationValue,
                StartDate = request.StartDate,
                EndDate = null,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ClientAssetAllocations.Add(allocation);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created allocation {AllocationId} for client {ClientId}",
                allocation.Id, request.ClientId);

            return Result<Guid>.Success(allocation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating allocation for client {ClientId}", request.ClientId);
            return Result<Guid>.Failure("An error occurred while creating the allocation");
        }
    }
}
