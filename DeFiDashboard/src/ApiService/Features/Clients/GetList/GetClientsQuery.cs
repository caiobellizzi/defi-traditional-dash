using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Clients.GetList;

public record GetClientsQuery(
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<ClientDto>>>;

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record ClientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Document { get; init; }
    public string? PhoneNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
