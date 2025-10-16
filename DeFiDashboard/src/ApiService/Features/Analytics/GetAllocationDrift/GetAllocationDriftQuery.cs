using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Analytics.GetAllocationDrift;

public record GetAllocationDriftQuery(
    decimal Threshold = 5.0m // Percentage threshold for drift alert
) : IRequest<Result<AllocationDriftDto>>;

public record AllocationDriftDto
{
    public IEnumerable<AllocationDriftDetail> Drifts { get; init; } = [];
    public int TotalAllocations { get; init; }
    public int DriftsOverThreshold { get; init; }
    public decimal AverageDriftPercentage { get; init; }
    public DateTime CalculatedAt { get; init; }
}

public record AllocationDriftDetail
{
    public Guid AllocationId { get; init; }
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string AssetType { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string AssetIdentifier { get; init; } = string.Empty;
    public string AllocationType { get; init; } = string.Empty;
    public decimal TargetValue { get; init; }
    public decimal CurrentValue { get; init; }
    public decimal TargetPercentage { get; init; }
    public decimal CurrentPercentage { get; init; }
    public decimal DriftPercentage { get; init; }
    public decimal DriftAmountUsd { get; init; }
    public string Severity { get; init; } = string.Empty; // Low, Medium, High
    public string? RecommendedAction { get; init; }
}
