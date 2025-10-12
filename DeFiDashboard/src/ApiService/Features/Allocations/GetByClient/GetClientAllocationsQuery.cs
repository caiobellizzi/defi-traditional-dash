using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.GetByClient;

public record GetClientAllocationsQuery(
    Guid ClientId,
    bool ActiveOnly = true
) : IRequest<Result<IEnumerable<AllocationDto>>>;

public record AllocationDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string AssetType { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string AssetIdentifier { get; init; } = string.Empty; // Wallet address or Account number
    public string AllocationType { get; init; } = string.Empty;
    public decimal AllocationValue { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
