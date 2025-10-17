using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.DTOs;
using ApiService.Common.Providers;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.Sync;

public class SyncWalletHandler : IRequestHandler<SyncWalletCommand, Result<SyncResultDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IBlockchainDataProvider _blockchainProvider;
    private readonly ILogger<SyncWalletHandler> _logger;

    public SyncWalletHandler(
        ApplicationDbContext context,
        IBlockchainDataProvider blockchainProvider,
        ILogger<SyncWalletHandler> logger)
    {
        _context = context;
        _blockchainProvider = blockchainProvider;
        _logger = logger;
    }

    public async Task<Result<SyncResultDto>> Handle(
        SyncWalletCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _context.CustodyWallets
                .FirstOrDefaultAsync(w => w.Id == request.WalletId, cancellationToken);

            if (wallet == null)
            {
                return Result<SyncResultDto>.Failure("Wallet not found");
            }

            if (wallet.Status != "Active")
            {
                return Result<SyncResultDto>.Failure("Cannot sync inactive wallet");
            }

            _logger.LogInformation("Starting sync for wallet {WalletId} ({Address})",
                request.WalletId, wallet.WalletAddress);

            var balancesUpdated = 0;
            var transactionsAdded = 0;

            // 1. Sync wallet balances
            var balances = await _blockchainProvider.GetWalletBalancesAsync(
                wallet.WalletAddress,
                wallet.SupportedChains,
                cancellationToken);

            foreach (var balance in balances)
            {
                var existingBalance = await _context.WalletBalances
                    .FirstOrDefaultAsync(b =>
                        b.WalletId == wallet.Id &&
                        b.Chain == balance.Chain &&
                        b.TokenAddress == balance.TokenAddress,
                        cancellationToken);

                if (existingBalance != null)
                {
                    existingBalance.Balance = balance.Balance;
                    existingBalance.BalanceUsd = balance.BalanceUsd;
                    existingBalance.LastUpdated = DateTime.UtcNow;
                }
                else
                {
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
                balancesUpdated++;
            }

            // 2. Sync transactions for each supported chain
            foreach (var chain in wallet.SupportedChains ?? Array.Empty<string>())
            {
                try
                {
                    // Get transactions from the last 30 days (Moralis limitation)
                    var fromDate = DateTime.UtcNow.AddDays(-30);
                    var transactions = await _blockchainProvider.GetWalletTransactionsAsync(
                        wallet.WalletAddress,
                        chain,
                        fromDate,
                        DateTime.UtcNow,
                        cancellationToken);

                    foreach (var tx in transactions)
                    {
                        // Check if transaction already exists
                        var existingTx = await _context.Transactions
                            .FirstOrDefaultAsync(t =>
                                t.TransactionHash == tx.TransactionHash &&
                                t.Chain == tx.Chain,
                                cancellationToken);

                        if (existingTx == null)
                        {
                            var direction = DetermineTransactionDirection(tx, wallet.WalletAddress);

                            _context.Transactions.Add(new Transaction
                            {
                                Id = Guid.NewGuid(),
                                TransactionType = "Wallet",
                                AssetId = wallet.Id,
                                TransactionHash = tx.TransactionHash,
                                Chain = tx.Chain,
                                Direction = direction,
                                FromAddress = tx.FromAddress,
                                ToAddress = tx.ToAddress,
                                TokenSymbol = tx.TokenSymbol,
                                Amount = tx.Amount,
                                AmountUsd = tx.AmountUsd,
                                Fee = tx.Fee,
                                FeeUsd = tx.FeeUsd,
                                TransactionDate = tx.TransactionDate,
                                Status = tx.Status,
                                IsManualEntry = false,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                            transactionsAdded++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync transactions for chain {Chain}", chain);
                }
            }

            // Update wallet's updated timestamp
            wallet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Wallet sync completed for {WalletId}. Balances: {Balances}, Transactions: {Transactions}",
                request.WalletId, balancesUpdated, transactionsAdded);

            var result = new SyncResultDto
            {
                Success = true,
                Message = $"Wallet synced successfully via {_blockchainProvider.ProviderName}",
                SyncedAt = DateTime.UtcNow,
                BalancesUpdated = balancesUpdated,
                TransactionsAdded = transactionsAdded
            };

            return Result<SyncResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing wallet {WalletId}", request.WalletId);
            return Result<SyncResultDto>.Failure("An error occurred while syncing the wallet");
        }
    }

    private static string DetermineTransactionDirection(TokenTransaction tx, string walletAddress)
    {
        var isFrom = tx.FromAddress.Equals(walletAddress, StringComparison.OrdinalIgnoreCase);
        var isTo = tx.ToAddress.Equals(walletAddress, StringComparison.OrdinalIgnoreCase);

        if (isFrom && isTo)
            return "INTERNAL";
        if (isFrom)
            return "OUT";
        return "IN";
    }
}
