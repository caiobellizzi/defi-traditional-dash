using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.GetById;

public record GetAccountByIdQuery(Guid Id) : IRequest<Result<AccountDetailDto>>;

public record AccountDetailDto
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
    public string? SyncErrorMessage { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int TotalBalances { get; init; }
}
