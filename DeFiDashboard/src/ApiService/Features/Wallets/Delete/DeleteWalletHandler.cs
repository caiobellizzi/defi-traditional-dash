using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.Delete;

public class DeleteWalletHandler : IRequestHandler<DeleteWalletCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteWalletHandler> _logger;

    public DeleteWalletHandler(ApplicationDbContext context, ILogger<DeleteWalletHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteWalletCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _context.CustodyWallets
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (wallet == null)
            {
                return Result<bool>.Failure("Wallet not found");
            }

            // Check if wallet has active allocations
            var hasActiveAllocations = await _context.ClientAssetAllocations
                .AnyAsync(a => a.AssetType == "Wallet" && a.AssetId == request.Id && a.EndDate == null,
                    cancellationToken);

            if (hasActiveAllocations)
            {
                return Result<bool>.Failure("Cannot delete wallet with active client allocations");
            }

            // Soft delete - mark as inactive
            wallet.Status = "Inactive";
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted (soft) wallet {WalletId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting wallet {WalletId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the wallet");
        }
    }
}
