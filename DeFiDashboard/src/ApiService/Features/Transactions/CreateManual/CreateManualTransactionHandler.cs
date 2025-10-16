using System.Text.Json;
using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.Services;
using ApiService.Common.Utilities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Transactions.CreateManual;

public class CreateManualTransactionHandler : IRequestHandler<CreateManualTransactionCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateManualTransactionHandler> _logger;

    public CreateManualTransactionHandler(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILogger<CreateManualTransactionHandler> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateManualTransactionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify asset exists
            if (request.TransactionType == "Wallet")
            {
                var walletExists = await _context.CustodyWallets
                    .AnyAsync(w => w.Id == request.AssetId, cancellationToken);

                if (!walletExists)
                {
                    return Result<Guid>.Failure("Wallet not found");
                }
            }
            else
            {
                var accountExists = await _context.TraditionalAccounts
                    .AnyAsync(a => a.Id == request.AssetId, cancellationToken);

                if (!accountExists)
                {
                    return Result<Guid>.Failure("Account not found");
                }
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionType = request.TransactionType,
                AssetId = request.AssetId,
                TransactionHash = request.TransactionHash,
                ExternalId = null,
                Chain = request.Chain,
                Direction = request.Direction,
                FromAddress = request.FromAddress,
                ToAddress = request.ToAddress,
                TokenSymbol = request.TokenSymbol,
                Amount = request.Amount,
                AmountUsd = request.AmountUsd,
                Fee = request.Fee,
                FeeUsd = request.FeeUsd,
                Description = InputSanitizer.Sanitize(request.Description),
                Category = request.Category,
                TransactionDate = request.TransactionDate,
                IsManualEntry = true,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Create audit entry
            var auditEntry = new TransactionAudit
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Action = "CREATE",
                ChangedBy = null, // Will be set when auth is implemented
                ChangedAt = DateTime.UtcNow,
                OldData = null,
                NewData = JsonDocument.Parse(JsonSerializer.Serialize(transaction)),
                Reason = InputSanitizer.Sanitize(request.Reason)
            };

            _context.TransactionAudits.Add(auditEntry);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created manual transaction {TransactionId} for {TransactionType} {AssetId}",
                transaction.Id, transaction.TransactionType, transaction.AssetId);

            // Notify connected clients about new transaction
            await _notificationService.NotifyNewTransactionAsync(
                transaction.AssetId,
                transaction.TransactionType,
                new
                {
                    transactionId = transaction.Id,
                    transactionHash = transaction.TransactionHash,
                    chain = transaction.Chain,
                    direction = transaction.Direction,
                    tokenSymbol = transaction.TokenSymbol,
                    amount = transaction.Amount,
                    amountUsd = transaction.AmountUsd,
                    transactionDate = transaction.TransactionDate,
                    category = transaction.Category,
                    description = transaction.Description,
                    isManualEntry = transaction.IsManualEntry
                });

            return Result<Guid>.Success(transaction.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manual transaction");
            return Result<Guid>.Failure("An error occurred while creating the transaction");
        }
    }
}
