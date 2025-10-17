using ApiService.Common.Database;
using ApiService.Features.Allocations.GetByClient;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.GetList;

public class GetAllocationsHandler : IRequestHandler<GetAllocationsQuery, Result<PagedResult<AllocationDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllocationsHandler> _logger;

    public GetAllocationsHandler(
        ApplicationDbContext context,
        ILogger<GetAllocationsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AllocationDto>>> Handle(
        GetAllocationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.ClientAssetAllocations.AsNoTracking();

            // Apply filters
            if (request.ClientId.HasValue)
            {
                query = query.Where(a => a.ClientId == request.ClientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.AssetType))
            {
                query = query.Where(a => a.AssetType == request.AssetType);
            }

            if (request.AssetId.HasValue)
            {
                query = query.Where(a => a.AssetId == request.AssetId.Value);
            }

            if (request.ActiveOnly)
            {
                query = query.Where(a => a.EndDate == null);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering (newest first)
            var allocations = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AllocationDto
                {
                    Id = a.Id,
                    ClientId = a.ClientId,
                    AssetType = a.AssetType,
                    AssetId = a.AssetId,
                    AssetIdentifier = a.AssetType == "Wallet"
                        ? _context.CustodyWallets.Where(w => w.Id == a.AssetId).Select(w => w.WalletAddress).FirstOrDefault() ?? ""
                        : _context.TraditionalAccounts.Where(ac => ac.Id == a.AssetId).Select(ac => ac.AccountNumber ?? "").FirstOrDefault() ?? "",
                    AllocationType = a.AllocationType,
                    AllocationValue = a.AllocationValue,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<AllocationDto>(
                allocations,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return Result<PagedResult<AllocationDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocations");
            return Result<PagedResult<AllocationDto>>.Failure("An error occurred while retrieving allocations");
        }
    }
}
