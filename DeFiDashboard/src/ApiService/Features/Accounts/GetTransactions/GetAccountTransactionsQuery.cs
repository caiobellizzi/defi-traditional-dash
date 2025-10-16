using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;

namespace ApiService.Features.Accounts.GetTransactions;

public record GetAccountTransactionsQuery(
    Guid AccountId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Category = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AccountTransactionDto>>>;

public record AccountTransactionDto
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public string Direction { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public DateTime TransactionDate { get; init; }
    public string Status { get; init; } = string.Empty;
}
