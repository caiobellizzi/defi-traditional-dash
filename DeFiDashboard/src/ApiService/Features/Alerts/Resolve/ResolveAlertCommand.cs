using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Alerts.Resolve;

public record ResolveAlertCommand(
    Guid Id,
    string? Resolution
) : IRequest<Result<bool>>;
