using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.GetById;

public class GetWalletByIdHandler : IRequestHandler<GetWalletByIdQuery, Result<WalletDetailDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetWalletByIdHandler> _logger;

    public GetWalletByIdHandler(ApplicationDbContext context, ILogger<GetWalletByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<WalletDetailDto>> Handle(
        GetWalletByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _context.CustodyWallets
                .AsNoTracking()
                .Where(w => w.Id == request.Id)
                .Select(w => new WalletDetailDto
                {
                    Id = w.Id,
                    WalletAddress = w.WalletAddress,
                    Label = w.Label,
                    BlockchainProvider = w.BlockchainProvider,
                    SupportedChains = w.SupportedChains,
                    Status = w.Status,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    TotalBalances = w.Balances.Count,
                    TotalValueUsd = w.Balances.Sum(b => b.BalanceUsd ?? 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (wallet == null)
            {
                return Result<WalletDetailDto>.Failure("Wallet not found");
            }

            return Result<WalletDetailDto>.Success(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet {WalletId}", request.Id);
            return Result<WalletDetailDto>.Failure("An error occurred while retrieving the wallet");
        }
    }
}
