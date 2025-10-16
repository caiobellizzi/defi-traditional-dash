using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ApiService.BackgroundJobs;

public class PortfolioCalculationJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PortfolioCalculationJob> _logger;

    public PortfolioCalculationJob(
        ApplicationDbContext context,
        ILogger<PortfolioCalculationJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 180 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting portfolio calculation job");

        try
        {
            // Get all active clients
            var clients = await _context.Clients
                .Where(c => c.Status == "Active")
                .ToListAsync();

            _logger.LogInformation("Calculating portfolios for {Count} clients", clients.Count);

            foreach (var client in clients)
            {
                try
                {
                    await CalculateClientPortfolioAsync(client.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating portfolio for client {ClientId}", client.Id);
                }
            }

            // Calculate consolidated portfolio metrics
            await CalculateConsolidatedMetricsAsync();

            _logger.LogInformation("Portfolio calculation job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in portfolio calculation job");
            throw;
        }
    }

    private async Task CalculateClientPortfolioAsync(Guid clientId)
    {
        _logger.LogInformation("Calculating portfolio for client {ClientId}", clientId);

        // Get client allocations
        var allocations = await _context.ClientAssetAllocations
            .Where(a => a.ClientId == clientId && a.EndDate == null)
            .ToListAsync();

        decimal totalValueUsd = 0;
        var assetBreakdown = new Dictionary<string, decimal>();

        // Calculate wallet allocations
        foreach (var allocation in allocations.Where(a => a.AssetType == "Wallet"))
        {
            var walletBalances = await _context.WalletBalances
                .Where(b => b.WalletId == allocation.AssetId)
                .ToListAsync();

            foreach (var balance in walletBalances)
            {
                decimal allocatedValue = 0;

                if (allocation.AllocationType == "Percentage")
                {
                    allocatedValue = (balance.BalanceUsd ?? 0) * (allocation.AllocationValue / 100);
                }
                else // FixedAmount
                {
                    allocatedValue = allocation.AllocationValue;
                }

                totalValueUsd += allocatedValue;

                var key = $"{balance.TokenSymbol} ({balance.Chain})";
                if (assetBreakdown.ContainsKey(key))
                {
                    assetBreakdown[key] += allocatedValue;
                }
                else
                {
                    assetBreakdown[key] = allocatedValue;
                }
            }
        }

        // Calculate account allocations
        foreach (var allocation in allocations.Where(a => a.AssetType == "Account"))
        {
            var accountBalance = await _context.AccountBalances
                .FirstOrDefaultAsync(b => b.AccountId == allocation.AssetId);

            if (accountBalance != null)
            {
                decimal allocatedValue = 0;

                if (allocation.AllocationType == "Percentage")
                {
                    allocatedValue = accountBalance.Amount * (allocation.AllocationValue / 100);
                }
                else // FixedAmount
                {
                    allocatedValue = allocation.AllocationValue;
                }

                totalValueUsd += allocatedValue;

                var key = $"Traditional ({accountBalance.Currency})";
                if (assetBreakdown.ContainsKey(key))
                {
                    assetBreakdown[key] += allocatedValue;
                }
                else
                {
                    assetBreakdown[key] = allocatedValue;
                }
            }
        }

        // Get or create performance metric record
        var metric = await _context.PerformanceMetrics
            .FirstOrDefaultAsync(m => m.ClientId == clientId && m.CalculationDate.Date == DateTime.UtcNow.Date);

        if (metric == null)
        {
            metric = new PerformanceMetric
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                CalculationDate = DateTime.UtcNow.Date,
                CalculatedAt = DateTime.UtcNow
            };
            _context.PerformanceMetrics.Add(metric);
        }

        // Update metric values
        metric.TotalValueUsd = totalValueUsd;
        metric.CalculatedAt = DateTime.UtcNow;

        // Calculate simple ROI (would need historical data for accurate calculation)
        // For now, we'll just track the current value
        metric.Roi = 0; // Placeholder - implement with historical price data

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Calculated portfolio for client {ClientId}: Total Value ${TotalValue:N2}, Assets: {AssetCount}",
            clientId, totalValueUsd, assetBreakdown.Count);
    }

    private async Task CalculateConsolidatedMetricsAsync()
    {
        _logger.LogInformation("Calculating consolidated portfolio metrics");

        var clients = await _context.Clients
            .Where(c => c.Status == "Active")
            .ToListAsync();

        var totalClientsValue = 0m;
        var clientCount = clients.Count;

        // Calculate total AUM (Assets Under Management)
        foreach (var client in clients)
        {
            var latestMetric = await _context.PerformanceMetrics
                .Where(m => m.ClientId == client.Id)
                .OrderByDescending(m => m.CalculationDate)
                .FirstOrDefaultAsync();

            if (latestMetric != null)
            {
                totalClientsValue += latestMetric.TotalValueUsd;
            }
        }

        // Get total wallet balances
        var totalWalletValue = await _context.WalletBalances
            .SumAsync(b => b.BalanceUsd ?? 0);

        // Get total account balances
        var totalAccountValue = await _context.AccountBalances
            .SumAsync(b => b.Amount);

        _logger.LogInformation(
            "Consolidated metrics: Clients: {ClientCount}, Total AUM: ${TotalAUM:N2}, Wallet Value: ${WalletValue:N2}, Account Value: ${AccountValue:N2}",
            clientCount, totalClientsValue, totalWalletValue, totalAccountValue);
    }
}
