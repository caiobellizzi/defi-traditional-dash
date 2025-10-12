namespace ApiService.Common.Database.Entities;

/// <summary>
/// CRITICAL TABLE: Maps client portions of custody assets
/// </summary>
public class ClientAssetAllocation
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string AssetType { get; set; } = string.Empty; // 'Wallet' or 'Account'
    public Guid AssetId { get; set; }
    public string AllocationType { get; set; } = string.Empty; // 'Percentage' or 'FixedAmount'
    public decimal AllocationValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
}
