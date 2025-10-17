using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Wallets.Update;

public record UpdateWalletCommand(
    Guid Id,
    string? Label,
    string[]? SupportedChains,
    string? Notes
) : IRequest<Result<bool>>;
