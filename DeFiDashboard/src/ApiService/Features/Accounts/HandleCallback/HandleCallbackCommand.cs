using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.HandleCallback;

public record HandleCallbackCommand(
    string ItemId,
    string? ExecutionStatus
) : IRequest<Result<CallbackResultDto>>;

public record CallbackResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? AccountId { get; init; }
}
