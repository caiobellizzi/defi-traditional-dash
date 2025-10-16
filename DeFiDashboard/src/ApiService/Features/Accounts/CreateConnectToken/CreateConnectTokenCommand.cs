using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.CreateConnectToken;

public record CreateConnectTokenCommand : IRequest<Result<ConnectTokenDto>>;

public record ConnectTokenDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string ConnectUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
