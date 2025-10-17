using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.System.GetConfiguration;

public class GetConfigurationHandler : IRequestHandler<GetConfigurationQuery, Result<SystemConfigurationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetConfigurationHandler> _logger;

    public GetConfigurationHandler(ApplicationDbContext context, ILogger<GetConfigurationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SystemConfigurationDto>> Handle(
        GetConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var configs = await _context.SystemConfigurations
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var configDict = configs.ToDictionary(
                c => c.Key,
                c => new ConfigItemDto
                {
                    Key = c.Key,
                    Value = c.Value,
                    Description = c.Description,
                    UpdatedAt = c.UpdatedAt
                });

            var result = new SystemConfigurationDto
            {
                Settings = configDict
            };

            return Result<SystemConfigurationDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system configuration");
            return Result<SystemConfigurationDto>.Failure("An error occurred while retrieving system configuration");
        }
    }
}
