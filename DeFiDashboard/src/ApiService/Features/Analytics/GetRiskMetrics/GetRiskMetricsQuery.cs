using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Analytics.GetRiskMetrics;

public record GetRiskMetricsQuery(
    Guid? ClientId = null,
    int PeriodDays = 30
) : IRequest<Result<RiskMetricsDto>>;

public record RiskMetricsDto
{
    public Guid? ClientId { get; init; }
    public string? ClientName { get; init; }
    public int PeriodDays { get; init; }
    public VolatilityMetrics Volatility { get; init; } = null!;
    public RiskAdjustedReturns RiskAdjustedReturns { get; init; } = null!;
    public DrawdownMetrics Drawdown { get; init; } = null!;
    public string RiskRating { get; init; } = string.Empty; // Low, Medium, High
    public DateTime CalculatedAt { get; init; }
}

public record VolatilityMetrics
{
    public decimal DailyVolatilityPercentage { get; init; }
    public decimal AnnualizedVolatilityPercentage { get; init; }
    public decimal StandardDeviation { get; init; }
    public string VolatilityLevel { get; init; } = string.Empty; // Low, Medium, High
}

public record RiskAdjustedReturns
{
    public decimal SharpeRatio { get; init; } // Placeholder calculation
    public decimal SortinoRatio { get; init; } // Placeholder calculation
    public decimal CalmarRatio { get; init; } // Placeholder calculation
}

public record DrawdownMetrics
{
    public decimal MaxDrawdownPercentage { get; init; }
    public decimal CurrentDrawdownPercentage { get; init; }
    public DateTime? MaxDrawdownDate { get; init; }
    public int DaysInDrawdown { get; init; }
}
