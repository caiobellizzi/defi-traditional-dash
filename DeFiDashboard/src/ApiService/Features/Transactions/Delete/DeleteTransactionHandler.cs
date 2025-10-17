using System.Text.Json;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.Utilities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.Delete;

public class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteTransactionHandler> _logger;

    public DeleteTransactionHandler(
        ApplicationDbContext context,
        ILogger<DeleteTransactionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteTransactionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (transaction == null)
            {
                return Result<bool>.Failure("Transaction not found");
            }

            // Business Rule: Only manual entries can be deleted
            if (!transaction.IsManualEntry)
            {
                return Result<bool>.Failure("Only manual transactions can be deleted");
            }

            // Capture old data for audit trail
            var oldData = JsonDocument.Parse(JsonSerializer.Serialize(transaction));

            // Create audit entry before deletion
            var auditEntry = new TransactionAudit
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Action = "DELETE",
                ChangedBy = null, // Will be set when auth is implemented
                ChangedAt = DateTime.UtcNow,
                OldData = oldData,
                NewData = null,
                Reason = InputSanitizer.Sanitize(request.Reason)
            };

            _context.TransactionAudits.Add(auditEntry);
            _context.Transactions.Remove(transaction);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted transaction {TransactionId}",
                transaction.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the transaction");
        }
    }
}
