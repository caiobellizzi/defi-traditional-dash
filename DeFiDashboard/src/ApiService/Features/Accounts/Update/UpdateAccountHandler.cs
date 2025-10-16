using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Accounts.Update;

public class UpdateAccountHandler : IRequestHandler<UpdateAccountCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateAccountHandler> _logger;

    public UpdateAccountHandler(ApplicationDbContext context, ILogger<UpdateAccountHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateAccountCommand request,
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

            account.Label = request.Label;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated account {AccountId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {AccountId}", request.Id);
            return Result<bool>.Failure("An error occurred while updating the account");
        }
    }
}
