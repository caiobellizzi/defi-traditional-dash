using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Analytics.GetCorrelation;

public record GetCorrelationQuery(
    int PeriodDays = 30
) : IRequest<Result<CorrelationDto>>;

public record CorrelationDto
{
    public int PeriodDays { get; init; }
    public IEnumerable<AssetCorrelation> Correlations { get; init; } = [];
    public CorrelationSummary Summary { get; init; } = null!;
    public DateTime CalculatedAt { get; init; }
}

public record AssetCorrelation
{
    public string Asset1Type { get; init; } = string.Empty;
    public string Asset1Symbol { get; init; } = string.Empty;
    public string Asset2Type { get; init; } = string.Empty;
    public string Asset2Symbol { get; init; } = string.Empty;
    public decimal CorrelationCoefficient { get; init; } // -1 to 1
    public string CorrelationStrength { get; init; } = string.Empty; // Weak, Moderate, Strong
    public string CorrelationDirection { get; init; } = string.Empty; // Positive, Negative, None
}

public record CorrelationSummary
{
    public decimal AverageCorrelation { get; init; }
    public decimal HighestCorrelation { get; init; }
    public decimal LowestCorrelation { get; init; }
    public int TotalPairs { get; init; }
    public decimal CryptoTraditionalCorrelation { get; init; } // Crypto vs Traditional correlation
    public string DiversificationScore { get; init; } = string.Empty; // Excellent, Good, Fair, Poor
}
