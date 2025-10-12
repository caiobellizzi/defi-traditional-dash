using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Portfolio.GetClientPortfolio;

public record GetClientPortfolioQuery(Guid ClientId) : IRequest<Result<ClientPortfolioDto>>;

public record ClientPortfolioDto
{
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public decimal TotalValueUsd { get; init; }
    public decimal CryptoValueUsd { get; init; }
    public decimal TraditionalValueUsd { get; init; }
    public List<PortfolioAssetDto> Assets { get; init; } = new();
    public DateTime CalculatedAt { get; init; }
}

public record PortfolioAssetDto
{
    public string AssetType { get; init; } = string.Empty; // "Wallet" or "Account"
    public Guid AssetId { get; init; }
    public string AssetIdentifier { get; init; } = string.Empty;
    public string AllocationType { get; init; } = string.Empty;
    public decimal AllocationValue { get; init; }
    public decimal TotalAssetValueUsd { get; init; }
    public decimal ClientAllocatedValueUsd { get; init; }
    public List<TokenBalanceDto> Tokens { get; init; } = new();
}

public record TokenBalanceDto
{
    public string Chain { get; init; } = string.Empty;
    public string TokenSymbol { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal? BalanceUsd { get; init; }
}
