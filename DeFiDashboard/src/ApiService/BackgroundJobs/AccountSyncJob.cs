using ApiService.Common.Database;
using Entities = ApiService.Common.Database.Entities;
using ApiService.Common.Providers;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ApiService.BackgroundJobs;

public class AccountSyncJob
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenFinanceProvider _openFinanceProvider;
    private readonly ILogger<AccountSyncJob> _logger;

    public AccountSyncJob(
        ApplicationDbContext context,
        IOpenFinanceProvider openFinanceProvider,
        ILogger<AccountSyncJob> logger)
    {
        _context = context;
        _openFinanceProvider = openFinanceProvider;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting account sync job");

        try
        {
            // Get all active accounts
            var accounts = await _context.TraditionalAccounts
                .Where(a => a.Status == "Active")
                .ToListAsync();

            _logger.LogInformation("Syncing {Count} accounts", accounts.Count);

            foreach (var account in accounts)
            {
                try
                {
                    await SyncAccountDataAsync(account);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing account {AccountId}", account.Id);

                    // Update sync status to indicate error
                    account.SyncStatus = "Error";
                    account.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Account sync job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in account sync job");
            throw;
        }
    }

    private async Task SyncAccountDataAsync(Entities.TraditionalAccount account)
    {
        _logger.LogInformation("Syncing account {AccountId} ({Label})", account.Id, account.Label ?? account.InstitutionName ?? "Unknown");

        var balancesUpdated = 0;
        var transactionsAdded = 0;

        // Sync account balance
        var balance = await _openFinanceProvider.GetAccountBalanceAsync(account.PluggyAccountId);

        if (balance != null)
        {
            var existingBalance = await _context.AccountBalances
                .FirstOrDefaultAsync(b => b.AccountId == account.Id);

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

        // Sync transactions from last 7 days
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;

        var transactions = await _openFinanceProvider.GetAccountTransactionsAsync(
            account.PluggyAccountId,
            fromDate,
            toDate);

        foreach (var txn in transactions)
        {
            // Check if transaction already exists
            var exists = await _context.Transactions
                .AnyAsync(t =>
                    t.TransactionType == "Account" &&
                    t.AssetId == account.Id &&
                    t.ExternalId == txn.Id);

            if (!exists)
            {
                _context.Transactions.Add(new Entities.Transaction
                {
                    Id = Guid.NewGuid(),
                    TransactionType = "Account",
                    AssetId = account.Id,
                    Direction = txn.Amount >= 0 ? "IN" : "OUT",
                    Amount = Math.Abs(txn.Amount),
                    TransactionDate = txn.Date,
                    Description = txn.Description,
                    Category = txn.Category,
                    Status = txn.Status,
                    ExternalId = txn.Id,
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

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Synced account {AccountId}: {Balances} balances, {Transactions} transactions",
            account.Id, balancesUpdated, transactionsAdded);
    }
}
