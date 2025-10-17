using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Portfolio.GetSnapshot;

public class GetPortfolioSnapshotHandler : IRequestHandler<GetPortfolioSnapshotQuery, Result<PortfolioSnapshotDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetPortfolioSnapshotHandler> _logger;
    private const decimal BrlToUsdRate = 0.20m; // Placeholder

    public GetPortfolioSnapshotHandler(ApplicationDbContext context, ILogger<GetPortfolioSnapshotHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PortfolioSnapshotDto>> Handle(
        GetPortfolioSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var targetDate = request.Date ?? DateTime.UtcNow.Date;
            var isEstimated = false;

            // Try to get performance metrics for the date
            var performanceMetrics = await _context.PerformanceMetrics
                .AsNoTracking()
                .Where(pm => pm.CalculationDate.Date == targetDate)
                .Include(pm => pm.Client)
                .ToListAsync(cancellationToken);

            if (!performanceMetrics.Any())
            {
                // If no exact match, get closest earlier date
                performanceMetrics = await _context.PerformanceMetrics
                    .AsNoTracking()
                    .Where(pm => pm.CalculationDate.Date <= targetDate)
                    .Include(pm => pm.Client)
                    .GroupBy(pm => pm.ClientId)
                    .Select(g => g.OrderByDescending(pm => pm.CalculationDate).First())
                    .ToListAsync(cancellationToken);

                isEstimated = true;
            }

            // Calculate from current data if no historical metrics
            if (!performanceMetrics.Any())
            {
                return await GetCurrentSnapshotAsync(targetDate, cancellationToken);
            }

            var totalValue = performanceMetrics.Sum(pm => pm.TotalValueUsd);
            var cryptoValue = performanceMetrics.Sum(pm => pm.CryptoValueUsd ?? 0);
            var traditionalValue = performanceMetrics.Sum(pm => pm.TraditionalValueUsd ?? 0);

            var clientSnapshots = performanceMetrics
                .Select(pm => new ClientSnapshotDto
                {
                    ClientId = pm.ClientId,
                    ClientName = pm.Client.Name,
                    ValueUsd = pm.TotalValueUsd,
                    Percentage = totalValue > 0 ? (pm.TotalValueUsd / totalValue) * 100 : 0
                })
                .OrderByDescending(c => c.ValueUsd)
                .ToList();

            // Get asset breakdown from price history (placeholder - would need more complex logic)
            var assetSnapshots = new List<AssetSnapshotDto>
            {
                new()
                {
                    AssetType = "Crypto",
                    Symbol = "CRYPTO",
                    Name = "Cryptocurrency Assets",
                    ValueUsd = cryptoValue,
                    Percentage = totalValue > 0 ? (cryptoValue / totalValue) * 100 : 0
                },
                new()
                {
                    AssetType = "Traditional",
                    Symbol = "FIAT",
                    Name = "Traditional Finance Assets",
                    ValueUsd = traditionalValue,
                    Percentage = totalValue > 0 ? (traditionalValue / totalValue) * 100 : 0
                }
            };

            var snapshot = new PortfolioSnapshotDto
            {
                SnapshotDate = performanceMetrics.First().CalculationDate,
                TotalValueUsd = totalValue,
                CryptoValueUsd = cryptoValue,
                TraditionalValueUsd = traditionalValue,
                ClientSnapshots = clientSnapshots,
                AssetSnapshots = assetSnapshots.Where(a => a.ValueUsd > 0),
                IsEstimated = isEstimated,
                Notes = isEstimated ? "Snapshot estimated from nearest available data" : null
            };

            return Result<PortfolioSnapshotDto>.Success(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio snapshot for date {Date}", request.Date);
            return Result<PortfolioSnapshotDto>.Failure("An error occurred while getting portfolio snapshot");
        }
    }

    private async Task<Result<PortfolioSnapshotDto>> GetCurrentSnapshotAsync(
        DateTime targetDate,
        CancellationToken cancellationToken)
    {
        // Calculate from current wallet and account balances
        var cryptoValue = await _context.WalletBalances
            .AsNoTracking()
            .Where(wb => wb.BalanceUsd.HasValue)
            .SumAsync(wb => wb.BalanceUsd!.Value, cancellationToken);

        var traditionalValue = await _context.AccountBalances
            .AsNoTracking()
            .Where(ab => ab.BalanceType == "AVAILABLE" || ab.BalanceType == "CURRENT")
            .GroupBy(ab => ab.AccountId)
            .Select(g => g.OrderByDescending(ab => ab.LastUpdated).First())
            .SumAsync(ab => ab.Currency == "USD" ? ab.Amount : ab.Amount * BrlToUsdRate, cancellationToken);

        var totalValue = cryptoValue + traditionalValue;

        // Get client allocations (placeholder - simplified calculation)
        var clients = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Status == "Active")
            .ToListAsync(cancellationToken);

        var clientSnapshots = clients.Select(c => new ClientSnapshotDto
        {
            ClientId = c.Id,
            ClientName = c.Name,
            ValueUsd = 0, // Would need to calculate from allocations
            Percentage = 0
        }).ToList();

        var assetSnapshots = new List<AssetSnapshotDto>
        {
            new()
            {
                AssetType = "Crypto",
                Symbol = "CRYPTO",
                Name = "Cryptocurrency Assets",
                ValueUsd = cryptoValue,
                Percentage = totalValue > 0 ? (cryptoValue / totalValue) * 100 : 0
            },
            new()
            {
                AssetType = "Traditional",
                Symbol = "FIAT",
                Name = "Traditional Finance Assets",
                ValueUsd = traditionalValue,
                Percentage = totalValue > 0 ? (traditionalValue / totalValue) * 100 : 0
            }
        };

        var snapshot = new PortfolioSnapshotDto
        {
            SnapshotDate = targetDate,
            TotalValueUsd = totalValue,
            CryptoValueUsd = cryptoValue,
            TraditionalValueUsd = traditionalValue,
            ClientSnapshots = clientSnapshots,
            AssetSnapshots = assetSnapshots.Where(a => a.ValueUsd > 0),
            IsEstimated = true,
            Notes = "Snapshot calculated from current data - no historical metrics available"
        };

        return Result<PortfolioSnapshotDto>.Success(snapshot);
    }
}
