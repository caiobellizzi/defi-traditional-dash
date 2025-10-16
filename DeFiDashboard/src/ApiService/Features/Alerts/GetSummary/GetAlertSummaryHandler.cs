using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Alerts.GetSummary;

public class GetAlertSummaryHandler : IRequestHandler<GetAlertSummaryQuery, Result<AlertSummaryDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAlertSummaryHandler> _logger;

    public GetAlertSummaryHandler(ApplicationDbContext context, ILogger<GetAlertSummaryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AlertSummaryDto>> Handle(
        GetAlertSummaryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _context.RebalancingAlerts
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var summary = new AlertSummaryDto
            {
                TotalAlerts = alerts.Count,
                NewAlerts = alerts.Count(a => a.Status == "Active" || a.Status == "New"),
                AcknowledgedAlerts = alerts.Count(a => a.Status == "Acknowledged"),
                ResolvedAlerts = alerts.Count(a => a.Status == "Resolved"),
                CriticalAlerts = alerts.Count(a => a.Severity == "Critical"),
                HighAlerts = alerts.Count(a => a.Severity == "High"),
                MediumAlerts = alerts.Count(a => a.Severity == "Medium"),
                LowAlerts = alerts.Count(a => a.Severity == "Low"),
                AlertsByType = alerts
                    .GroupBy(a => a.AlertType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Result<AlertSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert summary");
            return Result<AlertSummaryDto>.Failure("An error occurred while retrieving alert summary");
        }
    }
}
