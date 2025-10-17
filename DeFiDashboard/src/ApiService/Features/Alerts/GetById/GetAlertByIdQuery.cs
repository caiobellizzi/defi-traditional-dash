using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Alerts.GetById;

public record GetAlertByIdQuery(Guid Id) : IRequest<Result<AlertDto>>;
