using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Analytics.GetPerformance;

public record GetPerformanceQuery(
    Guid? ClientId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string Granularity = "daily" // daily, weekly, monthly
) : IRequest<Result<PerformanceDto>>;

public record PerformanceDto
{
    public Guid? ClientId { get; init; }
    public string? ClientName { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public PerformanceSummary Summary { get; init; } = null!;
    public IEnumerable<PerformanceDataPoint> TimeSeries { get; init; } = [];
    public PerformanceBreakdown Breakdown { get; init; } = null!;
}

public record PerformanceSummary
{
    public decimal StartValueUsd { get; init; }
    public decimal EndValueUsd { get; init; }
    public decimal AbsoluteReturnUsd { get; init; }
    public decimal RoiPercentage { get; init; }
    public decimal AnnualizedReturn { get; init; }
    public decimal BestDayPercentage { get; init; }
    public decimal WorstDayPercentage { get; init; }
    public int PositiveDays { get; init; }
    public int NegativeDays { get; init; }
}

public record PerformanceDataPoint
{
    public DateTime Date { get; init; }
    public decimal ValueUsd { get; init; }
    public decimal? ReturnPercentage { get; init; }
    public decimal? CryptoValue { get; init; }
    public decimal? TraditionalValue { get; init; }
}

public record PerformanceBreakdown
{
    public decimal CryptoReturnPercentage { get; init; }
    public decimal TraditionalReturnPercentage { get; init; }
    public decimal CryptoContribution { get; init; }
    public decimal TraditionalContribution { get; init; }
}
