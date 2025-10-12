namespace ApiService.Common.Database.Entities;

public class TraditionalAccount
{
    public Guid Id { get; set; }
    public string? PluggyItemId { get; set; }
    public string? PluggyAccountId { get; set; }
    public string? AccountType { get; set; }
    public string? InstitutionName { get; set; }
    public string? AccountNumber { get; set; }
    public string? Label { get; set; }
    public string OpenFinanceProvider { get; set; } = "Pluggy";
    public string Status { get; set; } = "Active";
    public DateTime? LastSyncAt { get; set; }
    public string? SyncStatus { get; set; }
    public string? SyncErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<AccountBalance> Balances { get; set; } = new List<AccountBalance>();
}
