using System.Text.Json;

namespace ApiService.Common.Database.Entities;

public class PerformanceMetric
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public DateTime CalculationDate { get; set; }
    public decimal TotalValueUsd { get; set; }
    public decimal? CryptoValueUsd { get; set; }
    public decimal? TraditionalValueUsd { get; set; }
    public decimal? Roi { get; set; }
    public decimal? ProfitLoss { get; set; }
    public JsonDocument? MetricsData { get; set; }
    public DateTime CalculatedAt { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
}
