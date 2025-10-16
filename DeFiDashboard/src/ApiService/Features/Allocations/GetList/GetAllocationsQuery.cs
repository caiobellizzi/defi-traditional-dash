using ApiService.Features.Allocations.GetByClient;
using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Allocations.GetList;

public record GetAllocationsQuery(
    Guid? ClientId = null,
    string? AssetType = null, // 'Wallet' or 'Account'
    Guid? AssetId = null,
    bool ActiveOnly = false,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AllocationDto>>>;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
