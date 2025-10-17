using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.CreateConnectToken;

public class CreateConnectTokenHandler : IRequestHandler<CreateConnectTokenCommand, Result<ConnectTokenDto>>
{
    private readonly ILogger<CreateConnectTokenHandler> _logger;

    public CreateConnectTokenHandler(ILogger<CreateConnectTokenHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<ConnectTokenDto>> Handle(
        CreateConnectTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Integrate with actual Pluggy SDK
            // This is a placeholder that returns mock data
            // In real implementation, this would:
            // 1. Call IOpenFinanceProvider.CreateConnectTokenAsync()
            // 2. Return the actual Pluggy connect token and URL

            _logger.LogInformation("Connect token requested. Placeholder implementation - would call Pluggy API in production.");

            await Task.Delay(100, cancellationToken); // Simulate API call

            var mockToken = new ConnectTokenDto
            {
                AccessToken = $"mock_token_{Guid.NewGuid():N}",
                ConnectUrl = "https://pluggy.ai/connect/mock-url",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            return Result<ConnectTokenDto>.Success(mockToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating connect token");
            return Result<ConnectTokenDto>.Failure("An error occurred while creating the connect token");
        }
    }
}
