using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Portfolio.GetConsolidated;

public record GetConsolidatedPortfolioQuery(
    string? AssetType = null, // Crypto, Traditional, or null for all
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<ConsolidatedPortfolioDto>>;

public record ConsolidatedPortfolioDto
{
    public decimal TotalValueUsd { get; init; }
    public IEnumerable<ConsolidatedAssetDto> Assets { get; init; } = [];
    public int TotalAssets { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public DateTime LastUpdated { get; init; }
}

public record ConsolidatedAssetDto
{
    public Guid AssetId { get; init; }
    public string AssetType { get; init; } = string.Empty; // Wallet or Account
    public string Identifier { get; init; } = string.Empty; // Wallet address or Account name
    public string? Symbol { get; init; }
    public string? Name { get; init; }
    public decimal ValueUsd { get; init; }
    public decimal Percentage { get; init; }
    public int ClientAllocations { get; init; }
    public IEnumerable<ClientAllocationInfo> AllocatedClients { get; init; } = [];
    public DateTime LastUpdated { get; init; }
}

public record ClientAllocationInfo
{
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string AllocationType { get; init; } = string.Empty;
    public decimal AllocationValue { get; init; }
    public decimal AllocatedValueUsd { get; init; }
}
