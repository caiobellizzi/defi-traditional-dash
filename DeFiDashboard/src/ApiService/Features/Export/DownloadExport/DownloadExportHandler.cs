using ApiService.Common.Database;
using ApiService.Features.Alerts.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Export.DownloadExport;

public class DownloadExportHandler : IRequestHandler<DownloadExportQuery, Result<ExportFileDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DownloadExportHandler> _logger;

    public DownloadExportHandler(ApplicationDbContext context, ILogger<DownloadExportHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ExportFileDto>> Handle(
        DownloadExportQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exportJob = await _context.ExportJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

            if (exportJob == null)
            {
                return Result<ExportFileDto>.Failure("Export job not found");
            }

            if (exportJob.Status != "Completed")
            {
                return Result<ExportFileDto>.Failure($"Export job is not completed. Current status: {exportJob.Status}");
            }

            if (string.IsNullOrEmpty(exportJob.FilePath) || !File.Exists(exportJob.FilePath))
            {
                return Result<ExportFileDto>.Failure("Export file not found or has been deleted");
            }

            var fileContent = await File.ReadAllBytesAsync(exportJob.FilePath, cancellationToken);

            // Determine content type based on file extension
            var contentType = exportJob.JobType == "PDF"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var file = new ExportFileDto
            {
                FileContent = fileContent,
                FileName = exportJob.FileName ?? $"export_{exportJob.Id}.{(exportJob.JobType == "PDF" ? "pdf" : "xlsx")}",
                ContentType = contentType
            };

            _logger.LogInformation("Export file downloaded for job {JobId}, file: {FileName}",
                request.JobId, file.FileName);

            return Result<ExportFileDto>.Success(file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading export file for job {JobId}", request.JobId);
            return Result<ExportFileDto>.Failure("An error occurred while downloading the export file");
        }
    }
}
