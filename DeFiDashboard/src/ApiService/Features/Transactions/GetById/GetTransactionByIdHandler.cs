using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using ApiService.Features.Transactions.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.GetById;

public class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDto>>
{
    private readonly ApplicationDbContext _context;

    public GetTransactionByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TransactionDto>> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Where(t => t.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null)
        {
            return Result<TransactionDto>.Failure("Transaction not found");
        }

        return Result<TransactionDto>.Success(transaction);
    }
}
