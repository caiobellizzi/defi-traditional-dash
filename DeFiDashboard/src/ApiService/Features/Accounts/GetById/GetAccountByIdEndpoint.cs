using Carter;
using MediatR;

namespace ApiService.Features.Accounts.GetById;

public class GetAccountByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAccountByIdQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetAccountById")
        .WithTags("Accounts")
        .WithOpenApi()
        .Produces<AccountDetailDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
