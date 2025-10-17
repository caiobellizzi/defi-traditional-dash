using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Alerts.Resolve;

public class ResolveAlertHandler : IRequestHandler<ResolveAlertCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResolveAlertHandler> _logger;

    public ResolveAlertHandler(ApplicationDbContext context, ILogger<ResolveAlertHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ResolveAlertCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _context.RebalancingAlerts
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (alert == null)
            {
                return Result<bool>.Failure("Alert not found");
            }

            if (alert.Status == "Resolved")
            {
                return Result<bool>.Failure("Alert has already been resolved");
            }

            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.UtcNow;
            // In a real system, you would set ResolvedBy to the current user ID
            // alert.ResolvedBy = currentUserId;

            // Note: Resolution notes would need to be added to the entity
            // For now, we can store it in AlertData or add a new field

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Alert {AlertId} resolved", alert.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert {AlertId}", request.Id);
            return Result<bool>.Failure("An error occurred while resolving the alert");
        }
    }
}
