using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ApiService.BackgroundJobs;

public class AlertGenerationJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AlertGenerationJob> _logger;

    // Thresholds
    private const decimal LOW_BALANCE_THRESHOLD_USD = 1000m;
    private const decimal ALLOCATION_DRIFT_THRESHOLD_PERCENT = 10m;
    private const int FAILED_SYNC_ALERT_HOURS = 24;

    public AlertGenerationJob(
        ApplicationDbContext context,
        ILogger<AlertGenerationJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 180 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting alert generation job");

        try
        {
            await CheckLowWalletBalancesAsync();
            await CheckLowAccountBalancesAsync();
            await CheckAllocationDriftAsync();
            await CheckFailedSyncsAsync();

            _logger.LogInformation("Alert generation job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in alert generation job");
            throw;
        }
    }

    private async Task CheckLowWalletBalancesAsync()
    {
        _logger.LogInformation("Checking for low wallet balances");

        var wallets = await _context.CustodyWallets
            .Where(w => w.Status == "Active")
            .ToListAsync();

        foreach (var wallet in wallets)
        {
            var totalBalance = await _context.WalletBalances
                .Where(b => b.WalletId == wallet.Id)
                .SumAsync(b => b.BalanceUsd ?? 0);

            if (totalBalance < LOW_BALANCE_THRESHOLD_USD)
            {
                await CreateOrUpdateAlertAsync(
                    alertType: "LowBalance",
                    severity: "Warning",
                    message: $"Wallet {wallet.Label ?? wallet.WalletAddress} has a low balance of ${totalBalance:N2} USD (threshold: ${LOW_BALANCE_THRESHOLD_USD:N2})",
                    clientId: null,
                    metadata: new { walletId = wallet.Id, balance = totalBalance, threshold = LOW_BALANCE_THRESHOLD_USD }
                );
            }
        }
    }

    private async Task CheckLowAccountBalancesAsync()
    {
        _logger.LogInformation("Checking for low account balances");

        var accounts = await _context.TraditionalAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync();

        foreach (var account in accounts)
        {
            var balance = await _context.AccountBalances
                .Where(b => b.AccountId == account.Id)
                .Select(b => b.Amount)
                .FirstOrDefaultAsync();

            if (balance < LOW_BALANCE_THRESHOLD_USD)
            {
                var accountName = account.Label ?? account.InstitutionName ?? "Unknown Account";
                await CreateOrUpdateAlertAsync(
                    alertType: "LowBalance",
                    severity: "Warning",
                    message: $"Account {accountName} has a low balance of ${balance:N2} USD (threshold: ${LOW_BALANCE_THRESHOLD_USD:N2})",
                    clientId: null,
                    metadata: new { accountId = account.Id, balance, threshold = LOW_BALANCE_THRESHOLD_USD }
                );
            }
        }
    }

    private async Task CheckAllocationDriftAsync()
    {
        _logger.LogInformation("Checking for allocation drift");

        var clients = await _context.Clients
            .Where(c => c.Status == "Active")
            .ToListAsync();

        foreach (var client in clients)
        {
            var allocations = await _context.ClientAssetAllocations
                .Where(a => a.ClientId == client.Id && a.EndDate == null && a.AllocationType == "Percentage")
                .ToListAsync();

            foreach (var allocation in allocations)
            {
                decimal actualPercentage = 0;
                decimal totalValue = 0;
                decimal assetValue = 0;

                // Calculate total portfolio value for this client
                var allAllocations = await _context.ClientAssetAllocations
                    .Where(a => a.ClientId == client.Id && a.EndDate == null)
                    .ToListAsync();

                foreach (var a in allAllocations)
                {
                    if (a.AssetType == "Wallet")
                    {
                        var walletValue = await _context.WalletBalances
                            .Where(b => b.WalletId == a.AssetId)
                            .SumAsync(b => b.BalanceUsd ?? 0);
                        totalValue += walletValue;

                        if (a.AssetId == allocation.AssetId)
                        {
                            assetValue = walletValue;
                        }
                    }
                    else if (a.AssetType == "Account")
                    {
                        var accountBalance = await _context.AccountBalances
                            .Where(b => b.AccountId == a.AssetId)
                            .Select(b => b.Amount)
                            .FirstOrDefaultAsync();
                        totalValue += accountBalance;

                        if (a.AssetId == allocation.AssetId)
                        {
                            assetValue = accountBalance;
                        }
                    }
                }

                if (totalValue > 0)
                {
                    actualPercentage = (assetValue / totalValue) * 100;
                    var drift = Math.Abs(actualPercentage - allocation.AllocationValue);

                    if (drift > ALLOCATION_DRIFT_THRESHOLD_PERCENT)
                    {
                        await CreateOrUpdateAlertAsync(
                            alertType: "AllocationDrift",
                            severity: drift > 20 ? "High" : "Medium",
                            message: $"Client {client.Name}: Asset allocation has drifted by {drift:N2}% (Target: {allocation.AllocationValue:N2}%, Actual: {actualPercentage:N2}%)",
                            clientId: client.Id,
                            metadata: new
                            {
                                clientId = client.Id,
                                allocationId = allocation.Id,
                                assetType = allocation.AssetType,
                                assetId = allocation.AssetId,
                                targetPercentage = allocation.AllocationValue,
                                actualPercentage,
                                drift
                            }
                        );
                    }
                }
            }
        }
    }

    private async Task CheckFailedSyncsAsync()
    {
        _logger.LogInformation("Checking for failed syncs");

        var cutoffTime = DateTime.UtcNow.AddHours(-FAILED_SYNC_ALERT_HOURS);

        // Check wallets with failed syncs (based on most recent balance update)
        var wallets = await _context.CustodyWallets
            .Where(w => w.Status == "Active")
            .ToListAsync();

        foreach (var wallet in wallets)
        {
            var lastBalanceUpdate = await _context.WalletBalances
                .Where(b => b.WalletId == wallet.Id)
                .OrderByDescending(b => b.LastUpdated)
                .Select(b => b.LastUpdated)
                .FirstOrDefaultAsync();

            if (lastBalanceUpdate == default || lastBalanceUpdate < cutoffTime)
            {
                await CreateOrUpdateAlertAsync(
                    alertType: "SyncFailure",
                    severity: "High",
                    message: $"Wallet {wallet.Label ?? wallet.WalletAddress} has not synced successfully in the last {FAILED_SYNC_ALERT_HOURS} hours",
                    clientId: null,
                    metadata: new
                    {
                        walletId = wallet.Id,
                        lastSyncAt = lastBalanceUpdate != default ? (DateTime?)lastBalanceUpdate : null,
                        hoursSinceSync = lastBalanceUpdate != default
                            ? (DateTime.UtcNow - lastBalanceUpdate).TotalHours
                            : (double?)null
                    }
                );
            }
        }

        // Check accounts with failed syncs
        var failedAccounts = await _context.TraditionalAccounts
            .Where(a => a.Status == "Active" &&
                       (a.LastSyncAt == null || a.LastSyncAt < cutoffTime))
            .ToListAsync();

        foreach (var account in failedAccounts)
        {
            var accountName = account.Label ?? account.InstitutionName ?? "Unknown Account";
            await CreateOrUpdateAlertAsync(
                alertType: "SyncFailure",
                severity: "High",
                message: $"Account {accountName} has not synced successfully in the last {FAILED_SYNC_ALERT_HOURS} hours",
                clientId: null,
                metadata: new
                {
                    accountId = account.Id,
                    lastSyncAt = account.LastSyncAt,
                    hoursSinceSync = account.LastSyncAt.HasValue
                        ? (DateTime.UtcNow - account.LastSyncAt.Value).TotalHours
                        : (double?)null
                }
            );
        }
    }

    private async Task CreateOrUpdateAlertAsync(
        string alertType,
        string severity,
        string message,
        Guid? clientId,
        object metadata)
    {
        var metadataJson = System.Text.Json.JsonDocument.Parse(
            System.Text.Json.JsonSerializer.Serialize(metadata));

        // Check if alert already exists and is not resolved
        var existingAlert = await _context.RebalancingAlerts
            .FirstOrDefaultAsync(a =>
                a.AlertType == alertType &&
                a.ClientId == clientId &&
                a.Status != "Resolved");

        if (existingAlert != null)
        {
            // Update existing alert
            existingAlert.Severity = severity;
            existingAlert.Message = message;
            existingAlert.AlertData = metadataJson;
            existingAlert.CreatedAt = DateTime.UtcNow; // Update timestamp

            _logger.LogInformation("Updated existing alert {AlertId}: {Message}", existingAlert.Id, message);
        }
        else
        {
            // Create new alert
            var alert = new RebalancingAlert
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AlertType = alertType,
                Severity = severity,
                Message = message,
                Status = "Active",
                AlertData = metadataJson,
                CreatedAt = DateTime.UtcNow
            };

            _context.RebalancingAlerts.Add(alert);

            _logger.LogInformation("Created new alert: {Message}", message);
        }

        await _context.SaveChangesAsync();
    }
}
