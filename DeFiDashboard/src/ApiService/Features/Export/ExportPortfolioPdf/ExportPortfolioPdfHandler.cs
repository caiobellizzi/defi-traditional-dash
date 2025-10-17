using System.Text.Json;
using ApiService.BackgroundJobs;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Alerts.GetList;
using Hangfire;
using MediatR;

namespace ApiService.Features.Export.ExportPortfolioPdf;

public class ExportPortfolioPdfHandler : IRequestHandler<ExportPortfolioPdfCommand, Result<ExportJobDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ExportPortfolioPdfHandler> _logger;

    public ExportPortfolioPdfHandler(
        ApplicationDbContext context,
        IBackgroundJobClient backgroundJobClient,
        ILogger<ExportPortfolioPdfHandler> logger)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task<Result<ExportJobDto>> Handle(
        ExportPortfolioPdfCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create export job record
            var exportJob = new ExportJob
            {
                Id = Guid.NewGuid(),
                JobType = "PDF",
                ExportType = "PortfolioPdf",
                Status = "Queued",
                Parameters = JsonSerializer.Serialize(new ExportProcessingJob.PortfolioPdfParameters(
                    request.ClientId,
                    request.IncludeTransactions)),
                CreatedAt = DateTime.UtcNow
            };

            _context.ExportJobs.Add(exportJob);
            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue background job
            _backgroundJobClient.Enqueue<ExportProcessingJob>(
                job => job.ProcessExportAsync(exportJob.Id));

            _logger.LogInformation(
                "Portfolio PDF export job created: {JobId} for ClientId: {ClientId}",
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
            _logger.LogError(ex, "Error creating portfolio PDF export job");
            return Result<ExportJobDto>.Failure("An error occurred while creating the export job");
        }
    }
}
