using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.GetBalances;

public class GetAccountBalancesHandler : IRequestHandler<GetAccountBalancesQuery, Result<IEnumerable<AccountBalanceDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAccountBalancesHandler> _logger;

    public GetAccountBalancesHandler(ApplicationDbContext context, ILogger<GetAccountBalancesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<AccountBalanceDto>>> Handle(
        GetAccountBalancesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountExists = await _context.TraditionalAccounts
                .AnyAsync(a => a.Id == request.AccountId, cancellationToken);

            if (!accountExists)
            {
                return Result<IEnumerable<AccountBalanceDto>>.Failure("Account not found");
            }

            var balances = await _context.AccountBalances
                .AsNoTracking()
                .Where(b => b.AccountId == request.AccountId)
                .OrderBy(b => b.BalanceType)
                .Select(b => new AccountBalanceDto
                {
                    Id = b.Id,
                    BalanceType = b.BalanceType,
                    Currency = b.Currency,
                    Amount = b.Amount,
                    LastUpdated = b.LastUpdated
                })
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<AccountBalanceDto>>.Success(balances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balances for account {AccountId}", request.AccountId);
            return Result<IEnumerable<AccountBalanceDto>>.Failure("An error occurred while retrieving account balances");
        }
    }
}
