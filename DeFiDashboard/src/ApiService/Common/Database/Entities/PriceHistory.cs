namespace ApiService.Common.Database.Entities;

public class PriceHistory
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public decimal PriceUsd { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Source { get; set; }
}
