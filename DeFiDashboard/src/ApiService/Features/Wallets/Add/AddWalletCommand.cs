using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.Add;

public record AddWalletCommand(
    string WalletAddress,
    string? Label,
    string[]? SupportedChains,
    string? Notes
) : IRequest<Result<Guid>>;
