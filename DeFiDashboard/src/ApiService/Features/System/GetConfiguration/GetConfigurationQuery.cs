using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.GetConfiguration;

public record GetConfigurationQuery : IRequest<Result<SystemConfigurationDto>>;

public record SystemConfigurationDto
{
    public Dictionary<string, ConfigItemDto> Settings { get; init; } = new();
}

public record ConfigItemDto
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime UpdatedAt { get; init; }
}
