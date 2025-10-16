using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Export.ExportPortfolioPdf;

public record ExportPortfolioPdfCommand(
    Guid ClientId,
    bool IncludeTransactions = false
) : IRequest<Result<ExportJobDto>>;

public record ExportJobDto
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? FileUrl { get; init; }
}
