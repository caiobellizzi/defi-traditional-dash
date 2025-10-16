using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Portfolio.GetSnapshot;

public record GetPortfolioSnapshotQuery(
    DateTime? Date = null // If null, use latest available
) : IRequest<Result<PortfolioSnapshotDto>>;

public record PortfolioSnapshotDto
{
    public DateTime SnapshotDate { get; init; }
    public decimal TotalValueUsd { get; init; }
    public decimal CryptoValueUsd { get; init; }
    public decimal TraditionalValueUsd { get; init; }
    public IEnumerable<ClientSnapshotDto> ClientSnapshots { get; init; } = [];
    public IEnumerable<AssetSnapshotDto> AssetSnapshots { get; init; } = [];
    public bool IsEstimated { get; init; }
    public string? Notes { get; init; }
}

public record ClientSnapshotDto
{
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
}

public record AssetSnapshotDto
{
    public string AssetType { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public string? Name { get; init; }
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
}
