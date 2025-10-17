using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;

namespace ApiService.Features.Accounts.GetList;

public record GetAccountsQuery(
    string? Status = null,
    string? InstitutionName = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AccountDto>>>;

public record AccountDto
{
    public Guid Id { get; init; }
    public string? PluggyItemId { get; init; }
    public string? PluggyAccountId { get; init; }
    public string? AccountType { get; init; }
    public string? InstitutionName { get; init; }
    public string? AccountNumber { get; init; }
    public string? Label { get; init; }
    public string OpenFinanceProvider { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? LastSyncAt { get; init; }
    public string? SyncStatus { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
