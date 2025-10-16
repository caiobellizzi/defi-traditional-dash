using System.Text.Json;
using ApiService.Common.Database;
using ApiService.Common.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ApiService.BackgroundJobs;

public class ExportProcessingJob
{
    private readonly ApplicationDbContext _context;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;
    private readonly ILogger<ExportProcessingJob> _logger;
    private readonly string _exportBasePath;

    public ExportProcessingJob(
        ApplicationDbContext context,
        IPdfExportService pdfService,
        IExcelExportService excelService,
        ILogger<ExportProcessingJob> logger,
        IConfiguration configuration)
    {
        _context = context;
        _pdfService = pdfService;
        _excelService = excelService;
        _logger = logger;
        _exportBasePath = configuration["ExportSettings:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "exports");
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessExportAsync(Guid exportJobId)
    {
        _logger.LogInformation("Processing export job {ExportJobId}", exportJobId);

        var job = await _context.ExportJobs.FindAsync(exportJobId);
        if (job == null)
        {
            _logger.LogWarning("Export job {ExportJobId} not found", exportJobId);
            return;
        }

        if (job.Status != "Queued")
        {
            _logger.LogWarning("Export job {ExportJobId} is not in Queued status, current status: {Status}",
                exportJobId, job.Status);
            return;
        }

        try
        {
            job.Status = "Processing";
            await _context.SaveChangesAsync();

            byte[] fileData;
            string fileName;
            string fileExtension;

            // Generate export based on type
            if (job.ExportType == "PortfolioPdf")
            {
                var parameters = JsonSerializer.Deserialize<PortfolioPdfParameters>(job.Parameters!);
                if (parameters == null)
                    throw new InvalidOperationException("Invalid parameters for PortfolioPdf");

                fileData = await _pdfService.GeneratePortfolioReportAsync(
                    parameters.ClientId,
                    parameters.IncludeTransactions);
                fileExtension = "pdf";
                fileName = $"portfolio_{parameters.ClientId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
            }
            else if (job.ExportType == "PerformancePdf")
            {
                var parameters = JsonSerializer.Deserialize<PerformancePdfParameters>(job.Parameters!);
                if (parameters == null)
                    throw new InvalidOperationException("Invalid parameters for PerformancePdf");

                fileData = await _pdfService.GeneratePerformanceReportAsync(
                    parameters.ClientId,
                    parameters.FromDate,
                    parameters.ToDate);
                fileExtension = "pdf";
                fileName = $"performance_{parameters.ClientId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
            }
            else if (job.ExportType == "TransactionsExcel")
            {
                var parameters = JsonSerializer.Deserialize<TransactionsExcelParameters>(job.Parameters!);
                if (parameters == null)
                    throw new InvalidOperationException("Invalid parameters for TransactionsExcel");

                fileData = await _excelService.GenerateTransactionsExportAsync(
                    parameters.FromDate,
                    parameters.ToDate,
                    parameters.ClientId,
                    parameters.TransactionType);
                fileExtension = "xlsx";
                fileName = $"transactions_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
            }
            else if (job.ExportType == "PerformanceExcel")
            {
                var parameters = JsonSerializer.Deserialize<PerformanceExcelParameters>(job.Parameters!);
                if (parameters == null)
                    throw new InvalidOperationException("Invalid parameters for PerformanceExcel");

                fileData = await _excelService.GeneratePerformanceExportAsync(
                    parameters.ClientId,
                    parameters.FromDate,
                    parameters.ToDate);
                fileExtension = "xlsx";
                fileName = $"performance_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
            }
            else if (job.ExportType == "AllocationsExcel")
            {
                var parameters = JsonSerializer.Deserialize<AllocationsExcelParameters>(job.Parameters!);
                if (parameters == null)
                    throw new InvalidOperationException("Invalid parameters for AllocationsExcel");

                fileData = await _excelService.GenerateAllocationsExportAsync(
                    parameters.ClientId,
                    parameters.ActiveOnly);
                fileExtension = "xlsx";
                fileName = $"allocations_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
            }
            else
            {
                throw new InvalidOperationException($"Unknown export type: {job.ExportType}");
            }

            // Save file to storage
            Directory.CreateDirectory(_exportBasePath);
            var exportPath = Path.Combine(_exportBasePath, fileName);
            await File.WriteAllBytesAsync(exportPath, fileData);

            _logger.LogInformation("Export job {ExportJobId} completed successfully, file saved to {FilePath}",
                exportJobId, exportPath);

            job.Status = "Completed";
            job.FileName = fileName;
            job.FilePath = exportPath;
            job.CompletedAt = DateTime.UtcNow;
            job.ExpiresAt = DateTime.UtcNow.AddHours(24);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export job {ExportJobId} failed", exportJobId);
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
        }

        await _context.SaveChangesAsync();
    }

    // Parameter classes
    public record PortfolioPdfParameters(Guid ClientId, bool IncludeTransactions);
    public record PerformancePdfParameters(Guid ClientId, DateTime FromDate, DateTime ToDate);
    public record TransactionsExcelParameters(DateTime? FromDate, DateTime? ToDate, Guid? ClientId, string? TransactionType);
    public record PerformanceExcelParameters(Guid? ClientId, DateTime FromDate, DateTime ToDate);
    public record AllocationsExcelParameters(Guid? ClientId, bool ActiveOnly);
}
