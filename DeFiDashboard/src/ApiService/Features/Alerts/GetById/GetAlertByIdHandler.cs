using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Alerts.GetById;

public class GetAlertByIdHandler : IRequestHandler<GetAlertByIdQuery, Result<AlertDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAlertByIdHandler> _logger;

    public GetAlertByIdHandler(ApplicationDbContext context, ILogger<GetAlertByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AlertDto>> Handle(
        GetAlertByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _context.RebalancingAlerts
                .AsNoTracking()
                .Where(a => a.Id == request.Id)
                .Select(a => new AlertDto
                {
                    Id = a.Id,
                    ClientId = a.ClientId,
                    ClientName = a.Client != null ? a.Client.Name : null,
                    AlertType = a.AlertType,
                    Severity = a.Severity,
                    Status = a.Status,
                    Message = a.Message,
                    AlertData = a.AlertData != null ? a.AlertData.RootElement.GetRawText() : null,
                    CreatedAt = a.CreatedAt,
                    AcknowledgedAt = a.AcknowledgedAt,
                    ResolvedAt = a.ResolvedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (alert == null)
            {
                return Result<AlertDto>.Failure("Alert not found");
            }

            return Result<AlertDto>.Success(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert {AlertId}", request.Id);
            return Result<AlertDto>.Failure("An error occurred while retrieving the alert");
        }
    }
}
