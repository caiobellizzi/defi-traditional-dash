using ApiService.Features.Alerts.GetList;
using Carter;
using MediatR;

namespace ApiService.Features.Alerts.GetById;

public class GetAlertByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/alerts/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetAlertByIdQuery(id);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetAlertById")
        .WithTags("Alerts")
        .WithOpenApi()
        .Produces<AlertDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
