using ApiService.Common.Database;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ApiService.BackgroundJobs;

public class ExportCleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExportCleanupJob> _logger;

    public ExportCleanupJob(ApplicationDbContext context, ILogger<ExportCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting export cleanup job");

        var now = DateTime.UtcNow;

        // Find expired export jobs
        var expiredJobs = await _context.ExportJobs
            .Where(j => j.ExpiresAt != null && j.ExpiresAt < now)
            .ToListAsync();

        _logger.LogInformation("Found {Count} expired export jobs", expiredJobs.Count);

        int deletedFiles = 0;
        int deletedRecords = 0;

        foreach (var job in expiredJobs)
        {
            try
            {
                // Delete physical file
                if (!string.IsNullOrEmpty(job.FilePath) && File.Exists(job.FilePath))
                {
                    File.Delete(job.FilePath);
                    deletedFiles++;
                    _logger.LogDebug("Deleted export file: {FilePath}", job.FilePath);
                }

                // Delete database record
                _context.ExportJobs.Remove(job);
                deletedRecords++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete export job {JobId}", job.Id);
            }
        }

        // Also delete old failed/completed jobs (older than 7 days)
        var sevenDaysAgo = now.AddDays(-7);
        var oldJobs = await _context.ExportJobs
            .Where(j => (j.Status == "Failed" || j.Status == "Completed") && j.CreatedAt < sevenDaysAgo)
            .ToListAsync();

        _logger.LogInformation("Found {Count} old export jobs (>7 days)", oldJobs.Count);

        foreach (var job in oldJobs)
        {
            try
            {
                if (!string.IsNullOrEmpty(job.FilePath) && File.Exists(job.FilePath))
                {
                    File.Delete(job.FilePath);
                    deletedFiles++;
                }

                _context.ExportJobs.Remove(job);
                deletedRecords++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete old export job {JobId}", job.Id);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Export cleanup completed: {DeletedFiles} files deleted, {DeletedRecords} records removed",
            deletedFiles, deletedRecords);
    }
}
