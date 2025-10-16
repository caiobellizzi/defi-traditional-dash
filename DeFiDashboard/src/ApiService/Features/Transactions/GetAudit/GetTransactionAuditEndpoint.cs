using Carter;
using MediatR;

namespace ApiService.Features.Transactions.GetAudit;

public class GetTransactionAuditEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions/{id:guid}/audit", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetTransactionAuditQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetTransactionAudit")
        .WithTags("Transactions")
        .WithOpenApi();
    }
}
