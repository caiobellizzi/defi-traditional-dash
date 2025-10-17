using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.GetTransactions;

public class GetAccountTransactionsHandler : IRequestHandler<GetAccountTransactionsQuery, Result<PagedResult<AccountTransactionDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAccountTransactionsHandler> _logger;

    public GetAccountTransactionsHandler(ApplicationDbContext context, ILogger<GetAccountTransactionsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AccountTransactionDto>>> Handle(
        GetAccountTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountExists = await _context.TraditionalAccounts
                .AnyAsync(a => a.Id == request.AccountId, cancellationToken);

            if (!accountExists)
            {
                return Result<PagedResult<AccountTransactionDto>>.Failure("Account not found");
            }

            var query = _context.Transactions
                .AsNoTracking()
                .Where(t => t.TransactionType == "Account" && t.AssetId == request.AccountId);

            // Apply filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= request.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(t => t.Category == request.Category);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new AccountTransactionDto
                {
                    Id = t.Id,
                    ExternalId = t.ExternalId,
                    Direction = t.Direction,
                    Amount = t.Amount,
                    Description = t.Description,
                    Category = t.Category,
                    TransactionDate = t.TransactionDate,
                    Status = t.Status
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AccountTransactionDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<AccountTransactionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account {AccountId}", request.AccountId);
            return Result<PagedResult<AccountTransactionDto>>.Failure("An error occurred while retrieving account transactions");
        }
    }
}
