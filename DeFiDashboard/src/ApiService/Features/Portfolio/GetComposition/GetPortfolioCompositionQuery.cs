using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Portfolio.GetComposition;

public record GetPortfolioCompositionQuery : IRequest<Result<PortfolioCompositionDto>>;

public record PortfolioCompositionDto
{
    public decimal TotalValueUsd { get; init; }
    public IEnumerable<AssetClassBreakdownDto> AssetClasses { get; init; } = [];
    public IEnumerable<ChainBreakdownDto> ChainBreakdown { get; init; } = [];
    public IEnumerable<CurrencyBreakdownDto> CurrencyBreakdown { get; init; } = [];
    public ConcentrationMetricsDto ConcentrationMetrics { get; init; } = null!;
    public DateTime LastUpdated { get; init; }
}

public record AssetClassBreakdownDto
{
    public string AssetClass { get; init; } = string.Empty; // Crypto, Traditional, etc.
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
    public int AssetCount { get; init; }
}

public record ChainBreakdownDto
{
    public string Chain { get; init; } = string.Empty;
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
    public int WalletCount { get; init; }
}

public record CurrencyBreakdownDto
{
    public string Currency { get; init; } = string.Empty;
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
    public int AccountCount { get; init; }
}

public record ConcentrationMetricsDto
{
    public decimal TopAssetPercentage { get; init; }
    public decimal Top5AssetsPercentage { get; init; }
    public decimal Top10AssetsPercentage { get; init; }
    public int TotalAssets { get; init; }
    public decimal HerfindahlIndex { get; init; } // Concentration index (0-1, higher = more concentrated)
}
