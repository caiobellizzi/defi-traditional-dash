using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Analytics.GetRiskMetrics;

public class GetRiskMetricsHandler : IRequestHandler<GetRiskMetricsQuery, Result<RiskMetricsDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetRiskMetricsHandler> _logger;
    private const decimal RiskFreeRate = 0.04m; // 4% annual risk-free rate (placeholder)

    public GetRiskMetricsHandler(ApplicationDbContext context, ILogger<GetRiskMetricsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<RiskMetricsDto>> Handle(
        GetRiskMetricsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-request.PeriodDays);

            // Get performance metrics
            var query = _context.PerformanceMetrics
                .AsNoTracking()
                .Where(pm => pm.CalculationDate >= fromDate);

            if (request.ClientId.HasValue)
            {
                query = query.Where(pm => pm.ClientId == request.ClientId.Value);
            }

            var metrics = await query
                .Include(pm => pm.Client)
                .OrderBy(pm => pm.CalculationDate)
                .ToListAsync(cancellationToken);

            if (metrics.Count < 2)
            {
                return Result<RiskMetricsDto>.Failure("Insufficient data to calculate risk metrics");
            }

            // Calculate daily returns
            var dailyReturns = new List<decimal>();
            for (int i = 1; i < metrics.Count; i++)
            {
                var prevValue = metrics[i - 1].TotalValueUsd;
                var currValue = metrics[i].TotalValueUsd;
                if (prevValue > 0)
                {
                    var dailyReturn = ((currValue - prevValue) / prevValue);
                    dailyReturns.Add(dailyReturn);
                }
            }

            if (!dailyReturns.Any())
            {
                return Result<RiskMetricsDto>.Failure("Unable to calculate returns from available data");
            }

            // Calculate volatility
            var avgReturn = dailyReturns.Average();
            var variance = dailyReturns.Sum(r => (r - avgReturn) * (r - avgReturn)) / dailyReturns.Count;
            var stdDev = (decimal)Math.Sqrt((double)variance);
            var dailyVolatility = stdDev * 100; // Convert to percentage
            var annualizedVolatility = dailyVolatility * (decimal)Math.Sqrt(252); // 252 trading days

            var volatilityLevel = annualizedVolatility switch
            {
                < 10 => "Low",
                < 25 => "Medium",
                _ => "High"
            };

            var volatilityMetrics = new VolatilityMetrics
            {
                DailyVolatilityPercentage = dailyVolatility,
                AnnualizedVolatilityPercentage = annualizedVolatility,
                StandardDeviation = stdDev,
                VolatilityLevel = volatilityLevel
            };

            // Calculate risk-adjusted returns (placeholder calculations)
            var avgAnnualReturn = avgReturn * 252; // Annualized
            var excessReturn = avgAnnualReturn - (RiskFreeRate / 252); // Daily excess return

            var sharpeRatio = stdDev != 0 ? (excessReturn / stdDev) * (decimal)Math.Sqrt(252) : 0;

            // Sortino ratio (only considers downside volatility)
            var downsideReturns = dailyReturns.Where(r => r < 0).ToList();
            var downsideStdDev = downsideReturns.Any()
                ? (decimal)Math.Sqrt((double)(downsideReturns.Sum(r => r * r) / downsideReturns.Count))
                : stdDev;
            var sortinoRatio = downsideStdDev != 0 ? (excessReturn / downsideStdDev) * (decimal)Math.Sqrt(252) : 0;

            // Calculate drawdown
            var runningMax = metrics[0].TotalValueUsd;
            var maxDrawdown = 0m;
            var maxDrawdownDate = metrics[0].CalculationDate;
            var currentValue = metrics.Last().TotalValueUsd;

            foreach (var metric in metrics)
            {
                if (metric.TotalValueUsd > runningMax)
                {
                    runningMax = metric.TotalValueUsd;
                }

                var drawdown = runningMax > 0 ? ((runningMax - metric.TotalValueUsd) / runningMax) * 100 : 0;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                    maxDrawdownDate = metric.CalculationDate;
                }
            }

            var currentDrawdown = runningMax > 0 ? ((runningMax - currentValue) / runningMax) * 100 : 0;
            var daysInDrawdown = currentDrawdown > 0 ? (DateTime.UtcNow - maxDrawdownDate).Days : 0;

            // Calmar ratio (return / max drawdown)
            var calmarRatio = maxDrawdown > 0 ? (avgAnnualReturn * 100) / maxDrawdown : 0;

            var riskAdjustedReturns = new RiskAdjustedReturns
            {
                SharpeRatio = sharpeRatio,
                SortinoRatio = sortinoRatio,
                CalmarRatio = calmarRatio
            };

            var drawdownMetrics = new DrawdownMetrics
            {
                MaxDrawdownPercentage = maxDrawdown,
                CurrentDrawdownPercentage = currentDrawdown,
                MaxDrawdownDate = maxDrawdownDate,
                DaysInDrawdown = daysInDrawdown
            };

            // Overall risk rating
            var riskRating = (volatilityLevel, maxDrawdown) switch
            {
                ("Low", < 10) => "Low",
                ("High", > 25) => "High",
                _ => "Medium"
            };

            // Get client name if specified
            string? clientName = null;
            if (request.ClientId.HasValue)
            {
                clientName = metrics.First().Client.Name;
            }

            var result = new RiskMetricsDto
            {
                ClientId = request.ClientId,
                ClientName = clientName,
                PeriodDays = request.PeriodDays,
                Volatility = volatilityMetrics,
                RiskAdjustedReturns = riskAdjustedReturns,
                Drawdown = drawdownMetrics,
                RiskRating = riskRating,
                CalculatedAt = DateTime.UtcNow
            };

            return Result<RiskMetricsDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating risk metrics for client {ClientId}", request.ClientId);
            return Result<RiskMetricsDto>.Failure("An error occurred while calculating risk metrics");
        }
    }
}
