using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.GetAudit;

public class GetTransactionAuditHandler : IRequestHandler<GetTransactionAuditQuery, Result<IEnumerable<TransactionAuditDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetTransactionAuditHandler> _logger;

    public GetTransactionAuditHandler(
        ApplicationDbContext context,
        ILogger<GetTransactionAuditHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TransactionAuditDto>>> Handle(
        GetTransactionAuditQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify transaction exists
            var transactionExists = await _context.Transactions
                .AnyAsync(t => t.Id == request.TransactionId, cancellationToken);

            if (!transactionExists)
            {
                return Result<IEnumerable<TransactionAuditDto>>.Failure("Transaction not found");
            }

            var auditTrail = await _context.TransactionAudits
                .AsNoTracking()
                .Where(a => a.TransactionId == request.TransactionId)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => new TransactionAuditDto(
                    a.Id,
                    a.TransactionId,
                    a.Action,
                    a.ChangedBy,
                    a.ChangedAt,
                    a.OldData,
                    a.NewData,
                    a.Reason
                ))
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<TransactionAuditDto>>.Success(auditTrail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for transaction {TransactionId}", request.TransactionId);
            return Result<IEnumerable<TransactionAuditDto>>.Failure("An error occurred while retrieving the audit trail");
        }
    }
}
