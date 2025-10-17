using ApiService.Common.Database;
using ApiService.Features.Allocations.GetByClient;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Allocations.GetById;

public class GetAllocationByIdHandler : IRequestHandler<GetAllocationByIdQuery, Result<AllocationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllocationByIdHandler> _logger;

    public GetAllocationByIdHandler(
        ApplicationDbContext context,
        ILogger<GetAllocationByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AllocationDto>> Handle(
        GetAllocationByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var allocation = await _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.Id == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (allocation == null)
            {
                return Result<AllocationDto>.Failure("Allocation not found");
            }

            return Result<AllocationDto>.Success(allocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocation {AllocationId}", request.Id);
            return Result<AllocationDto>.Failure("An error occurred while retrieving the allocation");
        }
    }
}
