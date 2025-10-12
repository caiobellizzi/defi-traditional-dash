namespace ApiService.Common.Database.Entities;

public class CustodyWallet
{
    public Guid Id { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string BlockchainProvider { get; set; } = "Moralis";
    public string[]? SupportedChains { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<WalletBalance> Balances { get; set; } = new List<WalletBalance>();
}
