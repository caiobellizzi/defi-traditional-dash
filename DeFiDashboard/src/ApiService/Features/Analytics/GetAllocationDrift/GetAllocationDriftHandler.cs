using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Analytics.GetAllocationDrift;

public class GetAllocationDriftHandler : IRequestHandler<GetAllocationDriftQuery, Result<AllocationDriftDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllocationDriftHandler> _logger;
    private const decimal BrlToUsdRate = 0.20m; // Placeholder

    public GetAllocationDriftHandler(ApplicationDbContext context, ILogger<GetAllocationDriftHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AllocationDriftDto>> Handle(
        GetAllocationDriftQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all active allocations
            var allocations = await _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.EndDate == null)
                .Include(a => a.Client)
                .ToListAsync(cancellationToken);

            if (!allocations.Any())
            {
                return Result<AllocationDriftDto>.Success(new AllocationDriftDto
                {
                    Drifts = [],
                    TotalAllocations = 0,
                    DriftsOverThreshold = 0,
                    AverageDriftPercentage = 0,
                    CalculatedAt = DateTime.UtcNow
                });
            }

            // Get current asset values
            var walletValues = await GetWalletValuesAsync(cancellationToken);
            var accountValues = await GetAccountValuesAsync(cancellationToken);

            var driftDetails = new List<AllocationDriftDetail>();

            foreach (var allocation in allocations)
            {
                var currentAssetValue = allocation.AssetType == "Wallet"
                    ? walletValues.GetValueOrDefault(allocation.AssetId, 0)
                    : accountValues.GetValueOrDefault(allocation.AssetId, 0);

                if (currentAssetValue == 0)
                {
                    _logger.LogWarning("Asset {AssetId} has zero value, skipping drift calculation", allocation.AssetId);
                    continue;
                }

                // Calculate target and current values
                decimal targetValue, currentValue, targetPercentage, currentPercentage;

                if (allocation.AllocationType == "Percentage")
                {
                    // For percentage allocations
                    targetPercentage = allocation.AllocationValue;
                    targetValue = currentAssetValue * (allocation.AllocationValue / 100);

                    // Get client's current portfolio value to calculate actual percentage
                    var clientTotalValue = await GetClientTotalValueAsync(allocation.ClientId, cancellationToken);
                    currentValue = targetValue; // Simplified - would need more complex calculation
                    currentPercentage = clientTotalValue > 0 ? (currentValue / clientTotalValue) * 100 : 0;
                }
                else
                {
                    // For fixed amount allocations
                    targetValue = allocation.AllocationValue;
                    currentValue = Math.Min(allocation.AllocationValue, currentAssetValue);
                    targetPercentage = currentAssetValue > 0 ? (targetValue / currentAssetValue) * 100 : 0;
                    currentPercentage = currentAssetValue > 0 ? (currentValue / currentAssetValue) * 100 : 0;
                }

                // Calculate drift
                var driftPercentage = Math.Abs(currentPercentage - targetPercentage);
                var driftAmountUsd = Math.Abs(currentValue - targetValue);

                // Determine severity
                var severity = driftPercentage switch
                {
                    < 5 => "Low",
                    < 10 => "Medium",
                    _ => "High"
                };

                // Recommended action
                string? recommendedAction = null;
                if (driftPercentage > request.Threshold)
                {
                    recommendedAction = currentPercentage > targetPercentage
                        ? $"Consider reducing allocation by {driftAmountUsd:F2} USD"
                        : $"Consider increasing allocation by {driftAmountUsd:F2} USD";
                }

                // Get asset identifier
                var assetIdentifier = await GetAssetIdentifierAsync(
                    allocation.AssetType,
                    allocation.AssetId,
                    cancellationToken);

                driftDetails.Add(new AllocationDriftDetail
                {
                    AllocationId = allocation.Id,
                    ClientId = allocation.ClientId,
                    ClientName = allocation.Client.Name,
                    AssetType = allocation.AssetType,
                    AssetId = allocation.AssetId,
                    AssetIdentifier = assetIdentifier,
                    AllocationType = allocation.AllocationType,
                    TargetValue = targetValue,
                    CurrentValue = currentValue,
                    TargetPercentage = targetPercentage,
                    CurrentPercentage = currentPercentage,
                    DriftPercentage = driftPercentage,
                    DriftAmountUsd = driftAmountUsd,
                    Severity = severity,
                    RecommendedAction = recommendedAction
                });
            }

            // Order by drift percentage descending
            driftDetails = driftDetails.OrderByDescending(d => d.DriftPercentage).ToList();

            var result = new AllocationDriftDto
            {
                Drifts = driftDetails,
                TotalAllocations = allocations.Count,
                DriftsOverThreshold = driftDetails.Count(d => d.DriftPercentage > request.Threshold),
                AverageDriftPercentage = driftDetails.Any() ? driftDetails.Average(d => d.DriftPercentage) : 0,
                CalculatedAt = DateTime.UtcNow
            };

            return Result<AllocationDriftDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating allocation drift");
            return Result<AllocationDriftDto>.Failure("An error occurred while calculating allocation drift");
        }
    }

    private async Task<Dictionary<Guid, decimal>> GetWalletValuesAsync(CancellationToken cancellationToken)
    {
        return await _context.WalletBalances
            .AsNoTracking()
            .GroupBy(wb => wb.WalletId)
            .Select(g => new { WalletId = g.Key, Value = g.Sum(wb => wb.BalanceUsd ?? 0) })
            .ToDictionaryAsync(x => x.WalletId, x => x.Value, cancellationToken);
    }

    private async Task<Dictionary<Guid, decimal>> GetAccountValuesAsync(CancellationToken cancellationToken)
    {
        var accountBalances = await _context.AccountBalances
            .AsNoTracking()
            .Where(ab => ab.BalanceType == "AVAILABLE" || ab.BalanceType == "CURRENT")
            .GroupBy(ab => ab.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Balance = g.OrderByDescending(ab => ab.LastUpdated).First(),
            })
            .ToListAsync(cancellationToken);

        return accountBalances.ToDictionary(
            x => x.AccountId,
            x => x.Balance.Currency == "USD" ? x.Balance.Amount : x.Balance.Amount * BrlToUsdRate
        );
    }

    private async Task<decimal> GetClientTotalValueAsync(Guid clientId, CancellationToken cancellationToken)
    {
        // Get all allocations for client and sum their current values
        // Simplified calculation - in production would need more sophisticated logic
        var clientAllocations = await _context.ClientAssetAllocations
            .AsNoTracking()
            .Where(a => a.ClientId == clientId && a.EndDate == null)
            .ToListAsync(cancellationToken);

        var walletValues = await GetWalletValuesAsync(cancellationToken);
        var accountValues = await GetAccountValuesAsync(cancellationToken);

        decimal total = 0;
        foreach (var allocation in clientAllocations)
        {
            var assetValue = allocation.AssetType == "Wallet"
                ? walletValues.GetValueOrDefault(allocation.AssetId, 0)
                : accountValues.GetValueOrDefault(allocation.AssetId, 0);

            if (allocation.AllocationType == "Percentage")
            {
                total += assetValue * (allocation.AllocationValue / 100);
            }
            else
            {
                total += Math.Min(allocation.AllocationValue, assetValue);
            }
        }

        return total;
    }

    private async Task<string> GetAssetIdentifierAsync(
        string assetType,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        if (assetType == "Wallet")
        {
            var wallet = await _context.CustodyWallets
                .AsNoTracking()
                .Where(w => w.Id == assetId)
                .Select(w => new { w.WalletAddress, w.Label })
                .FirstOrDefaultAsync(cancellationToken);

            return wallet?.Label ?? wallet?.WalletAddress ?? assetId.ToString();
        }
        else
        {
            var account = await _context.TraditionalAccounts
                .AsNoTracking()
                .Where(a => a.Id == assetId)
                .Select(a => new { a.Label, a.AccountNumber })
                .FirstOrDefaultAsync(cancellationToken);

            return account?.Label ?? account?.AccountNumber ?? assetId.ToString();
        }
    }
}
