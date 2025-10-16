using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Portfolio.GetComposition;

public class GetPortfolioCompositionHandler : IRequestHandler<GetPortfolioCompositionQuery, Result<PortfolioCompositionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetPortfolioCompositionHandler> _logger;
    private const decimal BrlToUsdRate = 0.20m; // Placeholder

    public GetPortfolioCompositionHandler(ApplicationDbContext context, ILogger<GetPortfolioCompositionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PortfolioCompositionDto>> Handle(
        GetPortfolioCompositionQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get crypto assets by chain
            var cryptoByChain = await _context.WalletBalances
                .AsNoTracking()
                .Where(wb => wb.BalanceUsd.HasValue && wb.BalanceUsd.Value > 0)
                .GroupBy(wb => wb.Chain)
                .Select(g => new
                {
                    Chain = g.Key,
                    ValueUsd = g.Sum(wb => wb.BalanceUsd!.Value),
                    WalletCount = g.Select(wb => wb.WalletId).Distinct().Count()
                })
                .ToListAsync(cancellationToken);

            var totalCryptoValue = cryptoByChain.Sum(c => c.ValueUsd);

            // Get traditional assets by currency
            var traditionalByCurrency = await _context.AccountBalances
                .AsNoTracking()
                .Where(ab => ab.BalanceType == "AVAILABLE" || ab.BalanceType == "CURRENT")
                .GroupBy(ab => new { ab.AccountId, ab.Currency })
                .Select(g => new
                {
                    AccountId = g.Key.AccountId,
                    Currency = g.Key.Currency,
                    Amount = g.OrderByDescending(ab => ab.LastUpdated).First().Amount
                })
                .ToListAsync(cancellationToken);

            var traditionalGrouped = traditionalByCurrency
                .GroupBy(t => t.Currency)
                .Select(g => new
                {
                    Currency = g.Key,
                    ValueUsd = g.Sum(t => t.Currency == "USD" ? t.Amount : t.Amount * BrlToUsdRate),
                    AccountCount = g.Count()
                })
                .ToList();

            var totalTraditionalValue = traditionalGrouped.Sum(t => t.ValueUsd);
            var totalValue = totalCryptoValue + totalTraditionalValue;

            // Asset class breakdown
            var assetClasses = new List<AssetClassBreakdownDto>
            {
                new()
                {
                    AssetClass = "Crypto",
                    ValueUsd = totalCryptoValue,
                    Percentage = totalValue > 0 ? (totalCryptoValue / totalValue) * 100 : 0,
                    AssetCount = cryptoByChain.Sum(c => c.WalletCount)
                },
                new()
                {
                    AssetClass = "Traditional",
                    ValueUsd = totalTraditionalValue,
                    Percentage = totalValue > 0 ? (totalTraditionalValue / totalValue) * 100 : 0,
                    AssetCount = traditionalGrouped.Sum(t => t.AccountCount)
                }
            };

            // Chain breakdown
            var chainBreakdown = cryptoByChain
                .Select(c => new ChainBreakdownDto
                {
                    Chain = c.Chain,
                    ValueUsd = c.ValueUsd,
                    Percentage = totalValue > 0 ? (c.ValueUsd / totalValue) * 100 : 0,
                    WalletCount = c.WalletCount
                })
                .OrderByDescending(c => c.ValueUsd)
                .ToList();

            // Currency breakdown
            var currencyBreakdown = traditionalGrouped
                .Select(t => new CurrencyBreakdownDto
                {
                    Currency = t.Currency,
                    ValueUsd = t.ValueUsd,
                    Percentage = totalValue > 0 ? (t.ValueUsd / totalValue) * 100 : 0,
                    AccountCount = t.AccountCount
                })
                .OrderByDescending(c => c.ValueUsd)
                .ToList();

            // Calculate concentration metrics
            var allAssets = cryptoByChain.Select(c => c.ValueUsd)
                .Concat(traditionalGrouped.Select(t => t.ValueUsd))
                .OrderByDescending(v => v)
                .ToList();

            var topAsset = allAssets.FirstOrDefault();
            var top5Assets = allAssets.Take(5).Sum();
            var top10Assets = allAssets.Take(10).Sum();

            // Herfindahl Index: sum of squared market shares
            var herfindahlIndex = totalValue > 0
                ? allAssets.Sum(v => Math.Pow((double)(v / totalValue), 2))
                : 0;

            var concentrationMetrics = new ConcentrationMetricsDto
            {
                TopAssetPercentage = totalValue > 0 ? (topAsset / totalValue) * 100 : 0,
                Top5AssetsPercentage = totalValue > 0 ? (top5Assets / totalValue) * 100 : 0,
                Top10AssetsPercentage = totalValue > 0 ? (top10Assets / totalValue) * 100 : 0,
                TotalAssets = allAssets.Count,
                HerfindahlIndex = (decimal)herfindahlIndex
            };

            var composition = new PortfolioCompositionDto
            {
                TotalValueUsd = totalValue,
                AssetClasses = assetClasses,
                ChainBreakdown = chainBreakdown,
                CurrencyBreakdown = currencyBreakdown,
                ConcentrationMetrics = concentrationMetrics,
                LastUpdated = DateTime.UtcNow
            };

            return Result<PortfolioCompositionDto>.Success(composition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating portfolio composition");
            return Result<PortfolioCompositionDto>.Failure("An error occurred while calculating portfolio composition");
        }
    }
}
