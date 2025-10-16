using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.Utilities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.Add;

public class AddWalletHandler : IRequestHandler<AddWalletCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AddWalletHandler> _logger;

    public AddWalletHandler(ApplicationDbContext context, ILogger<AddWalletHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddWalletCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if wallet address already exists
            var exists = await _context.CustodyWallets
                .AnyAsync(w => w.WalletAddress == request.WalletAddress, cancellationToken);

            if (exists)
            {
                return Result<Guid>.Failure("A wallet with this address already exists");
            }

            var wallet = new CustodyWallet
            {
                Id = Guid.NewGuid(),
                WalletAddress = request.WalletAddress,
                Label = InputSanitizer.Sanitize(request.Label),
                BlockchainProvider = "Moralis",
                SupportedChains = request.SupportedChains,
                Status = "Active",
                Notes = InputSanitizer.Sanitize(request.Notes),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CustodyWallets.Add(wallet);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added custody wallet {WalletId} with address {Address}",
                wallet.Id, wallet.WalletAddress);

            return Result<Guid>.Success(wallet.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding wallet with address {Address}", request.WalletAddress);
            return Result<Guid>.Failure("An error occurred while adding the wallet");
        }
    }
}
