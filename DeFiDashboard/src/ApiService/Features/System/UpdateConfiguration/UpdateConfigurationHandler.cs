using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.System.UpdateConfiguration;

public class UpdateConfigurationHandler : IRequestHandler<UpdateConfigurationCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateConfigurationHandler> _logger;

    public UpdateConfigurationHandler(ApplicationDbContext context, ILogger<UpdateConfigurationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var (key, value) in request.Settings)
            {
                var config = await _context.SystemConfigurations
                    .FirstOrDefaultAsync(c => c.Key == key, cancellationToken);

                if (config != null)
                {
                    config.Value = value;
                    config.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new configuration entry
                    config = new SystemConfiguration
                    {
                        Key = key,
                        Value = value,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.SystemConfigurations.Add(config);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("System configuration updated: {Keys}", string.Join(", ", request.Settings.Keys));

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system configuration");
            return Result<bool>.Failure("An error occurred while updating system configuration");
        }
    }
}
