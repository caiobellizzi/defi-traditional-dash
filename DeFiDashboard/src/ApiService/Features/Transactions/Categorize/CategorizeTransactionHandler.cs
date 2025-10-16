using System.Text.Json;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.Categorize;

public class CategorizeTransactionHandler : IRequestHandler<CategorizeTransactionCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategorizeTransactionHandler> _logger;

    public CategorizeTransactionHandler(
        ApplicationDbContext context,
        ILogger<CategorizeTransactionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CategorizeTransactionCommand request,
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

            // Only update if category actually changed
            if (transaction.Category == request.Category)
            {
                return Result<bool>.Success(true);
            }

            var oldCategory = transaction.Category;
            transaction.Category = request.Category;
            transaction.UpdatedAt = DateTime.UtcNow;

            // Create audit entry for category change
            var auditEntry = new TransactionAudit
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Action = "UPDATE",
                ChangedBy = null, // Will be set when auth is implemented
                ChangedAt = DateTime.UtcNow,
                OldData = JsonDocument.Parse(JsonSerializer.Serialize(new { Category = oldCategory })),
                NewData = JsonDocument.Parse(JsonSerializer.Serialize(new { Category = request.Category })),
                Reason = "Category updated"
            };

            _context.TransactionAudits.Add(auditEntry);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated category for transaction {TransactionId} from '{OldCategory}' to '{NewCategory}'",
                transaction.Id, oldCategory, request.Category);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error categorizing transaction {TransactionId}", request.Id);
            return Result<bool>.Failure("An error occurred while categorizing the transaction");
        }
    }
}
