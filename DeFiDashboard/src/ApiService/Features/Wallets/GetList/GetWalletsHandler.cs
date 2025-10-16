using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.GetList;

public class GetWalletsHandler : IRequestHandler<GetWalletsQuery, Result<PagedResult<WalletDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetWalletsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<WalletDto>>> Handle(
        GetWalletsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.CustodyWallets.AsNoTracking();

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(w => w.Status == request.Status);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var wallets = await query
                .OrderByDescending(w => w.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(w => new WalletDto
                {
                    Id = w.Id,
                    WalletAddress = w.WalletAddress,
                    Label = w.Label,
                    BlockchainProvider = w.BlockchainProvider,
                    SupportedChains = w.SupportedChains,
                    Status = w.Status,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WalletDto>
            {
                Items = wallets,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<WalletDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Note: ex is available but not logged - consider adding ILogger
            return Result<PagedResult<WalletDto>>.Failure("An error occurred while retrieving wallets");
        }
    }
}
