using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Portfolio.Recalculate;

public record RecalculatePortfolioCommand : IRequest<Result<RecalculatePortfolioResultDto>>;

public record RecalculatePortfolioResultDto
{
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = "Queued";
    public string Message { get; init; } = string.Empty;
    public DateTime QueuedAt { get; init; }
}
