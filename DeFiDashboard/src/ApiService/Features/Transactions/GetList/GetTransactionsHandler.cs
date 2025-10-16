using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.GetList;

public class GetTransactionsHandler : IRequestHandler<GetTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetTransactionsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<TransactionDto>>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.TransactionType))
        {
            query = query.Where(t => t.TransactionType == request.TransactionType);
        }

        if (request.AssetId.HasValue)
        {
            query = query.Where(t => t.AssetId == request.AssetId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Direction))
        {
            query = query.Where(t => t.Direction == request.Direction);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TokenSymbol))
        {
            query = query.Where(t => t.TokenSymbol == request.TokenSymbol);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(t => t.Status == request.Status);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering (newest first)
        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.TransactionType,
                t.AssetId,
                t.TransactionHash,
                t.ExternalId,
                t.Chain,
                t.Direction,
                t.FromAddress,
                t.ToAddress,
                t.TokenSymbol,
                t.Amount,
                t.AmountUsd,
                t.Fee,
                t.FeeUsd,
                t.Description,
                t.Category,
                t.TransactionDate,
                t.IsManualEntry,
                t.Status,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<TransactionDto>(
            transactions,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PagedResult<TransactionDto>>.Success(pagedResult);
    }
}
