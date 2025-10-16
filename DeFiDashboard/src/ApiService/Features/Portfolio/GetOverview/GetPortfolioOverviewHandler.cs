using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Portfolio.GetOverview;

public class GetPortfolioOverviewHandler : IRequestHandler<GetPortfolioOverviewQuery, Result<PortfolioOverviewDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetPortfolioOverviewHandler> _logger;

    public GetPortfolioOverviewHandler(ApplicationDbContext context, ILogger<GetPortfolioOverviewHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PortfolioOverviewDto>> Handle(
        GetPortfolioOverviewQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Calculate crypto value from wallet balances
            var cryptoValue = await _context.WalletBalances
                .AsNoTracking()
                .Where(wb => wb.BalanceUsd.HasValue)
                .SumAsync(wb => wb.BalanceUsd!.Value, cancellationToken);

            // Calculate traditional value from account balances (assuming BRL, conversion rate placeholder)
            const decimal brlToUsdRate = 0.20m; // Placeholder - should be from price service
            var traditionalValue = await _context.AccountBalances
                .AsNoTracking()
                .Where(ab => ab.BalanceType == "AVAILABLE" || ab.BalanceType == "CURRENT")
                .GroupBy(ab => ab.AccountId)
                .Select(g => g.OrderByDescending(ab => ab.LastUpdated).First())
                .SumAsync(ab => ab.Currency == "USD" ? ab.Amount : ab.Amount * brlToUsdRate, cancellationToken);

            var totalAum = cryptoValue + traditionalValue;

            // Calculate percentages
            var cryptoPercentage = totalAum > 0 ? (cryptoValue / totalAum) * 100 : 0;
            var traditionalPercentage = totalAum > 0 ? (traditionalValue / totalAum) * 100 : 0;

            // Get counts
            var totalWallets = await _context.CustodyWallets
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var totalAccounts = await _context.TraditionalAccounts
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var totalClients = await _context.Clients
                .AsNoTracking()
                .Where(c => c.Status == "Active")
                .CountAsync(cancellationToken);

            // Get top crypto assets
            var topCryptoAssets = await _context.WalletBalances
                .AsNoTracking()
                .Where(wb => wb.BalanceUsd.HasValue && wb.BalanceUsd.Value > 0)
                .GroupBy(wb => new { wb.TokenSymbol, wb.TokenName })
                .Select(g => new TopAssetDto
                {
                    AssetType = "Crypto",
                    Symbol = g.Key.TokenSymbol,
                    Name = g.Key.TokenName,
                    ValueUsd = g.Sum(wb => wb.BalanceUsd!.Value),
                    Percentage = 0 // Will calculate after
                })
                .OrderByDescending(a => a.ValueUsd)
                .Take(5)
                .ToListAsync(cancellationToken);

            // Get top traditional accounts (grouped by currency)
            var topTraditionalAssets = await _context.AccountBalances
                .AsNoTracking()
                .Where(ab => ab.BalanceType == "AVAILABLE" || ab.BalanceType == "CURRENT")
                .GroupBy(ab => new { ab.AccountId, ab.Currency })
                .Select(g => new
                {
                    AccountId = g.Key.AccountId,
                    Currency = g.Key.Currency,
                    Amount = g.OrderByDescending(ab => ab.LastUpdated).First().Amount
                })
                .GroupBy(a => a.Currency)
                .Select(g => new TopAssetDto
                {
                    AssetType = "Traditional",
                    Symbol = g.Key,
                    Name = g.Key + " Accounts",
                    ValueUsd = g.Sum(a => a.Currency == "USD" ? a.Amount : a.Amount * brlToUsdRate),
                    Percentage = 0
                })
                .OrderByDescending(a => a.ValueUsd)
                .Take(5)
                .ToListAsync(cancellationToken);

            // Combine and calculate percentages
            var allTopAssets = topCryptoAssets.Concat(topTraditionalAssets)
                .OrderByDescending(a => a.ValueUsd)
                .Take(10)
                .Select(a => a with { Percentage = totalAum > 0 ? (a.ValueUsd / totalAum) * 100 : 0 })
                .ToList();

            // Get performance summary from latest metrics
            PerformanceSummaryDto? performanceSummary = null;
            var latestMetrics = await _context.PerformanceMetrics
                .AsNoTracking()
                .OrderByDescending(pm => pm.CalculationDate)
                .Take(30) // Last 30 days
                .ToListAsync(cancellationToken);

            if (latestMetrics.Any())
            {
                var latest = latestMetrics.First();
                var dayAgo = latestMetrics.FirstOrDefault(m => m.CalculationDate <= DateTime.UtcNow.AddDays(-1));
                var weekAgo = latestMetrics.FirstOrDefault(m => m.CalculationDate <= DateTime.UtcNow.AddDays(-7));
                var monthAgo = latestMetrics.FirstOrDefault(m => m.CalculationDate <= DateTime.UtcNow.AddDays(-30));

                performanceSummary = new PerformanceSummaryDto
                {
                    TotalRoiPercentage = latest.Roi ?? 0,
                    TotalProfitLossUsd = latest.ProfitLoss ?? 0,
                    DayChangePercentage = dayAgo != null && dayAgo.TotalValueUsd > 0
                        ? ((latest.TotalValueUsd - dayAgo.TotalValueUsd) / dayAgo.TotalValueUsd) * 100
                        : 0,
                    WeekChangePercentage = weekAgo != null && weekAgo.TotalValueUsd > 0
                        ? ((latest.TotalValueUsd - weekAgo.TotalValueUsd) / weekAgo.TotalValueUsd) * 100
                        : 0,
                    MonthChangePercentage = monthAgo != null && monthAgo.TotalValueUsd > 0
                        ? ((latest.TotalValueUsd - monthAgo.TotalValueUsd) / monthAgo.TotalValueUsd) * 100
                        : 0
                };
            }

            var overview = new PortfolioOverviewDto
            {
                TotalAumUsd = totalAum,
                CryptoValueUsd = cryptoValue,
                TraditionalValueUsd = traditionalValue,
                CryptoPercentage = cryptoPercentage,
                TraditionalPercentage = traditionalPercentage,
                TotalWallets = totalWallets,
                TotalAccounts = totalAccounts,
                TotalClients = totalClients,
                TopAssets = allTopAssets,
                PerformanceSummary = performanceSummary,
                LastUpdated = DateTime.UtcNow
            };

            return Result<PortfolioOverviewDto>.Success(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating portfolio overview");
            return Result<PortfolioOverviewDto>.Failure("An error occurred while calculating portfolio overview");
        }
    }
}
