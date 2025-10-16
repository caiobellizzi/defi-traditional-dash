using System.Text.Json;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.Utilities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.Update;

public class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateTransactionHandler> _logger;

    public UpdateTransactionHandler(
        ApplicationDbContext context,
        ILogger<UpdateTransactionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateTransactionCommand request,
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

            // Business Rule: Only manual entries can be updated
            if (!transaction.IsManualEntry)
            {
                return Result<bool>.Failure("Only manual transactions can be updated");
            }

            // Capture old data for audit trail
            var oldData = JsonDocument.Parse(JsonSerializer.Serialize(transaction));

            // Update transaction
            transaction.TransactionHash = request.TransactionHash;
            transaction.Chain = request.Chain;
            transaction.Direction = request.Direction;
            transaction.FromAddress = request.FromAddress;
            transaction.ToAddress = request.ToAddress;
            transaction.TokenSymbol = request.TokenSymbol;
            transaction.Amount = request.Amount;
            transaction.AmountUsd = request.AmountUsd;
            transaction.Fee = request.Fee;
            transaction.FeeUsd = request.FeeUsd;
            transaction.Description = InputSanitizer.Sanitize(request.Description);
            transaction.Category = request.Category;
            transaction.TransactionDate = request.TransactionDate;
            transaction.UpdatedAt = DateTime.UtcNow;

            // Create audit entry
            var auditEntry = new TransactionAudit
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Action = "UPDATE",
                ChangedBy = null, // Will be set when auth is implemented
                ChangedAt = DateTime.UtcNow,
                OldData = oldData,
                NewData = JsonDocument.Parse(JsonSerializer.Serialize(transaction)),
                Reason = InputSanitizer.Sanitize(request.Reason)
            };

            _context.TransactionAudits.Add(auditEntry);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated transaction {TransactionId}",
                transaction.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId}", request.Id);
            return Result<bool>.Failure("An error occurred while updating the transaction");
        }
    }
}
