using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using ApiService.Features.Export.ExportPortfolioPdf;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Export.GetExportJob;

public class GetExportJobHandler : IRequestHandler<GetExportJobQuery, Result<ExportJobDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetExportJobHandler> _logger;

    public GetExportJobHandler(ApplicationDbContext context, ILogger<GetExportJobHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ExportJobDto>> Handle(
        GetExportJobQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exportJob = await _context.ExportJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

            if (exportJob == null)
            {
                return Result<ExportJobDto>.Failure("Export job not found");
            }

            var job = new ExportJobDto
            {
                JobId = exportJob.Id,
                Status = exportJob.Status.ToLowerInvariant(),
                CreatedAt = exportJob.CreatedAt,
                CompletedAt = exportJob.CompletedAt,
                FileUrl = exportJob.Status == "Completed"
                    ? $"/api/export/jobs/{exportJob.Id}/download"
                    : null
            };

            return Result<ExportJobDto>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving export job {JobId}", request.JobId);
            return Result<ExportJobDto>.Failure("An error occurred while retrieving the export job");
        }
    }
}
