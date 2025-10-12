using System.Text.Json;

namespace ApiService.Common.Database.Entities;

public class TransactionAudit
{
    public Guid Id { get; set; }
    public Guid? TransactionId { get; set; }
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public JsonDocument? OldData { get; set; }
    public JsonDocument? NewData { get; set; }
    public string? Reason { get; set; }

    // Navigation properties
    public Transaction? Transaction { get; set; }
}
