using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Alerts.Acknowledge;

public record AcknowledgeAlertCommand(Guid Id) : IRequest<Result<bool>>;
