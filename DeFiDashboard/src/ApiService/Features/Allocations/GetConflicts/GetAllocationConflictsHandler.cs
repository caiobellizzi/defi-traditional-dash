using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.GetConflicts;

public class GetAllocationConflictsHandler : IRequestHandler<GetAllocationConflictsQuery, Result<IEnumerable<AllocationConflictDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllocationConflictsHandler> _logger;

    public GetAllocationConflictsHandler(
        ApplicationDbContext context,
        ILogger<GetAllocationConflictsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<AllocationConflictDto>>> Handle(
        GetAllocationConflictsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Find all assets with active percentage allocations exceeding 100%
            var conflicts = await _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.EndDate == null && a.AllocationType == "Percentage")
                .GroupBy(a => new { a.AssetType, a.AssetId })
                .Select(g => new
                {
                    g.Key.AssetType,
                    g.Key.AssetId,
                    TotalPercentage = g.Sum(a => a.AllocationValue),
                    AllocationCount = g.Count(),
                    Allocations = g.Select(a => new
                    {
                        a.Id,
                        a.ClientId,
                        a.AllocationValue,
                        a.StartDate
                    }).ToList()
                })
                .Where(x => x.TotalPercentage > 100)
                .ToListAsync(cancellationToken);

            if (!conflicts.Any())
            {
                return Result<IEnumerable<AllocationConflictDto>>.Success(
                    Enumerable.Empty<AllocationConflictDto>());
            }

            // Enrich with asset and client details
            var conflictDtos = new List<AllocationConflictDto>();

            foreach (var conflict in conflicts)
            {
                string assetIdentifier;
                if (conflict.AssetType == "Wallet")
                {
                    var wallet = await _context.CustodyWallets
                        .Where(w => w.Id == conflict.AssetId)
                        .Select(w => w.WalletAddress)
                        .FirstOrDefaultAsync(cancellationToken);
                    assetIdentifier = wallet ?? "Unknown";
                }
                else
                {
                    var account = await _context.TraditionalAccounts
                        .Where(a => a.Id == conflict.AssetId)
                        .Select(a => a.AccountNumber ?? "Unknown")
                        .FirstOrDefaultAsync(cancellationToken);
                    assetIdentifier = account ?? "Unknown";
                }

                var allocations = new List<ConflictingAllocationDto>();
                foreach (var alloc in conflict.Allocations)
                {
                    var client = await _context.Clients
                        .Where(c => c.Id == alloc.ClientId)
                        .Select(c => c.Name)
                        .FirstOrDefaultAsync(cancellationToken);

                    allocations.Add(new ConflictingAllocationDto(
                        alloc.Id,
                        alloc.ClientId,
                        client ?? "Unknown",
                        alloc.AllocationValue,
                        alloc.StartDate
                    ));
                }

                conflictDtos.Add(new AllocationConflictDto(
                    conflict.AssetType,
                    conflict.AssetId,
                    assetIdentifier,
                    conflict.TotalPercentage,
                    conflict.AllocationCount,
                    allocations
                ));
            }

            return Result<IEnumerable<AllocationConflictDto>>.Success(conflictDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocation conflicts");
            return Result<IEnumerable<AllocationConflictDto>>.Failure(
                "An error occurred while retrieving allocation conflicts");
        }
    }
}
