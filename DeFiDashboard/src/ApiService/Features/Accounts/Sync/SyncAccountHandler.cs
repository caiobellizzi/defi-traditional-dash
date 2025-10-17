using ApiService.Common.Database;
using Entities = ApiService.Common.Database.Entities;
using ApiService.Common.DTOs;
using ApiService.Common.Providers;
using ApiService.Features.Clients.Create;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.Sync;

public class SyncAccountHandler : IRequestHandler<SyncAccountCommand, Result<SyncResultDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenFinanceProvider _openFinanceProvider;
    private readonly ILogger<SyncAccountHandler> _logger;

    public SyncAccountHandler(
        ApplicationDbContext context,
        IOpenFinanceProvider openFinanceProvider,
        ILogger<SyncAccountHandler> logger)
    {
        _context = context;
        _openFinanceProvider = openFinanceProvider;
        _logger = logger;
    }

    public async Task<Result<SyncResultDto>> Handle(
        SyncAccountCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _context.TraditionalAccounts
                .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            if (account == null)
            {
                return Result<SyncResultDto>.Failure("Account not found");
            }

            if (account.Status != "Active")
            {
                return Result<SyncResultDto>.Failure("Cannot sync inactive account");
            }

            _logger.LogInformation("Starting sync for account {AccountId}", request.AccountId);

            var balancesUpdated = 0;
            var transactionsAdded = 0;

            // Sync account balance
            var balance = await _openFinanceProvider.GetAccountBalanceAsync(
                account.PluggyAccountId,
                cancellationToken);

            if (balance != null)
            {
                var existingBalance = await _context.AccountBalances
                    .FirstOrDefaultAsync(b => b.AccountId == account.Id && b.BalanceType == "CURRENT", cancellationToken);

                if (existingBalance != null)
                {
                    existingBalance.Amount = balance.Current;
                    existingBalance.Currency = balance.CurrencyCode;
                    existingBalance.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    _context.AccountBalances.Add(new Entities.AccountBalance
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        BalanceType = "CURRENT",
                        Amount = balance.Current,
                        Currency = balance.CurrencyCode,
                        LastUpdated = DateTime.UtcNow
                    });
                }
                balancesUpdated = 1;
            }

            // Sync transactions from last 30 days
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;

            var transactions = await _openFinanceProvider.GetAccountTransactionsAsync(
                account.PluggyAccountId,
                fromDate,
                toDate,
                cancellationToken);

            foreach (var txn in transactions)
            {
                // Check if transaction already exists
                var exists = await _context.Transactions
                    .AnyAsync(t =>
                        t.TransactionType == "Account" &&
                        t.AssetId == account.Id &&
                        t.ExternalId == txn.Id,
                        cancellationToken);

                if (!exists)
                {
                    _context.Transactions.Add(new Entities.Transaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionType = "Account",
                        AssetId = account.Id,
                        ExternalId = txn.Id,
                        Direction = txn.Amount >= 0 ? "IN" : "OUT",
                        Amount = Math.Abs(txn.Amount),
                        AmountUsd = null, // Would need currency conversion
                        TransactionDate = txn.Date,
                        Description = txn.Description,
                        Category = txn.Category,
                        Status = txn.Status,
                        IsManualEntry = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    transactionsAdded++;
                }
            }

            // Update sync status
            account.LastSyncAt = DateTime.UtcNow;
            account.SyncStatus = "Success";
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully synced account {AccountId}: {Balances} balances, {Transactions} transactions",
                account.Id, balancesUpdated, transactionsAdded);

            var result = new SyncResultDto
            {
                Success = true,
                Message = "Account synced successfully",
                SyncedAt = DateTime.UtcNow,
                BalancesUpdated = balancesUpdated,
                TransactionsAdded = transactionsAdded
            };

            return Result<SyncResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing account {AccountId}", request.AccountId);
            return Result<SyncResultDto>.Failure("An error occurred while syncing the account");
        }
    }
}
