using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Alerts.Acknowledge;

public class AcknowledgeAlertHandler : IRequestHandler<AcknowledgeAlertCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AcknowledgeAlertHandler> _logger;

    public AcknowledgeAlertHandler(ApplicationDbContext context, ILogger<AcknowledgeAlertHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        AcknowledgeAlertCommand request,
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

            if (alert.Status == "Acknowledged" || alert.Status == "Resolved")
            {
                return Result<bool>.Failure("Alert has already been acknowledged or resolved");
            }

            alert.Status = "Acknowledged";
            alert.AcknowledgedAt = DateTime.UtcNow;
            // In a real system, you would set AcknowledgedBy to the current user ID
            // alert.AcknowledgedBy = currentUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Alert {AlertId} acknowledged", alert.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", request.Id);
            return Result<bool>.Failure("An error occurred while acknowledging the alert");
        }
    }
}
