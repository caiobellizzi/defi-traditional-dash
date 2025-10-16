using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.GetAllocations;

public record GetAccountAllocationsQuery(Guid AccountId) : IRequest<Result<IEnumerable<AllocationDto>>>;

public record AllocationDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string AllocationType { get; init; } = string.Empty;
    public decimal AllocationValue { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
}
