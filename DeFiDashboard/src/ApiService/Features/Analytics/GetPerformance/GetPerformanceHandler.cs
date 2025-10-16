using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Analytics.GetPerformance;

public class GetPerformanceHandler : IRequestHandler<GetPerformanceQuery, Result<PerformanceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetPerformanceHandler> _logger;

    public GetPerformanceHandler(ApplicationDbContext context, ILogger<GetPerformanceHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PerformanceDto>> Handle(
        GetPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = request.ToDate ?? DateTime.UtcNow;

            // Get performance metrics
            var query = _context.PerformanceMetrics
                .AsNoTracking()
                .Where(pm => pm.CalculationDate >= fromDate && pm.CalculationDate <= toDate);

            if (request.ClientId.HasValue)
            {
                query = query.Where(pm => pm.ClientId == request.ClientId.Value);
            }

            var metrics = await query
                .Include(pm => pm.Client)
                .OrderBy(pm => pm.CalculationDate)
                .ToListAsync(cancellationToken);

            if (!metrics.Any())
            {
                return Result<PerformanceDto>.Failure("No performance data available for the specified period");
            }

            // Group by granularity
            var groupedMetrics = GroupByGranularity(metrics, request.Granularity);

            // Calculate summary
            var startValue = groupedMetrics.First().TotalValueUsd;
            var endValue = groupedMetrics.Last().TotalValueUsd;
            var absoluteReturn = endValue - startValue;
            var roiPercentage = startValue > 0 ? (absoluteReturn / startValue) * 100 : 0;

            // Calculate annualized return
            var daysDiff = (toDate - fromDate).TotalDays;
            var annualizedReturn = daysDiff > 0 && startValue > 0
                ? (decimal)(Math.Pow((double)(endValue / startValue), 365.0 / daysDiff) - 1) * 100
                : 0;

            // Calculate daily returns
            var dailyReturns = new List<decimal>();
            for (int i = 1; i < groupedMetrics.Count; i++)
            {
                var prevValue = groupedMetrics[i - 1].TotalValueUsd;
                var currValue = groupedMetrics[i].TotalValueUsd;
                if (prevValue > 0)
                {
                    var dailyReturn = ((currValue - prevValue) / prevValue) * 100;
                    dailyReturns.Add(dailyReturn);
                }
            }

            var bestDay = dailyReturns.Any() ? dailyReturns.Max() : 0;
            var worstDay = dailyReturns.Any() ? dailyReturns.Min() : 0;
            var positiveDays = dailyReturns.Count(r => r > 0);
            var negativeDays = dailyReturns.Count(r => r < 0);

            var summary = new PerformanceSummary
            {
                StartValueUsd = startValue,
                EndValueUsd = endValue,
                AbsoluteReturnUsd = absoluteReturn,
                RoiPercentage = roiPercentage,
                AnnualizedReturn = annualizedReturn,
                BestDayPercentage = bestDay,
                WorstDayPercentage = worstDay,
                PositiveDays = positiveDays,
                NegativeDays = negativeDays
            };

            // Create time series
            var timeSeries = new List<PerformanceDataPoint>();
            for (int i = 0; i < groupedMetrics.Count; i++)
            {
                var metric = groupedMetrics[i];
                decimal? returnPercentage = null;

                if (i > 0)
                {
                    var prevValue = groupedMetrics[i - 1].TotalValueUsd;
                    if (prevValue > 0)
                    {
                        returnPercentage = ((metric.TotalValueUsd - prevValue) / prevValue) * 100;
                    }
                }

                timeSeries.Add(new PerformanceDataPoint
                {
                    Date = metric.CalculationDate,
                    ValueUsd = metric.TotalValueUsd,
                    ReturnPercentage = returnPercentage,
                    CryptoValue = metric.CryptoValueUsd,
                    TraditionalValue = metric.TraditionalValueUsd
                });
            }

            // Calculate breakdown
            var startCrypto = groupedMetrics.First().CryptoValueUsd ?? 0;
            var endCrypto = groupedMetrics.Last().CryptoValueUsd ?? 0;
            var startTraditional = groupedMetrics.First().TraditionalValueUsd ?? 0;
            var endTraditional = groupedMetrics.Last().TraditionalValueUsd ?? 0;

            var cryptoReturn = startCrypto > 0 ? ((endCrypto - startCrypto) / startCrypto) * 100 : 0;
            var traditionalReturn = startTraditional > 0 ? ((endTraditional - startTraditional) / startTraditional) * 100 : 0;

            var cryptoContribution = startValue > 0 ? ((endCrypto - startCrypto) / startValue) * 100 : 0;
            var traditionalContribution = startValue > 0 ? ((endTraditional - startTraditional) / startValue) * 100 : 0;

            var breakdown = new PerformanceBreakdown
            {
                CryptoReturnPercentage = cryptoReturn,
                TraditionalReturnPercentage = traditionalReturn,
                CryptoContribution = cryptoContribution,
                TraditionalContribution = traditionalContribution
            };

            // Get client name if specified
            string? clientName = null;
            if (request.ClientId.HasValue)
            {
                clientName = metrics.First().Client.Name;
            }

            var performance = new PerformanceDto
            {
                ClientId = request.ClientId,
                ClientName = clientName,
                FromDate = fromDate,
                ToDate = toDate,
                Summary = summary,
                TimeSeries = timeSeries,
                Breakdown = breakdown
            };

            return Result<PerformanceDto>.Success(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating performance for client {ClientId}", request.ClientId);
            return Result<PerformanceDto>.Failure("An error occurred while calculating performance");
        }
    }

    private List<Common.Database.Entities.PerformanceMetric> GroupByGranularity(
        List<Common.Database.Entities.PerformanceMetric> metrics,
        string granularity)
    {
        return granularity.ToLower() switch
        {
            "weekly" => metrics
                .GroupBy(m => new { Year = m.CalculationDate.Year, Week = GetWeekOfYear(m.CalculationDate) })
                .Select(g => g.OrderByDescending(m => m.CalculationDate).First())
                .OrderBy(m => m.CalculationDate)
                .ToList(),
            "monthly" => metrics
                .GroupBy(m => new { m.CalculationDate.Year, m.CalculationDate.Month })
                .Select(g => g.OrderByDescending(m => m.CalculationDate).First())
                .OrderBy(m => m.CalculationDate)
                .ToList(),
            _ => metrics // daily
        };
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var calendar = global::System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, global::System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
