using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.GetById;

public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDetailDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAccountByIdHandler> _logger;

    public GetAccountByIdHandler(ApplicationDbContext context, ILogger<GetAccountByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AccountDetailDto>> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _context.TraditionalAccounts
                .AsNoTracking()
                .Where(a => a.Id == request.Id)
                .Select(a => new AccountDetailDto
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
                    SyncErrorMessage = a.SyncErrorMessage,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    TotalBalances = a.Balances.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (account == null)
            {
                return Result<AccountDetailDto>.Failure("Account not found");
            }

            return Result<AccountDetailDto>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {AccountId}", request.Id);
            return Result<AccountDetailDto>.Failure("An error occurred while retrieving the account");
        }
    }
}
