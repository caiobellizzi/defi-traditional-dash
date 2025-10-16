using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.GetBalances;

public class GetWalletBalancesHandler : IRequestHandler<GetWalletBalancesQuery, Result<IEnumerable<WalletBalanceDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetWalletBalancesHandler> _logger;

    public GetWalletBalancesHandler(ApplicationDbContext context, ILogger<GetWalletBalancesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<WalletBalanceDto>>> Handle(
        GetWalletBalancesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var walletExists = await _context.CustodyWallets
                .AnyAsync(w => w.Id == request.WalletId, cancellationToken);

            if (!walletExists)
            {
                return Result<IEnumerable<WalletBalanceDto>>.Failure("Wallet not found");
            }

            var balances = await _context.WalletBalances
                .AsNoTracking()
                .Where(b => b.WalletId == request.WalletId)
                .OrderByDescending(b => b.BalanceUsd ?? 0)
                .Select(b => new WalletBalanceDto
                {
                    Id = b.Id,
                    Chain = b.Chain,
                    TokenAddress = b.TokenAddress,
                    TokenSymbol = b.TokenSymbol,
                    TokenName = b.TokenName,
                    TokenDecimals = b.TokenDecimals,
                    Balance = b.Balance,
                    BalanceUsd = b.BalanceUsd,
                    LastUpdated = b.LastUpdated
                })
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<WalletBalanceDto>>.Success(balances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balances for wallet {WalletId}", request.WalletId);
            return Result<IEnumerable<WalletBalanceDto>>.Failure("An error occurred while retrieving wallet balances");
        }
    }
}
