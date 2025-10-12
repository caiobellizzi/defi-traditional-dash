using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.Providers;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ApiService.BackgroundJobs;

public class WalletSyncJob
{
    private readonly ApplicationDbContext _context;
    private readonly IBlockchainDataProvider _blockchainProvider;
    private readonly ILogger<WalletSyncJob> _logger;

    public WalletSyncJob(
        ApplicationDbContext context,
        IBlockchainDataProvider blockchainProvider,
        ILogger<WalletSyncJob> logger)
    {
        _context = context;
        _blockchainProvider = blockchainProvider;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting wallet sync job");

        try
        {
            // Get all active wallets
            var wallets = await _context.CustodyWallets
                .Where(w => w.Status == "Active")
                .ToListAsync();

            _logger.LogInformation("Syncing {Count} wallets", wallets.Count);

            foreach (var wallet in wallets)
            {
                try
                {
                    await SyncWalletBalancesAsync(wallet);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing wallet {WalletId}", wallet.Id);
                }
            }

            _logger.LogInformation("Wallet sync job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in wallet sync job");
            throw;
        }
    }

    private async Task SyncWalletBalancesAsync(CustodyWallet wallet)
    {
        _logger.LogInformation("Syncing balances for wallet {Address}", wallet.WalletAddress);

        // Get balances from blockchain provider
        var balances = await _blockchainProvider.GetWalletBalancesAsync(
            wallet.WalletAddress,
            wallet.SupportedChains);

        foreach (var balance in balances)
        {
            // Find existing balance record
            var existingBalance = await _context.WalletBalances
                .FirstOrDefaultAsync(b =>
                    b.WalletId == wallet.Id &&
                    b.Chain == balance.Chain &&
                    b.TokenAddress == balance.TokenAddress);

            if (existingBalance != null)
            {
                // Update existing balance
                existingBalance.Balance = balance.Balance;
                existingBalance.BalanceUsd = balance.BalanceUsd;
                existingBalance.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                // Create new balance record
                _context.WalletBalances.Add(new WalletBalance
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Chain = balance.Chain,
                    TokenAddress = balance.TokenAddress,
                    TokenSymbol = balance.TokenSymbol,
                    TokenName = balance.TokenName,
                    TokenDecimals = balance.TokenDecimals,
                    Balance = balance.Balance,
                    BalanceUsd = balance.BalanceUsd,
                    LastUpdated = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Synced {Count} balances for wallet {Address}",
            balances.Count(), wallet.WalletAddress);
    }
}
