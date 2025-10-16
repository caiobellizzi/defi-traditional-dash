using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Alerts.GetSummary;

public record GetAlertSummaryQuery : IRequest<Result<AlertSummaryDto>>;

public record AlertSummaryDto
{
    public int TotalAlerts { get; init; }
    public int NewAlerts { get; init; }
    public int AcknowledgedAlerts { get; init; }
    public int ResolvedAlerts { get; init; }
    public int CriticalAlerts { get; init; }
    public int HighAlerts { get; init; }
    public int MediumAlerts { get; init; }
    public int LowAlerts { get; init; }
    public Dictionary<string, int> AlertsByType { get; init; } = new();
}
