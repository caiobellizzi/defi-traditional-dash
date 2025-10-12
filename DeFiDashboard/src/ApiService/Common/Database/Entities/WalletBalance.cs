namespace ApiService.Common.Database.Entities;

public class WalletBalance
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string Chain { get; set; } = string.Empty;
    public string? TokenAddress { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public string? TokenName { get; set; }
    public int? TokenDecimals { get; set; }
    public decimal Balance { get; set; }
    public decimal? BalanceUsd { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public CustodyWallet Wallet { get; set; } = null!;
}
