namespace ApiService.Common.Database.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string TransactionType { get; set; } = string.Empty; // 'Wallet' or 'Account'
    public Guid AssetId { get; set; }
    public string? TransactionHash { get; set; }
    public string? ExternalId { get; set; }
    public string? Chain { get; set; }
    public string Direction { get; set; } = string.Empty; // 'IN', 'OUT', 'INTERNAL'
    public string? FromAddress { get; set; }
    public string? ToAddress { get; set; }
    public string? TokenSymbol { get; set; }
    public decimal Amount { get; set; }
    public decimal? AmountUsd { get; set; }
    public decimal? Fee { get; set; }
    public decimal? FeeUsd { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool IsManualEntry { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<TransactionAudit> AuditTrail { get; set; } = new List<TransactionAudit>();
}
