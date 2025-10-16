using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Portfolio.GetOverview;

public record GetPortfolioOverviewQuery : IRequest<Result<PortfolioOverviewDto>>;

public record PortfolioOverviewDto
{
    public decimal TotalAumUsd { get; init; }
    public decimal CryptoValueUsd { get; init; }
    public decimal TraditionalValueUsd { get; init; }
    public decimal CryptoPercentage { get; init; }
    public decimal TraditionalPercentage { get; init; }
    public int TotalWallets { get; init; }
    public int TotalAccounts { get; init; }
    public int TotalClients { get; init; }
    public IEnumerable<TopAssetDto> TopAssets { get; init; } = [];
    public PerformanceSummaryDto? PerformanceSummary { get; init; }
    public DateTime LastUpdated { get; init; }
}

public record TopAssetDto
{
    public string AssetType { get; init; } = string.Empty; // Crypto or Traditional
    public string Symbol { get; init; } = string.Empty;
    public string? Name { get; init; }
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
}

public record PerformanceSummaryDto
{
    public decimal TotalRoiPercentage { get; init; }
    public decimal TotalProfitLossUsd { get; init; }
    public decimal DayChangePercentage { get; init; }
    public decimal WeekChangePercentage { get; init; }
    public decimal MonthChangePercentage { get; init; }
}
