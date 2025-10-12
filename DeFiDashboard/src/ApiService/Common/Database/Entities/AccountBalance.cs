namespace ApiService.Common.Database.Entities;

public class AccountBalance
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string BalanceType { get; set; } = string.Empty; // AVAILABLE, CURRENT, LIMIT
    public string Currency { get; set; } = "BRL";
    public decimal Amount { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public TraditionalAccount Account { get; set; } = null!;
}
