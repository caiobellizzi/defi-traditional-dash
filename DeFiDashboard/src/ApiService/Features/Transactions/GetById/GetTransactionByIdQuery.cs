using ApiService.Features.Clients.Create;
using ApiService.Features.Transactions.GetList;
using MediatR;

namespace ApiService.Features.Transactions.GetById;

public record GetTransactionByIdQuery(Guid Id) : IRequest<Result<TransactionDto>>;
