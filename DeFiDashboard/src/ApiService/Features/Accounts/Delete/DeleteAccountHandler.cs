using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.Delete;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(ApplicationDbContext context, ILogger<DeleteAccountHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteAccountCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _context.TraditionalAccounts
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (account == null)
            {
                return Result<bool>.Failure("Account not found");
            }

            // Check if account has active allocations
            var hasActiveAllocations = await _context.ClientAssetAllocations
                .AnyAsync(a => a.AssetType == "Account" && a.AssetId == request.Id && a.EndDate == null,
                    cancellationToken);

            if (hasActiveAllocations)
            {
                return Result<bool>.Failure("Cannot delete account with active client allocations");
            }

            // Soft delete - mark as inactive
            account.Status = "Inactive";
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted (soft) account {AccountId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {AccountId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the account");
        }
    }
}
