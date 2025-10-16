using System.Text.Json;
using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Transactions.GetAudit;

public record GetTransactionAuditQuery(
    Guid TransactionId
) : IRequest<Result<IEnumerable<TransactionAuditDto>>>;

public record TransactionAuditDto(
    Guid Id,
    Guid? TransactionId,
    string Action,
    Guid? ChangedBy,
    DateTime ChangedAt,
    JsonDocument? OldData,
    JsonDocument? NewData,
    string? Reason
);
