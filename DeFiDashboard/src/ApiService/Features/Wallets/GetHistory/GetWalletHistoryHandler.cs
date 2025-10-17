using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.GetHistory;

public class GetWalletHistoryHandler : IRequestHandler<GetWalletHistoryQuery, Result<IEnumerable<BalanceHistoryDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetWalletHistoryHandler> _logger;

    public GetWalletHistoryHandler(ApplicationDbContext context, ILogger<GetWalletHistoryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<BalanceHistoryDto>>> Handle(
        GetWalletHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var walletExists = await _context.CustodyWallets
                .AnyAsync(w => w.Id == request.WalletId, cancellationToken);

            if (!walletExists)
            {
                return Result<IEnumerable<BalanceHistoryDto>>.Failure("Wallet not found");
            }

            // TODO: Implement balance history tracking
            // For now, return current balances as a single snapshot
            // In production, this would query a BalanceHistory table that stores
            // daily snapshots of wallet balances for performance charting

            var currentBalances = await _context.WalletBalances
                .AsNoTracking()
                .Where(b => b.WalletId == request.WalletId)
                .Select(b => new BalanceHistoryDto
                {
                    Date = b.LastUpdated,
                    TokenSymbol = b.TokenSymbol,
                    Chain = b.Chain,
                    Balance = b.Balance,
                    BalanceUsd = b.BalanceUsd
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Balance history requested for wallet {WalletId}. Returning current snapshot (placeholder)", request.WalletId);

            return Result<IEnumerable<BalanceHistoryDto>>.Success(currentBalances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance history for wallet {WalletId}", request.WalletId);
            return Result<IEnumerable<BalanceHistoryDto>>.Failure("An error occurred while retrieving wallet balance history");
        }
    }
}
