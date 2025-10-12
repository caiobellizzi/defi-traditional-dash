using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;

namespace ApiService.Features.Wallets.GetList;

public record GetWalletsQuery(
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<WalletDto>>>;

public record WalletDto
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
}
