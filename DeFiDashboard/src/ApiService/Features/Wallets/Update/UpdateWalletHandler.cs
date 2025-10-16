using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.Update;

public class UpdateWalletHandler : IRequestHandler<UpdateWalletCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateWalletHandler> _logger;

    public UpdateWalletHandler(ApplicationDbContext context, ILogger<UpdateWalletHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateWalletCommand request,
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

            wallet.Label = request.Label;
            wallet.SupportedChains = request.SupportedChains;
            wallet.Notes = request.Notes;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated wallet {WalletId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wallet {WalletId}", request.Id);
            return Result<bool>.Failure("An error occurred while updating the wallet");
        }
    }
}
