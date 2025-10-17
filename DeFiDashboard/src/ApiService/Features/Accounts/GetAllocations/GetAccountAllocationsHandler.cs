using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.GetAllocations;

public class GetAccountAllocationsHandler : IRequestHandler<GetAccountAllocationsQuery, Result<IEnumerable<AllocationDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAccountAllocationsHandler> _logger;

    public GetAccountAllocationsHandler(ApplicationDbContext context, ILogger<GetAccountAllocationsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<AllocationDto>>> Handle(
        GetAccountAllocationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountExists = await _context.TraditionalAccounts
                .AnyAsync(a => a.Id == request.AccountId, cancellationToken);

            if (!accountExists)
            {
                return Result<IEnumerable<AllocationDto>>.Failure("Account not found");
            }

            var allocations = await _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.AssetType == "Account" && a.AssetId == request.AccountId)
                .OrderByDescending(a => a.StartDate)
                .Select(a => new AllocationDto
                {
                    Id = a.Id,
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name,
                    AllocationType = a.AllocationType,
                    AllocationValue = a.AllocationValue,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Notes = a.Notes
                })
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<AllocationDto>>.Success(allocations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocations for account {AccountId}", request.AccountId);
            return Result<IEnumerable<AllocationDto>>.Failure("An error occurred while retrieving account allocations");
        }
    }
}
