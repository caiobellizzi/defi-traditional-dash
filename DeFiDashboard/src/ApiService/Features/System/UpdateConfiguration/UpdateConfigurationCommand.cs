using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.UpdateConfiguration;

public record UpdateConfigurationCommand(
    Dictionary<string, string> Settings
) : IRequest<Result<bool>>;
