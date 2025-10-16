using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Analytics.GetHistoricalPerformance;

public class GetHistoricalPerformanceHandler : IRequestHandler<GetHistoricalPerformanceQuery, Result<HistoricalPerformanceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetHistoricalPerformanceHandler> _logger;

    public GetHistoricalPerformanceHandler(ApplicationDbContext context, ILogger<GetHistoricalPerformanceHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<HistoricalPerformanceDto>> Handle(
        GetHistoricalPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-3);
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
                return Result<HistoricalPerformanceDto>.Failure("No historical data available for the specified period");
            }

            // Build data points
            var dataPoints = new List<HistoricalDataPoint>();
            decimal? previousValue = null;
            var startValue = metrics.First().TotalValueUsd;

            foreach (var metric in metrics)
            {
                decimal? dailyChange = null;
                if (previousValue.HasValue && previousValue.Value > 0)
                {
                    dailyChange = ((metric.TotalValueUsd - previousValue.Value) / previousValue.Value) * 100;
                }

                var cumulativeReturn = startValue > 0
                    ? ((metric.TotalValueUsd - startValue) / startValue) * 100
                    : 0;

                dataPoints.Add(new HistoricalDataPoint
                {
                    Date = metric.CalculationDate,
                    TotalValueUsd = metric.TotalValueUsd,
                    CryptoValueUsd = metric.CryptoValueUsd,
                    TraditionalValueUsd = metric.TraditionalValueUsd,
                    Roi = metric.Roi,
                    DailyChange = dailyChange,
                    CumulativeReturn = cumulativeReturn
                });

                previousValue = metric.TotalValueUsd;
            }

            // Calculate summary
            var endValue = metrics.Last().TotalValueUsd;
            var peakMetric = metrics.OrderByDescending(m => m.TotalValueUsd).First();
            var troughMetric = metrics.OrderBy(m => m.TotalValueUsd).First();
            var totalReturn = startValue > 0 ? ((endValue - startValue) / startValue) * 100 : 0;
            var totalDays = (toDate - fromDate).Days;

            var summary = new HistoricalSummary
            {
                StartValue = startValue,
                EndValue = endValue,
                PeakValue = peakMetric.TotalValueUsd,
                PeakDate = peakMetric.CalculationDate,
                TroughValue = troughMetric.TotalValueUsd,
                TroughDate = troughMetric.CalculationDate,
                TotalReturn = totalReturn,
                TotalDays = totalDays
            };

            // Get client name if specified
            string? clientName = null;
            if (request.ClientId.HasValue)
            {
                clientName = metrics.First().Client.Name;
            }

            var result = new HistoricalPerformanceDto
            {
                ClientId = request.ClientId,
                ClientName = clientName,
                FromDate = fromDate,
                ToDate = toDate,
                DataPoints = dataPoints,
                Summary = summary
            };

            return Result<HistoricalPerformanceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical performance for client {ClientId}", request.ClientId);
            return Result<HistoricalPerformanceDto>.Failure("An error occurred while getting historical performance");
        }
    }
}
