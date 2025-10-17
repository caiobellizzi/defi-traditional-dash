using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.GetList;

public class GetAccountsHandler : IRequestHandler<GetAccountsQuery, Result<PagedResult<AccountDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAccountsHandler> _logger;

    public GetAccountsHandler(ApplicationDbContext context, ILogger<GetAccountsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AccountDto>>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.TraditionalAccounts.AsNoTracking();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(a => a.Status == request.Status);
            }

            // Filter by institution name if provided
            if (!string.IsNullOrEmpty(request.InstitutionName))
            {
                query = query.Where(a => a.InstitutionName != null &&
                    a.InstitutionName.Contains(request.InstitutionName));
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var accounts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    PluggyItemId = a.PluggyItemId,
                    PluggyAccountId = a.PluggyAccountId,
                    AccountType = a.AccountType,
                    InstitutionName = a.InstitutionName,
                    AccountNumber = a.AccountNumber,
                    Label = a.Label,
                    OpenFinanceProvider = a.OpenFinanceProvider,
                    Status = a.Status,
                    LastSyncAt = a.LastSyncAt,
                    SyncStatus = a.SyncStatus,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AccountDto>
            {
                Items = accounts,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<AccountDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts list");
            return Result<PagedResult<AccountDto>>.Failure("An error occurred while retrieving accounts");
        }
    }
}
