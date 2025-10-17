using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.GetById;

public record GetWalletByIdQuery(Guid Id) : IRequest<Result<WalletDetailDto>>;

public record WalletDetailDto
{
    public Guid Id { get; init; }
    public string WalletAddress { get; init; } = string.Empty;
    public string? Label { get; init; }
    public string BlockchainProvider { get; init; } = string.Empty;
    public string[]? SupportedChains { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int TotalBalances { get; init; }
    public decimal TotalValueUsd { get; init; }
}
