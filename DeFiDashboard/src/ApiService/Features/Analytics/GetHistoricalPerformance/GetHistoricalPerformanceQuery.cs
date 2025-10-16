using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Analytics.GetHistoricalPerformance;

public record GetHistoricalPerformanceQuery(
    Guid? ClientId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<HistoricalPerformanceDto>>;

public record HistoricalPerformanceDto
{
    public Guid? ClientId { get; init; }
    public string? ClientName { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public IEnumerable<HistoricalDataPoint> DataPoints { get; init; } = [];
    public HistoricalSummary Summary { get; init; } = null!;
}

public record HistoricalDataPoint
{
    public DateTime Date { get; init; }
    public decimal TotalValueUsd { get; init; }
    public decimal? CryptoValueUsd { get; init; }
    public decimal? TraditionalValueUsd { get; init; }
    public decimal? Roi { get; init; }
    public decimal? DailyChange { get; init; }
    public decimal? CumulativeReturn { get; init; }
}

public record HistoricalSummary
{
    public decimal StartValue { get; init; }
    public decimal EndValue { get; init; }
    public decimal PeakValue { get; init; }
    public DateTime PeakDate { get; init; }
    public decimal TroughValue { get; init; }
    public DateTime TroughDate { get; init; }
    public decimal TotalReturn { get; init; }
    public int TotalDays { get; init; }
}
