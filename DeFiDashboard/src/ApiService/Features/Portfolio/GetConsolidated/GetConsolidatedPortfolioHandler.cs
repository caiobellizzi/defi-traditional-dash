using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Portfolio.GetConsolidated;

public class GetConsolidatedPortfolioHandler : IRequestHandler<GetConsolidatedPortfolioQuery, Result<ConsolidatedPortfolioDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetConsolidatedPortfolioHandler> _logger;
    private const decimal BrlToUsdRate = 0.20m; // Placeholder

    public GetConsolidatedPortfolioHandler(ApplicationDbContext context, ILogger<GetConsolidatedPortfolioHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ConsolidatedPortfolioDto>> Handle(
        GetConsolidatedPortfolioQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assets = new List<ConsolidatedAssetDto>();

            // Get crypto assets (wallets)
            if (request.AssetType == null || request.AssetType == "Crypto")
            {
                var walletAssets = await GetWalletAssetsAsync(cancellationToken);
                assets.AddRange(walletAssets);
            }

            // Get traditional assets (accounts)
            if (request.AssetType == null || request.AssetType == "Traditional")
            {
                var accountAssets = await GetAccountAssetsAsync(cancellationToken);
                assets.AddRange(accountAssets);
            }

            // Calculate total value
            var totalValue = assets.Sum(a => a.ValueUsd);

            // Apply pagination
            var totalAssets = assets.Count;
            var paginatedAssets = assets
                .OrderByDescending(a => a.ValueUsd)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => a with { Percentage = totalValue > 0 ? (a.ValueUsd / totalValue) * 100 : 0 })
                .ToList();

            var result = new ConsolidatedPortfolioDto
            {
                TotalValueUsd = totalValue,
                Assets = paginatedAssets,
                TotalAssets = totalAssets,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                LastUpdated = DateTime.UtcNow
            };

            return Result<ConsolidatedPortfolioDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consolidated portfolio");
            return Result<ConsolidatedPortfolioDto>.Failure("An error occurred while getting consolidated portfolio");
        }
    }

    private async Task<List<ConsolidatedAssetDto>> GetWalletAssetsAsync(CancellationToken cancellationToken)
    {
        var wallets = await _context.CustodyWallets
            .AsNoTracking()
            .Include(w => w.Balances)
            .ToListAsync(cancellationToken);

        var walletAllocations = await _context.ClientAssetAllocations
            .AsNoTracking()
            .Where(a => a.AssetType == "Wallet" && a.EndDate == null)
            .Include(a => a.Client)
            .ToListAsync(cancellationToken);

        var assets = new List<ConsolidatedAssetDto>();

        foreach (var wallet in wallets)
        {
            var totalValue = wallet.Balances.Sum(b => b.BalanceUsd ?? 0);
            var allocations = walletAllocations.Where(a => a.AssetId == wallet.Id).ToList();

            var clientAllocations = allocations.Select(a =>
            {
                var allocatedValue = a.AllocationType == "Percentage"
                    ? totalValue * (a.AllocationValue / 100)
                    : Math.Min(a.AllocationValue, totalValue);

                return new ClientAllocationInfo
                {
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name,
                    AllocationType = a.AllocationType,
                    AllocationValue = a.AllocationValue,
                    AllocatedValueUsd = allocatedValue
                };
            }).ToList();

            // Get primary token symbol
            var primaryBalance = wallet.Balances.OrderByDescending(b => b.BalanceUsd ?? 0).FirstOrDefault();

            assets.Add(new ConsolidatedAssetDto
            {
                AssetId = wallet.Id,
                AssetType = "Wallet",
                Identifier = wallet.WalletAddress,
                Symbol = primaryBalance?.TokenSymbol ?? "MULTI",
                Name = wallet.Label ?? "Wallet",
                ValueUsd = totalValue,
                Percentage = 0, // Will be calculated later
                ClientAllocations = allocations.Count,
                AllocatedClients = clientAllocations,
                LastUpdated = wallet.Balances.Any() ? wallet.Balances.Max(b => b.LastUpdated) : DateTime.UtcNow
            });
        }

        return assets;
    }

    private async Task<List<ConsolidatedAssetDto>> GetAccountAssetsAsync(CancellationToken cancellationToken)
    {
        var accounts = await _context.TraditionalAccounts
            .AsNoTracking()
            .Include(a => a.Balances)
            .ToListAsync(cancellationToken);

        var accountAllocations = await _context.ClientAssetAllocations
            .AsNoTracking()
            .Where(a => a.AssetType == "Account" && a.EndDate == null)
            .Include(a => a.Client)
            .ToListAsync(cancellationToken);

        var assets = new List<ConsolidatedAssetDto>();

        foreach (var account in accounts)
        {
            // Get latest balance
            var latestBalance = account.Balances
                .Where(b => b.BalanceType == "AVAILABLE" || b.BalanceType == "CURRENT")
                .OrderByDescending(b => b.LastUpdated)
                .FirstOrDefault();

            if (latestBalance == null) continue;

            var totalValue = latestBalance.Currency == "USD"
                ? latestBalance.Amount
                : latestBalance.Amount * BrlToUsdRate;

            var allocations = accountAllocations.Where(a => a.AssetId == account.Id).ToList();

            var clientAllocations = allocations.Select(a =>
            {
                var allocatedValue = a.AllocationType == "Percentage"
                    ? totalValue * (a.AllocationValue / 100)
                    : Math.Min(a.AllocationValue, totalValue);

                return new ClientAllocationInfo
                {
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name,
                    AllocationType = a.AllocationType,
                    AllocationValue = a.AllocationValue,
                    AllocatedValueUsd = allocatedValue
                };
            }).ToList();

            assets.Add(new ConsolidatedAssetDto
            {
                AssetId = account.Id,
                AssetType = "Account",
                Identifier = account.AccountNumber ?? account.PluggyAccountId ?? account.Id.ToString(),
                Symbol = latestBalance.Currency,
                Name = account.Label ?? $"{account.AccountType} Account",
                ValueUsd = totalValue,
                Percentage = 0, // Will be calculated later
                ClientAllocations = allocations.Count,
                AllocatedClients = clientAllocations,
                LastUpdated = latestBalance.LastUpdated
            });
        }

        return assets;
    }
}
