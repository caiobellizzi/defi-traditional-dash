namespace ApiService.Common.Database.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Document { get; set; }
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<ClientAssetAllocation> AssetAllocations { get; set; } = new List<ClientAssetAllocation>();
    public ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();
    public ICollection<RebalancingAlert> RebalancingAlerts { get; set; } = new List<RebalancingAlert>();
}
