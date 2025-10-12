using System.Text.Json;

namespace ApiService.Common.Database.Entities;

public class RebalancingAlert
{
    public Guid Id { get; set; }
    public Guid? ClientId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public JsonDocument? AlertData { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }

    // Navigation properties
    public Client? Client { get; set; }
}
