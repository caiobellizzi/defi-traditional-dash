namespace ApiService.Common.Database.Entities;

public class ExportJob
{
    public Guid Id { get; set; }
    public string JobType { get; set; } = string.Empty; // PDF, Excel
    public string ExportType { get; set; } = string.Empty; // Portfolio, Transactions, Performance, Allocations
    public string Status { get; set; } = "Queued"; // Queued, Processing, Completed, Failed
    public string? Parameters { get; set; } // JSON serialized parameters
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // Auto-delete after 24 hours
}
