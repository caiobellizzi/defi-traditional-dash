using Carter;
using MediatR;

namespace ApiService.Features.Clients.GetList;

public class GetClientsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/clients", async (
            ISender sender,
            string? status,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            var query = new GetClientsQuery(status, pageNumber, pageSize);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetClients")
        .WithTags("Clients")
        .WithOpenApi()
        .Produces<PagedResult<ClientDto>>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
