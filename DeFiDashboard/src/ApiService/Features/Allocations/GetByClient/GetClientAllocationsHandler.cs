using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.GetByClient;

public class GetClientAllocationsHandler : IRequestHandler<GetClientAllocationsQuery, Result<IEnumerable<AllocationDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetClientAllocationsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<AllocationDto>>> Handle(
        GetClientAllocationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.ClientId == request.ClientId);

            if (request.ActiveOnly)
            {
                query = query.Where(a => a.EndDate == null);
            }

            var allocations = await query
                .OrderByDescending(a => a.CreatedAt)
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

            return Result<IEnumerable<AllocationDto>>.Success(allocations);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AllocationDto>>.Failure("An error occurred while retrieving allocations");
        }
    }
}
