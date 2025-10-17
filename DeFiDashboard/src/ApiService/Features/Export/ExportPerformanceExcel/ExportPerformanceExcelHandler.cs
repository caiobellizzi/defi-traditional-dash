using System.Text.Json;
using ApiService.BackgroundJobs;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Alerts.GetList;
using ApiService.Features.Export.ExportPortfolioPdf;
using Hangfire;
using MediatR;

namespace ApiService.Features.Export.ExportPerformanceExcel;

public class ExportPerformanceExcelHandler : IRequestHandler<ExportPerformanceExcelCommand, Result<ExportJobDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ExportPerformanceExcelHandler> _logger;

    public ExportPerformanceExcelHandler(
        ApplicationDbContext context,
        IBackgroundJobClient backgroundJobClient,
        ILogger<ExportPerformanceExcelHandler> logger)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task<Result<ExportJobDto>> Handle(
        ExportPerformanceExcelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exportJob = new ExportJob
            {
                Id = Guid.NewGuid(),
                JobType = "Excel",
                ExportType = "PerformanceExcel",
                Status = "Queued",
                Parameters = JsonSerializer.Serialize(new ExportProcessingJob.PerformanceExcelParameters(
                    request.ClientId,
                    request.FromDate,
                    request.ToDate)),
                CreatedAt = DateTime.UtcNow
            };

            _context.ExportJobs.Add(exportJob);
            await _context.SaveChangesAsync(cancellationToken);

            _backgroundJobClient.Enqueue<ExportProcessingJob>(
                job => job.ProcessExportAsync(exportJob.Id));

            _logger.LogInformation(
                "Performance Excel export job created: {JobId} for ClientId: {ClientId}",
                exportJob.Id,
                request.ClientId);

            return Result<ExportJobDto>.Success(new ExportJobDto
            {
                JobId = exportJob.Id,
                Status = exportJob.Status.ToLowerInvariant(),
                CreatedAt = exportJob.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating performance Excel export job");
            return Result<ExportJobDto>.Failure("An error occurred while creating the export job");
        }
    }
}
