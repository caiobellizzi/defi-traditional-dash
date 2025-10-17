using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Transactions.Delete;

public record DeleteTransactionCommand(
    Guid Id,
    string? Reason
) : IRequest<Result<bool>>;
