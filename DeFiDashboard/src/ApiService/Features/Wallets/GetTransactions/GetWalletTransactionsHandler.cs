using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Wallets.GetTransactions;

public class GetWalletTransactionsHandler : IRequestHandler<GetWalletTransactionsQuery, Result<PagedResult<WalletTransactionDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetWalletTransactionsHandler> _logger;

    public GetWalletTransactionsHandler(ApplicationDbContext context, ILogger<GetWalletTransactionsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<WalletTransactionDto>>> Handle(
        GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var walletExists = await _context.CustodyWallets
                .AnyAsync(w => w.Id == request.WalletId, cancellationToken);

            if (!walletExists)
            {
                return Result<PagedResult<WalletTransactionDto>>.Failure("Wallet not found");
            }

            var query = _context.Transactions
                .AsNoTracking()
                .Where(t => t.TransactionType == "Wallet" && t.AssetId == request.WalletId);

            // Apply filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= request.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(request.Direction))
            {
                query = query.Where(t => t.Direction == request.Direction);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    TransactionHash = t.TransactionHash,
                    Chain = t.Chain,
                    Direction = t.Direction,
                    FromAddress = t.FromAddress,
                    ToAddress = t.ToAddress,
                    TokenSymbol = t.TokenSymbol,
                    Amount = t.Amount,
                    AmountUsd = t.AmountUsd,
                    Fee = t.Fee,
                    FeeUsd = t.FeeUsd,
                    Description = t.Description,
                    Category = t.Category,
                    TransactionDate = t.TransactionDate,
                    Status = t.Status
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WalletTransactionDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<WalletTransactionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for wallet {WalletId}", request.WalletId);
            return Result<PagedResult<WalletTransactionDto>>.Failure("An error occurred while retrieving wallet transactions");
        }
    }
}
