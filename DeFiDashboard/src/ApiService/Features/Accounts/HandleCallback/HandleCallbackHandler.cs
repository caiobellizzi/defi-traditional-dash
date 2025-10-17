using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Accounts.HandleCallback;

public class HandleCallbackHandler : IRequestHandler<HandleCallbackCommand, Result<CallbackResultDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HandleCallbackHandler> _logger;

    public HandleCallbackHandler(ApplicationDbContext context, ILogger<HandleCallbackHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<CallbackResultDto>> Handle(
        HandleCallbackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Integrate with actual Pluggy SDK
            // This is a placeholder that simulates callback handling
            // In real implementation, this would:
            // 1. Validate the callback from Pluggy
            // 2. Fetch account details using IOpenFinanceProvider.GetAccountsAsync(itemId)
            // 3. Create TraditionalAccount records
            // 4. Trigger initial sync for balances and transactions

            _logger.LogInformation("Pluggy callback received for ItemId: {ItemId}, Status: {Status}. Placeholder implementation.",
                request.ItemId, request.ExecutionStatus);

            // Simulate account creation
            var account = new TraditionalAccount
            {
                Id = Guid.NewGuid(),
                PluggyItemId = request.ItemId,
                PluggyAccountId = $"mock_account_{Guid.NewGuid():N}",
                AccountType = "CHECKING",
                InstitutionName = "Mock Bank",
                AccountNumber = "****1234",
                Label = "Mock Account",
                OpenFinanceProvider = "Pluggy",
                Status = "Active",
                SyncStatus = "Success",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TraditionalAccounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            var result = new CallbackResultDto
            {
                Success = true,
                Message = "Account connected successfully (placeholder)",
                AccountId = account.Id
            };

            return Result<CallbackResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Pluggy callback for ItemId {ItemId}", request.ItemId);
            return Result<CallbackResultDto>.Failure("An error occurred while processing the callback");
        }
    }
}
