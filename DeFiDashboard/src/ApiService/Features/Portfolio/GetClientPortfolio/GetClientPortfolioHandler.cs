using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Portfolio.GetClientPortfolio;

public class GetClientPortfolioHandler : IRequestHandler<GetClientPortfolioQuery, Result<ClientPortfolioDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetClientPortfolioHandler> _logger;

    public GetClientPortfolioHandler(ApplicationDbContext context, ILogger<GetClientPortfolioHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ClientPortfolioDto>> Handle(
        GetClientPortfolioQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

            if (client == null)
            {
                return Result<ClientPortfolioDto>.Failure("Client not found");
            }

            // Get active allocations
            var allocations = await _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.ClientId == request.ClientId && a.EndDate == null)
                .ToListAsync(cancellationToken);

            var portfolioAssets = new List<PortfolioAssetDto>();
            decimal totalCryptoValue = 0;
            decimal totalTraditionalValue = 0;

            foreach (var allocation in allocations)
            {
                if (allocation.AssetType == "Wallet")
                {
                    var wallet = await _context.CustodyWallets
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.Id == allocation.AssetId, cancellationToken);

                    if (wallet != null)
                    {
                        var balances = await _context.WalletBalances
                            .AsNoTracking()
                            .Where(b => b.WalletId == wallet.Id)
                            .ToListAsync(cancellationToken);

                        var totalAssetValue = balances.Sum(b => b.BalanceUsd ?? 0);
                        var clientValue = CalculateClientValue(totalAssetValue, allocation.AllocationType, allocation.AllocationValue);

                        portfolioAssets.Add(new PortfolioAssetDto
                        {
                            AssetType = "Wallet",
                            AssetId = wallet.Id,
                            AssetIdentifier = wallet.WalletAddress,
                            AllocationType = allocation.AllocationType,
                            AllocationValue = allocation.AllocationValue,
                            TotalAssetValueUsd = totalAssetValue,
                            ClientAllocatedValueUsd = clientValue,
                            Tokens = balances.Select(b => new TokenBalanceDto
                            {
                                Chain = b.Chain,
                                TokenSymbol = b.TokenSymbol,
                                Balance = b.Balance,
                                BalanceUsd = b.BalanceUsd
                            }).ToList()
                        });

                        totalCryptoValue += clientValue;
                    }
                }
                else if (allocation.AssetType == "Account")
                {
                    var account = await _context.TraditionalAccounts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == allocation.AssetId, cancellationToken);

                    if (account != null)
                    {
                        var balances = await _context.AccountBalances
                            .AsNoTracking()
                            .Where(b => b.AccountId == account.Id)
                            .ToListAsync(cancellationToken);

                        var totalAssetValue = balances.Sum(b => b.Amount);
                        var clientValue = CalculateClientValue(totalAssetValue, allocation.AllocationType, allocation.AllocationValue);

                        portfolioAssets.Add(new PortfolioAssetDto
                        {
                            AssetType = "Account",
                            AssetId = account.Id,
                            AssetIdentifier = account.AccountNumber ?? account.InstitutionName ?? "Unknown",
                            AllocationType = allocation.AllocationType,
                            AllocationValue = allocation.AllocationValue,
                            TotalAssetValueUsd = totalAssetValue,
                            ClientAllocatedValueUsd = clientValue,
                            Tokens = new List<TokenBalanceDto>()
                        });

                        totalTraditionalValue += clientValue;
                    }
                }
            }

            var portfolio = new ClientPortfolioDto
            {
                ClientId = client.Id,
                ClientName = client.Name,
                TotalValueUsd = totalCryptoValue + totalTraditionalValue,
                CryptoValueUsd = totalCryptoValue,
                TraditionalValueUsd = totalTraditionalValue,
                Assets = portfolioAssets,
                CalculatedAt = DateTime.UtcNow
            };

            return Result<ClientPortfolioDto>.Success(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating portfolio for client {ClientId}", request.ClientId);
            return Result<ClientPortfolioDto>.Failure("An error occurred while calculating the portfolio");
        }
    }

    private static decimal CalculateClientValue(decimal totalAssetValue, string allocationType, decimal allocationValue)
    {
        return allocationType == "Percentage"
            ? totalAssetValue * (allocationValue / 100)
            : allocationValue;
    }
}
