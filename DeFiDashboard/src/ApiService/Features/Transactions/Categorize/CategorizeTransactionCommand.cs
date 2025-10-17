using ApiService.Features.Clients.Create;
using MediatR;

namespace ApiService.Features.Transactions.Categorize;

public record CategorizeTransactionCommand(
    Guid Id,
    string? Category
) : IRequest<Result<bool>>;
