using Carter;
using MediatR;

namespace ApiService.Features.Transactions.GetById;

public class GetTransactionByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetTransactionByIdQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetTransactionById")
        .WithTags("Transactions")
        .WithOpenApi()
        .Produces<ApiService.Features.Transactions.GetList.TransactionDto>(200)
        .Produces(404);
    }
}
