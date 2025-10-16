using ApiService.Common.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Alerts.GetList;

public class GetAlertsHandler : IRequestHandler<GetAlertsQuery, Result<PagedResult<AlertDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAlertsHandler> _logger;

    public GetAlertsHandler(ApplicationDbContext context, ILogger<GetAlertsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AlertDto>>> Handle(
        GetAlertsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.RebalancingAlerts.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Severity))
            {
                query = query.Where(a => a.Severity == request.Severity);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(a => a.Status == request.Status);
            }

            if (!string.IsNullOrEmpty(request.AlertType))
            {
                query = query.Where(a => a.AlertType == request.AlertType);
            }

            if (request.ClientId.HasValue)
            {
                query = query.Where(a => a.ClientId == request.ClientId.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and include client name
            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AlertDto>
            {
                Items = alerts,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<AlertDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts list");
            return Result<PagedResult<AlertDto>>.Failure("An error occurred while retrieving alerts");
        }
    }
}
