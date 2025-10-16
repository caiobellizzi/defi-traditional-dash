using System.Text.Json;
using ApiService.BackgroundJobs;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Alerts.GetList;
using ApiService.Features.Export.ExportPortfolioPdf;
using Hangfire;
using MediatR;

namespace ApiService.Features.Export.ExportTransactionsExcel;

public class ExportTransactionsExcelHandler : IRequestHandler<ExportTransactionsExcelCommand, Result<ExportJobDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ExportTransactionsExcelHandler> _logger;

    public ExportTransactionsExcelHandler(
        ApplicationDbContext context,
        IBackgroundJobClient backgroundJobClient,
        ILogger<ExportTransactionsExcelHandler> logger)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task<Result<ExportJobDto>> Handle(
        ExportTransactionsExcelCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exportJob = new ExportJob
            {
                Id = Guid.NewGuid(),
                JobType = "Excel",
                ExportType = "TransactionsExcel",
                Status = "Queued",
                Parameters = JsonSerializer.Serialize(new ExportProcessingJob.TransactionsExcelParameters(
                    request.FromDate,
                    request.ToDate,
                    request.ClientId,
                    request.TransactionType)),
                CreatedAt = DateTime.UtcNow
            };

            _context.ExportJobs.Add(exportJob);
            await _context.SaveChangesAsync(cancellationToken);

            _backgroundJobClient.Enqueue<ExportProcessingJob>(
                job => job.ProcessExportAsync(exportJob.Id));

            _logger.LogInformation(
                "Transactions Excel export job created: {JobId} for ClientId: {ClientId}, From: {From}, To: {To}",
                exportJob.Id,
                request.ClientId,
                request.FromDate,
                request.ToDate);

            return Result<ExportJobDto>.Success(new ExportJobDto
            {
                JobId = exportJob.Id,
                Status = exportJob.Status.ToLowerInvariant(),
                CreatedAt = exportJob.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transactions Excel export job");
            return Result<ExportJobDto>.Failure("An error occurred while creating the export job");
        }
    }
}
